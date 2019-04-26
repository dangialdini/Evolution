using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Evolution.DAL;
using Evolution.Models.Models;
using Evolution.CommonService;
using Evolution.AuditService;
using Evolution.Enumerations;
using Evolution.Resources;

namespace Evolution.PurchasingService {
    public partial class PurchasingService {

        #region Public methods

        public Error CompleteOrder(CompanyModel company, UserModel user, int purchaseOrderHeaderTempId) {
            // On entry, the order has already been saved back to the main tables

            var error = new Error();

            // Set the status of the order to closed
            var poht = FindPurchaseOrderHeaderTempModel(purchaseOrderHeaderTempId, company, false);
            if (poht == null) {
                error.SetRecordError("PurchaseOrderHeaderTemp", purchaseOrderHeaderTempId);

            } else {
                // Check if the order is already completed
                var closedStatus = LookupService.FindPurchaseOrderHeaderStatusByValueModel(PurchaseOrderStatus.Closed).Id;
                if (poht.POStatus == closedStatus) {
                    error.SetError(EvolutionResources.errCantCloseAlreadyClosedPurchaseOrder);

                } else {
                    // Lock the record (forces others to re-read)
                    string lgs = LockPurchaseOrderHeaderTemp(poht);

                    poht.POStatus = LookupService.FindPurchaseOrderHeaderStatusByValueModel(PurchaseOrderStatus.Closed).Id;
                    poht.CompletedDate = DateTimeOffset.Now;

                    error = InsertOrUpdatePurchaseOrderHeaderTemp(poht, user, lgs);
                    if (!error.IsError) {
                        // Now get the main purchase order header
                        var poh = FindPurchaseOrderHeaderModel(poht.OriginalRowId, company, false);
                        if (poh != null) {
                            // Lock it for writing
                            lgs = LockPurchaseOrderHeader(poh);
                            poh.POStatus = poht.POStatus;
                            poh.CompletedDate = poht.CompletedDate;

                            error = InsertOrUpdatePurchaseOrderHeader(poh, user, lgs);
                            if (!error.IsError) {
                                // Move the allocations from received POs to existing stock
                                foreach (var allocation in db.FindAllocationsToPurchaseOrder(poh.Id)
                                                             .ToList()) {
                                    allocation.PurchaseLineId = null;
                                    db.InsertOrUpdateAllocation(allocation);
                                }

                                foreach(var orderLine in db.FindPurchaseOrderDetails(company.Id, poh.Id)
                                                           .ToList()) {
                                    if (orderLine.ProductId != null) {

                                        var product = ProductService.FindProductModel(orderLine.ProductId.Value, null, null, false);
                                        if (product != null) {
                                            // Set the QuantityOnHand in Product

                                            double qtyOnHand = (product.QuantityOnHand == null ? 0 : product.QuantityOnHand.Value);
                                            if (orderLine.OrderQty != null) {
                                                product.QuantityOnHand = qtyOnHand + orderLine.OrderQty.Value;

                                                lgs = ProductService.LockProduct(product);
                                                ProductService.InsertOrUpdateProduct(product, user, lgs);
                                            }

                                            // Set the QuantityOnHand in ItemLocation

                                            var prodLocation = ProductService.FindProductLocationModel(company, product, poh.LocationId.Value);
                                            if(prodLocation == null) {
                                                prodLocation = new ProductLocationModel {
                                                    CompanyId = company.Id,
                                                    ProductId = product.Id,
                                                    LocationId = poh.LocationId.Value,
                                                    QuantityOnHand = orderLine.OrderQty.Value,
                                                    SellOnOrder = 0,
                                                    PurchaseOnOrder = 0
                                                };
                                            } else {
                                                prodLocation.QuantityOnHand += orderLine.OrderQty.Value;
                                            }
                                            lgs = ProductService.LockProductLocation(prodLocation);
                                            ProductService.InsertOrUpdateProductLocation(prodLocation, user, lgs);
                                        }
                                    }
                                }

                                // Send the order to the Accounts system via the connector
                                AccountsConnector.AccountsConnector accountsConnector = new AccountsConnector.AccountsConnector(db);
                                error = accountsConnector.SendCompletedPurchaseOrderToAccounts(poh);
                            }

                        } else {
                            error.SetRecordError("PurchaseOrderHeader", poht.OriginalRowId);
                        }
                    }
                }
            }
            return error;
        }

        #endregion
    }
}
