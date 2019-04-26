using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Evolution.DAL;
using Evolution.Models.ViewModels;
using Evolution.Models.Models;
using Evolution.CommonService;
using Evolution.Extensions;
using Evolution.Enumerations;
using Evolution.Resources;
using Evolution.FileManagerService;

namespace Evolution.SalesService {
    public partial class SalesService {

        #region Private members

        private List<SalesOrderHeaderModel> sohList = new List<SalesOrderHeaderModel>();

        private string errorMsg = "";
        private int errorCount = 0;

        #endregion

        #region Public methods

        public Error CreatePicks(CompanyModel company, UserModel currentUser, string sohIds, bool bCombine) {
            var error = validateSingleOrder(company, sohIds);
            if (!error.IsError) {
                // TBD: Validate the document creation so that we don't have a rollback scenario

                // All sales validate, so drop them as individual drops
                List<PickHeaderModel> picks = new List<PickHeaderModel>();

                error = PickService.CreatePicks(company, sohList, bCombine, picks);
                // The returned list contains the names of the pick CSV temp files.

                // Now create the corresponding support PDF document when
                // the location configuration specifies that it should be created.
                if(!error.IsError) error = createDocuments(company, picks);

                // Now drop the files to the warehouse
                if (!error.IsError) {
                    var sohIdList = new List<int>();

                    foreach (var pick in picks) {
                        error = FilePackagerService.SendPickToWarehouse(pick);
                        if (error.IsError) {
                            break;
                        } else {
                            // Pick successfully sent, so:
                            foreach (var detail in pick.PickDetails) {
                                var sod = FindSalesOrderDetailModel(detail.SalesOrderDetailId);
                                if (sod != null) {
                                    //  Change all the line items to 'sent for picking'
                                    sod.LineStatusId = (int)SalesOrderLineStatus.SentForPicking;
                                    InsertOrUpdateSalesOrderDetail(sod, "");

                                    //  Remove allocations from sale
                                    AllocationService.DeleteAllocationsForPurchaseLine(company, sod.Id);

                                    if (sohIdList.Where(l => l == sod.SalesOrderHeaderId).Count() == 0)
                                        sohIdList.Add(sod.SalesOrderHeaderId);
                                }
                            }

                            //  Set 'sent to warehouse date' on pick header
                            PickService.SetPickSentToWarehouseDate(pick, DateTimeOffset.Now);
                        }
                    }

                    // Attach notes to SOH's to indicate pick sent to W/House
                    TaskManagerService.TaskManagerService tm = new TaskManagerService.TaskManagerService(db, company);
                    foreach(var sohId in sohIdList) {
                        var soh = FindSalesOrderHeaderModel(sohId, company, false);
                        if (soh != null) {
                            var subject = "Order Sent to Warehouse";
                            var message = "Order sent to Warehouse for picking";
                            NoteService.AttachNoteToSalesOrder(soh, currentUser, subject, message);
                        }
                    }
                }

                // Cleanup all the temp pick files
                foreach(var pick in picks) {
                    foreach (var pickFile in pick.PickFiles) {
                        FileManagerService.FileManagerService.DeleteFile(pickFile);
                    }
                }

                if (!error.IsError) error.SetInfo(EvolutionResources.infPicksSuccessfullyCreated);
            }

            return error;
        }

        #endregion

        #region Private methods

        private Error createDocuments(CompanyModel company, List<PickHeaderModel> picks) {
            var error = new Error();

            var pick = picks.FirstOrDefault();      // Validation ensures that there will always be at least one pick

            var location = LookupService.FindLocationModel(pick.LocationId.Value, false);
            if(location != null && location.LocationIdentification.ToLower() == "warehouse") {
                // Warehouse requires supporting PDF document

                // TBD: The type of PDF is specified in the customer record
                // Combined orders can only be for the same customer, so there will only
                // ever be one customer, irrespective of single or combined picks.
                var customer = db.FindCustomer(pick.CustomerId ?? 0);
                if (customer == null) {
                    error.SetRecordError("Customer", pick.CustomerId ?? 0);
                } else {
                    var template = LookupService.FindDocumentTemplateModel(customer.ShippingTemplateId ?? 0);

                    var config = DataTransferService.FindFileTransferConfigurationForWarehouseModel(location, FileTransferDataType.WarehousePick);
                    if (config == null) {
                        error.SetError(EvolutionResources.errCannotDropOrderNoDataTransfer, pick.Id.ToString(), location.LocationName);

                    } else {
                        var pdfFile = MediaServices.GetMediaFolder(MediaFolder.Temp, company.Id) +
                                                DataTransferService.GetTargetFileName(config, "", pick.Id).ChangeExtension(".pdf");
                        string outputFile = "";
                        error = CreatePickDocumentPdf(pick, template, pdfFile, false, ref outputFile, int.MaxValue);
                        pick.PickFiles.Add(outputFile);

                        // TBD: Add the document to the sale notes/attachments
                    }
                }
            }

            return error;
        }

        private Error validateSingleOrder(CompanyModel company, string sohIds) {
            var error = new Error();

            if (string.IsNullOrEmpty(sohIds.Trim())) {
                error.SetError(EvolutionResources.errNoSalesWereSelectedForPicking);

            } else {
                string[] sohIdList = sohIds.Split(',');

                // The following rules apply to a single order:
                //  *Basic validation customer, address, billing, next state, MSQ
                foreach (var sohItem in sohIdList) {
                    int sohId = sohItem.ParseInt();
                    var soh = FindSalesOrderHeaderModel(sohId, company, false, true);
                    if (soh == null) {
                        error.SetRecordError("SalesOrderHeader", sohId);

                    } else {
                        error = validateOrderBasics(company, soh);
                        if(!error.IsError) sohList.Add(soh);
                    }
                }
            }
            return error;
        }

        private Error validateCombinedOrders(CompanyModel company, string sohIds) {
            var error = new Error();

            if (string.IsNullOrEmpty(sohIds.Trim())) {
                error.SetError(EvolutionResources.errNoSalesWereSelectedForPicking);

            } else {
                string[] sohIdList = sohIds.Split(',');

                // The following rules apply to combining orders:
                //  *Basic validation customer, address, billing, next state, MSQ
                //  *All orders must use the same carriers
                //   Orders must contain items with the same pack sizes
                //  *Orders must all be wholesale ie retail orders cannot be combined
                //  *Orders must all use the same credit card
                //  *Manual and auto freight mismatches cannot be combined
                //  *All orders must be from the same customer at the same delivery address
                //  *Replacement orders cannot be combined with regular orders
                foreach (var sohItem in sohIdList) {
                    int sohId = sohItem.ParseInt();
                    var soh = FindSalesOrderHeaderModel(sohId, company, false, true);
                    if (soh == null) {
                        error.SetRecordError("SalesOrderHeader", sohId);

                    } else {
                        error = validateOrderBasics(company, soh);
                    }
                    if (error.IsError) {
                        break;
                    } else {
                        sohList.Add(soh);
                    }
                }

                if (!error.IsError) {
                    // When we reach here, all orders have met basic validation.
                    // Now check the business rule validations.
                    bool    bFirst = true;

                    int     lastFreightCarrierId = -1,
                            lastCreditCardId = -1,
                            lastCustomerId = -1;
                    bool    isManualFreight = false,
                            isCCReplacementOrder = false;
                    string  lastDeliveryAddrs = "";

                    errorMsg = "";
                    errorCount = 0;

                    foreach (var soh in sohList) {
                        if(bFirst) {
                            // Freight carrier
                            lastFreightCarrierId = soh.FreightCarrierId.Value;

                            //  TBD: Orders must contain items with the same pack sizes
                            
                            //  Orders must all be wholesale ie retail orders cannot be combined
                            if(soh.IsRetailSale) {
                                addError(EvolutionResources.errCannotCombineRetailOrder.Replace("%1", soh.OrderNumber.ToString())
                                                                                       .Replace("%2", soh.CustomerName));
                            }

                            //  Orders must all use the same credit card
                            lastCreditCardId = soh.CreditCardId.Value;

                            //  Manual and auto freight mismatches cannot be combined
                            isManualFreight = soh.IsManualFreight;

                            //  All orders must be from the same customer at the same delivery address
                            lastCustomerId = soh.CustomerId.Value;
                            lastDeliveryAddrs = soh.ShipAddress1.Trim() + 
                                                soh.ShipAddress2.Trim() +
                                                soh.ShipAddress3.Trim() +
                                                soh.ShipAddress4.Trim() +
                                                soh.ShipSuburb.Trim() +
                                                soh.ShipState.Trim() +
                                                soh.ShipPostcode.Trim() +
                                                soh.ShipCountryId.ToString();
                            if(lastDeliveryAddrs.Length < 40) {
                                addError(EvolutionResources.errCannotDropOrderShipAddrsIncomplete.Replace("%1", soh.OrderNumber.ToString())
                                                                                                 .Replace("%2", soh.CustomerName));
                            }

                            //  Replacement orders cannot be combined with regular orders
                            isCCReplacementOrder = IsCreditClaimReplacementOrder(soh);

                            bFirst = false;

                        } else {
                            // Freight carrier
                            if(soh.FreightCarrierId != lastFreightCarrierId) {
                                addError(EvolutionResources.errCannotDropOrderWithDifferentFreightCarrier.Replace("%1", soh.OrderNumber.ToString())
                                                                                                         .Replace("%2", soh.CustomerName));
                            }

                            //  TBD: Orders must contain items with the same pack sizes

                            //  Orders must all be wholesale ie retail orders cannot be combined
                            if (soh.IsRetailSale) {
                                addError(EvolutionResources.errCannotCombineRetailOrder.Replace("%1", soh.OrderNumber.ToString())
                                                                                       .Replace("%2", soh.CustomerName));
                            }

                            //  Orders must all use the same credit card
                            if (soh.CreditCardId != lastCreditCardId) {
                                addError(EvolutionResources.errCannotDropOrderWithDifferentCreditCard.Replace("%1", soh.OrderNumber.ToString())
                                                                                                     .Replace("%2", soh.CustomerName));
                            }

                            //  Manual and auto freight mismatches cannot be combined
                            if(soh.IsManualFreight != isManualFreight) {
                                addError(EvolutionResources.errCannotDropOrderWithDifferentManualFreight.Replace("%1", soh.OrderNumber.ToString())
                                                                                                        .Replace("%2", soh.CustomerName));
                            }

                            //  All orders must be from the same customer at the same delivery address
                            if (soh.CustomerId != lastCustomerId) {
                                addError(EvolutionResources.errCannotDropOrderForDifferentCustomer.Replace("%1", soh.OrderNumber.ToString())
                                                                                                  .Replace("%2", soh.CustomerName));
                            }
                            string tempAddrs = soh.ShipAddress1.Trim() +
                                               soh.ShipAddress2.Trim() +
                                               soh.ShipAddress3.Trim() +
                                               soh.ShipAddress4.Trim() +
                                               soh.ShipSuburb.Trim() +
                                               soh.ShipState.Trim() +
                                               soh.ShipPostcode.Trim() +
                                               soh.ShipCountryId.ToString();
                            if (tempAddrs != lastDeliveryAddrs) {
                                addError(EvolutionResources.errCannotDropOrderShipAddrsDifferent.Replace("%1", soh.OrderNumber.ToString())
                                                                                                .Replace("%2", soh.CustomerName));
                            }

                            //  Replacement orders cannot be combined with regular orders
                            if (IsCreditClaimReplacementOrder(soh) != isCCReplacementOrder) {
                                addError(EvolutionResources.errCannotDropOrderMixedReplacements.Replace("%1", soh.OrderNumber.ToString())
                                                                                               .Replace("%2", soh.CustomerName));
                            }
                        }
                        if (errorCount >= 4) break;     // Stop on 4 orders so we don't create a large list of errors
                    }
                    if (errorCount > 0) error.SetError(errorMsg);
                }
            }
            return error;
        }

        private void addError(string str1) {
            if (!string.IsNullOrEmpty(errorMsg)) errorMsg += "<br/>";
            errorMsg += str1;
            errorCount++; ;
        }

        private Error validateOrderBasics(CompanyModel company, SalesOrderHeaderModel soh) {
            //  All orders must:
            //      Have a customer
            //      Have a valid/completed delivery address
            //      Have a valid/completed billing address
            //      Have a next action of 'Ship something'
            //      Have no items on the order which violate MSQ
            //      Have a sales person
            //      Have a freight carrier
            //      Have a credit card
            var error = new Error();
            int errorCount = 0;

            // Check for a customer
            var customer = CustomerService.FindCustomerModel(soh.CustomerId ?? 0, company, false);
            if (soh.CustomerId == null || customer == null) {
                error.SetError(EvolutionResources.errCannotDropOrderWithNoCustomer, "", soh.OrderNumber.ToString());

            } else {
                // Check for a valid billing address
                var billingAddrs = CustomerService.FindCustomerAddressesListModel(soh.CustomerId.Value)
                                                  .Items
                                                  .Where(addrs => addrs.AddressType == AddressType.Billing)
                                                  .FirstOrDefault();
                if (billingAddrs == null || !billingAddrs.IsValid) {
                    // Invalid billing address
                    error.SetError(EvolutionResources.errCannotDropOrderWithInvalidCustomerBillingAddress, "", soh.OrderNumber.ToString(), customer.Name);

                } else if (soh.NextActionId != (int)Enumerations.SaleNextAction.ShipSomething) {
                    // Invalid next action
                    error.SetError(EvolutionResources.errCannotDropOrderWhenNotShipSomething, "", soh.OrderNumber.ToString(), soh.CustomerName);

                } else if (soh.SalespersonId == null) {
                    // No sales person
                    error.SetError(EvolutionResources.errCannotDropOrderWhenNoSalesPerson, "", soh.OrderNumber.ToString(), soh.CustomerName);

                } else if (soh.FreightCarrierId == null) {
                    // No freight carrier
                    error.SetError(EvolutionResources.errCannotDropOrderWithNoFreightCarrier, "", soh.OrderNumber.ToString() , soh.CustomerName);

                } else if (soh.CreditCardId == null) {
                    // No credit card
                    error.SetError(EvolutionResources.errCannotDropOrderWithNoCreditCard, "", soh.OrderNumber.ToString(), soh.CustomerName);

                } else {
                    // Check for MSQ violations
                    var errorMsg = EvolutionResources.errCannotDropOrderWhenMSQNotMet;
                    foreach (var sod in FindSalesOrderDetailListModel(company, soh)) {
                        // Check the MSQ of each line
                        var product = ProductService.FindProductModel(sod.ProductId.Value, null, null, false);
                        if (sod.OrderQty < product.MinSaleQty) {
                            if (!string.IsNullOrEmpty(errorMsg)) errorMsg += "<br/>";
                            errorMsg += EvolutionResources.errCannotDropOrderWhenMSQNotMet
                                                          .Replace("%1", soh.OrderNumber.ToString())
                                                          .Replace("%2", soh.CustomerName)
                                                          .Replace("%3", product.ItemNumber)
                                                          .Replace("%4", product.MinSaleQty.ToString());

                            error.SetError(errorMsg);
                            errorCount++;
                            if (errorCount >= 4) break;
                        }
                    }
                }
            }
            return error;
        }

        #endregion
    }
}
