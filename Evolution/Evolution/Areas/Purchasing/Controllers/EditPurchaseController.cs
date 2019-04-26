using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Security.Permissions;
using Evolution.Security;
using Evolution.Controllers.Application;
using Evolution.Models.Models;
using Evolution.Models.ViewModels;
using Evolution.Enumerations;
using Evolution.Extensions;
using Evolution.Resources;

namespace Evolution.Areas.Purchasing.Controllers {
    public class EditPurchaseController : BaseController {
        
        #region Add/Edit Purchase

        // GET: Purchasing/Edit
        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.AllPurchasing)]
        public ActionResult Add() {
            // Called when the user clicks 'Create' to create a new order
            var editModel = new EditPurchaseOrderHeaderTempViewModel();

            var purchase = PurchasingService.FindPurchaseOrderHeaderModel(0, CurrentCompany, true);
            purchase.OrderDate = DateTimeOffset.Now;
            purchase.SalespersonId = CurrentUser.Id;
            purchase.BrandCategoryId = CurrentUser.DefaultBrandCategoryId;
            purchase.LocationId = CurrentCompany.DefaultLocationID;
            purchase.CancelMessage = CurrentCompany.CancelMessage;

            // Copy the order into temp tables for editing
            editModel.PurchaseTemp = PurchasingService.CopyPurchaseOrderToTemp(CurrentCompany, purchase, CurrentUser);

            return EditDetails(editModel.PurchaseTemp.Id);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.AllPurchasing)]
        public ActionResult Edit(int id) {
            // Called when a user clicks to edit an order
            var model = new EditPurchaseOrderHeaderTempViewModel();

            var purchase = PurchasingService.FindPurchaseOrderHeaderModel(id, CurrentCompany, true);

            // Copy the order into temp tables for editing
            model.PurchaseTemp = PurchasingService.CopyPurchaseOrderToTemp(CurrentCompany, purchase, CurrentUser);
            prepareEditModel(model, model.PurchaseTemp.Id);

            model.LGS = PurchasingService.LockPurchaseOrderHeader(purchase);
            model.LGST = PurchasingService.LockPurchaseOrderHeaderTemp(model.PurchaseTemp);

            return View("Edit", model);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.AllPurchasing)]
        public ActionResult EditDetails(int id) {
            // Called when a user is on one of the order screens and clicks the order details option
            var model = new EditPurchaseOrderHeaderTempViewModel();

            // Use the data in the temp tables
            var purchaseTemp = PurchasingService.FindPurchaseOrderHeaderTempModel(id, CurrentCompany, true);
            purchaseTemp.UserId = CurrentUser.Id;

            model.PurchaseTemp = purchaseTemp;
            prepareEditModel(model, id);

            var purchase = PurchasingService.FindPurchaseOrderHeaderModel(purchaseTemp.OriginalRowId, CurrentCompany, true);

            model.LGS = PurchasingService.LockPurchaseOrderHeader(purchase);
            model.LGST = PurchasingService.LockPurchaseOrderHeaderTemp(model.PurchaseTemp);

            return View("Edit", model);
        }

        void prepareEditModel(EditPurchaseOrderHeaderTempViewModel model, int id) {
            var poht = PurchasingService.FindPurchaseOrderHeaderTempModel(id, CurrentCompany, false);

            PrepareViewModel(model, EvolutionResources.bnrAddEditPurchase + " - " + EvolutionResources.lblOrderNumber + ": " + model.PurchaseTemp.OrderNumber.ToString(), id, MakeMenuOptionFlags(0, poht.OriginalRowId, 0) + MenuOptionFlag.RequiresNewPurchase);
            model.ParentId = id;

            // Get the landing date from the Shipment record
            if (model.PurchaseTemp.OrderNumber != null) {
                var shipmentContent = ShipmentService.FindShipmentContentByPONoModel(CurrentCompany, model.PurchaseTemp.OrderNumber.Value);
                if (shipmentContent != null && shipmentContent.ShipmentId != null) {
                    var shipment = ShipmentService.FindShipmentModel(shipmentContent.ShipmentId.Value, CurrentCompany);
                }
            }

            model.LocationList = LookupService.FindLocationListItemModel(model.CurrentCompany);
            model.SupplierList = SupplierService.FindSupplierListItemModel(CurrentCompany);
            model.POStatusList = LookupService.FindPurchaseOrderHeaderStatusListItemModel();
            model.UserList = MembershipManagementService.FindUserListItemModel();
            model.PaymentTermsList = LookupService.FindPaymentTermsListItemModel(model.CurrentCompany);
            model.CommercialTermsList = LookupService.FindLOVItemsListItemModel(model.CurrentCompany, LOVName.CommercialTerms);
            model.PortList = LookupService.FindPortsListItemModel();
            model.ShipMethodList = LookupService.FindLOVItemsListItemModel(model.CurrentCompany, LOVName.ShippingMethod);
            model.ContainerTypeList = LookupService.FindContainerTypeListItemModel();
            model.FreightForwarderList = LookupService.FindFreightForwardersListItemModel(model.CurrentCompany);
            model.CurrencyList = LookupService.FindCurrenciesListItemModel();
            model.BrandCategoryList = ProductService.FindBrandCategoryListItemModel(model.CurrentCompany);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.AllPurchasing)]
        public ActionResult GetSupplier(int id) {
            var supplier = SupplierService.FindSupplierModel(id);
            return Json(supplier, JsonRequestBehavior.AllowGet);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.AllPurchasing)]
        public ActionResult GetAddress(int id) {
            var location = LookupService.FindLocationModel(id);
            var result = "";
            if (location != null) result = location.FullAddress;
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.AllPurchasing)]
        public ActionResult GetExchangeRate(int id) {
            var currency = LookupService.FindCurrencyModel(id);
            decimal result = 0;
            if (currency != null && currency.ExchangeRate != null) result = currency.ExchangeRate.Value;
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Order details list

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.AllPurchasing)]
        public ActionResult GetPurchaseOrderDetailTemps(int index, int parentId, int pageNo, int pageSize, string search,
                                                        string sortColumn, int sortOrder) {

            return Json(PurchasingService.FindPurchaseOrderDetailTempsListModel(CurrentCompany.Id, parentId, index, pageNo, pageSize, search,
                                                                                sortColumn, (SortOrder)sortOrder),
                        JsonRequestBehavior.AllowGet);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.AllPurchasing)]
        public ActionResult AddEditItem(int parentId, int id) {
            var model = new EditPurchaseOrderDetailTempViewModel();

            model.PurchaseOrderDetailTemp = PurchasingService.FindPurchaseOrderDetailTempModel(id, CurrentCompany, true);
            model.PurchaseOrderDetailTemp.PurchaseOrderHeaderTempId = parentId;
            prepareEditModel(model, parentId);

            model.LGS = PurchasingService.LockPurchaseOrderDetailTemp(model.PurchaseOrderDetailTemp);

            return View("AddEditItem", model);
        }

        private void prepareEditModel(EditPurchaseOrderDetailTempViewModel model, int parentId) {
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.AllPurchasing)]
        public ActionResult GetProducts(int brandCategoryId, string search) {
            var avail = LookupService.FindLOVItemByValueModel(LOVName.ProductAvailability, ((int)ProductAvailability.Live).ToString());
            return Json(ProductService.FindProductsForBrandCategoryListItemModel(brandCategoryId, (avail == null ? 0 : avail.Id), 0, 1, 100, search), 
                        JsonRequestBehavior.AllowGet);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.AllPurchasing)]
        public ActionResult GetProduct(string id) {
            var result = new JSONResultModel { Data = ProductService.FindProductModel(id, null, CurrentCompany, true) };
            if (result.Data == null) result.Error.SetError(EvolutionResources.errProductNotFound, "", "PurchaseOrderDetailTemp_ProductName");
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.AllPurchasing)]
        [ValidateAntiForgeryToken]
        public ActionResult DoAddItem(EditPurchaseOrderDetailTempViewModel model, string command) {
            if (command.ToLower() == "additem") {
                prepareEditModel(model, model.ParentId);
                if (ModelState.IsValid) {
                    // Add the item to the temp table
                    model.PurchaseOrderDetailTemp.CompanyId = CurrentCompany.Id;

                    var modelError = PurchasingService.InsertOrUpdatePurchaseOrderDetailTemp(model.PurchaseOrderDetailTemp, CurrentUser, model.LGS);
                    if (modelError.IsError) {
                        ModelState.AddModelError("PurchaseOrderDetailTemp_" + modelError.FieldName, modelError.Message);
                    }
                }
            }
            return View("AddEditItem", model);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.AllPurchasing)]
        public ActionResult GetOrderSummary(int id) {
            var poht = PurchasingService.FindPurchaseOrderHeaderTempModel(id, CurrentCompany);
            var model = PurchasingService.CreateOrderSummary(poht, CurrentUser.DateFormat);
            model.CurrencySymbol = LookupService.FindCurrencySymbol(CurrentCompany.DefaultCurrencyID);
            return View("OrderSummary", model);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.AllPurchasing)]
        public ActionResult Delete(int index, int id, int parentId) {
            var model = new PurchaseOrderDetailTempListModel();
            model.GridIndex = index;
            try {
                PurchasingService.DeletePurchaseOrderDetailTemp(id);
            } catch (Exception e1) {
                model.Error.SetError(e1);
            }
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Save Purchase Order

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.AllPurchasing)]
        public ActionResult SaveKeyProperties(int pohtId, int locationId, int supplierId, int brandcatgeoryId, string lgst) {
            var error = new Error();

            var poht = PurchasingService.FindPurchaseOrderHeaderTempModel(pohtId, CurrentCompany, false);
            if (poht != null) {
                poht.LocationId = locationId;
                poht.SupplierId = supplierId;
                poht.BrandCategoryId = brandcatgeoryId;
                error = PurchasingService.InsertOrUpdatePurchaseOrderHeaderTemp(poht, CurrentUser, lgst);
                if (!error.IsError) error.Data = PurchasingService.LockPurchaseOrderHeaderTemp(poht);
            }
            return Json(error, JsonRequestBehavior.AllowGet);
        }

        [ValidateAntiForgeryToken]
        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.AllPurchasing)]
        public ActionResult Save(EditPurchaseOrderHeaderTempViewModel model, string command) {
            switch (command.ToLower()) {
            case "save":
                // Save the screen data back to the temp tables, then copy to live tables and exit
                if (ModelState.IsValid) {
                    adjustDates(model.PurchaseTemp, model.TZ);

                    var modelError = PurchasingService.InsertOrUpdatePurchaseOrderHeaderTemp(model.PurchaseTemp, CurrentUser, model.LGST);
                    if (modelError.IsError) {
                        model.Error.SetError(modelError.Message,
                                             "Purchase_" + modelError.FieldName);

                    } else {
                        // Copy the temp tables back to the main tables
                        modelError = PurchasingService.CopyTempToPurchaseOrderHeader(model.PurchaseTemp.Id, CurrentUser, model.LGS);
                        if (modelError.IsError) {
                            prepareEditModel(model, model.PurchaseTemp.Id);
                            model.Error.SetError(modelError.Message,
                                                 "Purchase_" + modelError.FieldName);
                        } else {
                            return RedirectToAction("Purchases");
                        }
                    }
                }
                prepareEditModel(model, model.PurchaseTemp.Id);
                return View("Edit", model);

            default:
                return RedirectToAction("Index", "Purchasing", new { area = "Purchasing" });
            }
        }

        #endregion

        #region Send Order to Supplier/Warehouse/Freight forwarder

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.AllPurchasing)]
        public ActionResult SendToSupplier(int id) {    // PurchaseOrderTempId
            var model = new PurchaseOrderHeaderListModel();

            // Activate the process of sending the purchase to the supplier
            try {
                var poht = PurchasingService.FindPurchaseOrderHeaderTempModel(id, CurrentCompany);

                var error = PurchasingService.SendPurchaseOrderToSupplier(poht, CurrentUser, CurrentCompany);
                if (error.IsError) {
                    model.Error.SetError(error.Message);
                } else {
                    model.Error.SetInfo(EvolutionResources.infPurchaseOrderSentToSupplier);
                }
            } catch (Exception e1) {
                model.Error.Icon = ErrorIcon.Error;
                model.Error.Message = "Error: " + e1.Message;
            }
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.AllPurchasing)]
        public ActionResult SendToWarehouse(int id) {
            var model = new PurchaseOrderHeaderListModel();

            // Activate the process of sending the purchase to the warehouse
            try {
                var error = PurchasingService.SendPurchaseOrderToWarehouse(id);
                if (error.IsError) {
                    model.Error.SetError(error.Message);
                } else {
                    model.Error.SetInfo(EvolutionResources.infPurchaseOrderSentToWareHouse);
                }
            } catch (Exception e1) {
                model.Error.Icon = ErrorIcon.Error;
                model.Error.Message = "Error: " + e1.Message;
            }
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.AllPurchasing)]
        public ActionResult SendToFreightForwarder(int id) {
            var model = new PurchaseOrderHeaderListModel();

            // Activate the process of sending the purchase to the freight forwarder
            try {
                var error = PurchasingService.SendPurchaseOrderToFreightForwarder(id);
                if (error.IsError) {
                    model.Error.SetError(error.Message);
                } else {
                    model.Error.SetInfo(EvolutionResources.infPurchaseOrderSentToFreightForwarder);
                }
            } catch (Exception e1) {
                model.Error.SetError(e1);
            }
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Complete Order

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.AllPurchasing)]
        public ActionResult CompleteOrder(int id) {
            var model = new PurchaseOrderHeaderListModel();

            // Activate the process of Completing an order
            try {
                var error = PurchasingService.CompleteOrder(CurrentCompany, CurrentUser, id);
                if (error.IsError) {
                    model.Error.SetError(error.Message);
                } else {
                    model.Error.SetInfo(error.Message);
                }
            } catch (Exception e1) {
                model.Error.SetError(e1);
            }
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Private methods

        private void adjustDates(PurchaseOrderHeaderTempModel model, string tz) {
            model.OrderDate = GetFieldValue(model.OrderDate, tz);
            model.CancelDate = GetFieldValue(model.CancelDate, tz);
            model.RequiredDate = GetFieldValue(model.RequiredDate, tz);
            model.RealisticRequiredDate = GetFieldValue(model.RealisticRequiredDate, tz);
            model.CompletedDate = GetFieldValue(model.CompletedDate, tz);
            model.RequiredShipDate = GetFieldValue(model.RequiredShipDate, tz);
            model.RequiredShipDate_Original = GetFieldValue(model.RequiredShipDate_Original, tz);
            model.RequiredDate_Original = GetFieldValue(model.RequiredDate_Original, tz);
            model.DatePOSentToSupplier = GetFieldValue(model.DatePOSentToSupplier, tz);
            model.DateOrderConfirmed = GetFieldValue(model.DateOrderConfirmed, tz);
            model.SupplierInvoiceDate = GetFieldValue(model.SupplierInvoiceDate, tz);
        }

        #endregion
    }
}
