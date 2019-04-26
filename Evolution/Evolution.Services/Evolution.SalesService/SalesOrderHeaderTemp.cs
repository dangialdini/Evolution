using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Evolution.DAL;
using Evolution.Models.Models;
using Evolution.CommonService;
using Evolution.Resources;
using Evolution.Enumerations;
using AutoMapper;

namespace Evolution.SalesService {
    public partial class SalesService {

        #region Public methods

        SalesOrderHeaderTempModel mapToModel(SalesOrderHeaderTemp item) {
            SalesOrderHeaderTempModel newItem = new SalesOrderHeaderTempModel();

            Mapper.Map(item, newItem);

            newItem.CustomerName = item.Customer.Name;

            if(item.SalesOrderHeaderStatu != null) { 
                newItem.SOStatusText = item.SalesOrderHeaderStatu.StatusName;
                newItem.SOStatusValue = (SalesOrderHeaderStatus)item.SalesOrderHeaderStatu.StatusValue;
            }
            if (item.SalesOrderHeaderSubStatu != null) {
                newItem.SOSubStatusText = item.SalesOrderHeaderSubStatu.StatusName;
            }
            if (item.LOVItem_Source != null) newItem.SourceText = item.LOVItem_Source.ItemText;

            newItem.SalesPersonName = db.MakeName(item.User_SalesPerson);

            newItem.NextActionText = (item.SaleNextAction == null ? "" : item.SaleNextAction.NextActionDescription);

            newItem.IsMSQOverridable = false;
            if (!item.IsOverrideMSQ &&
                item.SalesOrderHeaderSubStatu != null &&
                item.SalesOrderHeaderSubStatu.Id == (int)SalesOrderHeaderSubStatus.Unpicked) {
                newItem.IsMSQOverridable = true;
            }

            if (item.BrandCategory != null) newItem.BrandCategoryText = item.BrandCategory.CategoryName;

            return newItem;
        }

        public SalesOrderHeaderTempModel FindSalesOrderHeaderTempModel(int id, CompanyModel company, bool bCreateEmptyIfNotfound = true) {
            SalesOrderHeaderTempModel model = null;

            var p = db.FindSalesOrderHeaderTemp(id);
            if (p == null) {
                if (bCreateEmptyIfNotfound) model = new SalesOrderHeaderTempModel {
                    CompanyId = company.Id
                };

            } else {
                model = mapToModel(p);
            }

            return model;
        }

        public SalesOrderSummaryModel CreateOrderSummary(SalesOrderHeaderTempModel soht) {
            var model = new SalesOrderSummaryModel();
            model.SubTotal = 0;
            model.TaxName = "";
            model.TaxTotal = 0;
            model.Total = 0;
            model.Total = model.SubTotal + model.TaxTotal;
            model.TotalCbms = 0;

            // The tax code and currency comes from the customer
            double taxRate = 0;
            if(soht.CustomerId != null) { 
                var customer = db.FindCustomer(soht.CustomerId.Value);
                if(customer != null) {
                    var taxCode = db.FindTaxCode(customer.TaxCodeId);
                    if (taxCode != null) {
                        model.TaxName = taxCode.TaxCode1;
                        if(taxCode.TaxPercentageRate != null) taxRate = (double)taxCode.TaxPercentageRate.Value;
                    }
                    model.CurrencySymbol = LookupService.FindCurrencySymbol(customer.CurrencyId);
                }
            }

            // Now traverse all the items on the order
            foreach (var orderLine in FindSalesOrderDetailTempsListModel(soht.CompanyId, soht.Id, 0, 1, 9999, "").Items) {
                double linePrice = (double)(orderLine.LinePrice == null ? 0 : orderLine.LinePrice.Value);
                model.SubTotal += linePrice;

                double gstAmount = linePrice / 100 * taxRate;
                model.TaxTotal += gstAmount;

                if(orderLine.UnitCBM != null) model.TotalCbms += orderLine.UnitCBM.Value * (double)orderLine.OrderQty;
            }
            model.Total = model.SubTotal + model.TaxTotal;

            return model;
        }

        public Error InsertOrUpdateSalesOrderHeaderTemp(SalesOrderHeaderTempModel soht, UserModel user, string lockGuid, bool keyFieldsOnly) {
            var error = new Error();
            if(!keyFieldsOnly) error = validateModel(soht);
            if (!error.IsError) {
                // Check that the lock is still current
                if (!db.IsLockStillValid(typeof(SalesOrderHeaderTemp).ToString(), soht.Id, lockGuid)) {
                    error.SetError(EvolutionResources.errRecordChangedByAnotherUser, "OrderNumber");

                } else {
                    SalesOrderHeaderTemp temp = null;
                    if (soht.Id != 0) temp = db.FindSalesOrderHeaderTemp(soht.Id);
                    if (temp == null) temp = new SalesOrderHeaderTemp();

                    Mapper.Map<SalesOrderHeaderTempModel, SalesOrderHeaderTemp>(soht, temp);

                    db.InsertOrUpdateSalesOrderHeaderTemp(temp);
                    soht.Id = temp.Id;
                }
            }
            return error;
        }

        public string LockSalesOrderHeaderTemp(SalesOrderHeaderTempModel model) {
            return db.LockRecord(typeof(SalesOrderHeaderTemp).ToString(), model.Id);
        }

        #endregion

        #region Private methods

        private Error validateModel(SalesOrderHeaderTempModel model) {
            var error = new Error();
            if (model.CustomerId == null) error.SetError(EvolutionResources.errCustomerRequired, "CustomerId");
            if (!error.IsError) error = isValidNonRequiredString(getFieldValue(model.EndUserName), 52, "EndUserName", EvolutionResources.errTextDataRequiredInField);
            if (!error.IsError) error = isValidRequiredString(getFieldValue(model.ShipAddress1), 255, "ShipAddress1", EvolutionResources.errTextDataRequiredInField);
            if (!error.IsError) error = isValidNonRequiredString(getFieldValue(model.ShipAddress2), 255, "ShipAddress2", EvolutionResources.errTextDataRequiredInField);
            if (!error.IsError) error = isValidNonRequiredString(getFieldValue(model.ShipAddress3), 255, "ShipAddress3", EvolutionResources.errTextDataRequiredInField);
            if (!error.IsError) error = isValidNonRequiredString(getFieldValue(model.ShipAddress4), 255, "ShipAddress4", EvolutionResources.errTextDataRequiredInField);
            if (!error.IsError) error = isValidRequiredString(getFieldValue(model.ShipSuburb), 60, "ShipSuburb", EvolutionResources.errTextDataRequiredInField);
            if (!error.IsError) error = isValidRequiredString(getFieldValue(model.ShipState), 20, "ShipState", EvolutionResources.errTextDataRequiredInField);
            if (!error.IsError) error = isValidRequiredString(getFieldValue(model.ShipPostcode), 12, "ShipPostcode", EvolutionResources.errTextDataRequiredInField);
            if (!error.IsError) error = isValidNonRequiredString(getFieldValue(model.WarehouseInstructions), 100, "WarehouseInstructions", EvolutionResources.errTextDataRequiredInField);
            if (!error.IsError) error = isValidRequiredDate(model.DeliveryWindowOpen, "DeliveryWindowOpen", EvolutionResources.errDateRequiredInField);
            if (!error.IsError) error = isValidRequiredDate(model.DeliveryWindowClose, "DeliveryWindowClose", EvolutionResources.errDateRequiredInField);
            if (!error.IsError) error = isValidNonRequiredString(getFieldValue(model.DeliveryInstructions), 30, "DeliveryInstructions", EvolutionResources.errTextDataRequiredInField);
            if (!error.IsError) error = isValidNonRequiredString(getFieldValue(model.DeliveryContact), 30, "DeliveryContact", EvolutionResources.errTextDataRequiredInField);
            if (!error.IsError) error = isValidNonRequiredString(getFieldValue(model.CustPO), 255, "CustPO", EvolutionResources.errTextDataRequiredInField);
            if (!error.IsError) error = isValidNonRequiredString(getFieldValue(model.ShipMethodAccount), 25, "ShipMethodAccount", EvolutionResources.errTextDataRequiredInField);
            if (!error.IsError) error = isValidNonRequiredString(getFieldValue(model.EDI_Department), 30, "EDI_Department", EvolutionResources.errTextDataRequiredInField);
            if (!error.IsError) error = isValidNonRequiredString(getFieldValue(model.EDI_DCCode), 30, "EDI_DCCode", EvolutionResources.errTextDataRequiredInField);
            if (!error.IsError) error = isValidNonRequiredString(getFieldValue(model.EDI_StoreNo), 30, "EDI_StoreNo", EvolutionResources.errTextDataRequiredInField);

            if (!error.IsError && model.SOStatus != null) {
                var orderType = db.FindSalesOrderHeaderStatus(model.SOStatus.Value);
                if(orderType != null && orderType.StatusValue == (int)SalesOrderHeaderStatus.ConfirmedOrder) {
                    // A confirmed order must be signed
                    if(!model.IsConfirmedAddress) {
                        error.SetError(EvolutionResources.errAddressMustBeConfirmedForConfirmedOrder, "IsConfirmedAddress");
                    } else {
                        error = isValidRequiredString(getFieldValue(model.SignedBy), 30, "SignedBy", EvolutionResources.errTextDataRequiredInField);
                        if (!error.IsError) {
                            if(model.DateSigned == null) error.SetError(EvolutionResources.errDateRequiredInField, "DateSigned", "Date Signed");
                        }
                    }
                }
            }

            return error;
        }

        #endregion
    }
}
