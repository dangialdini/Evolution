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
    public class PurchasingController : BaseController {

        #region Puchases list

        // GET: Purchasing/Purchasing
        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.AllPurchasing)]
        public ActionResult Index() {
            return Purchases();
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.AllPurchasing)]
        public ActionResult Purchases() {
            var model = createModel();
            return View("Purchases", model);
        }

        ViewModelBase createModel() {
            var model = new ViewModelBase();
            PrepareViewModel(model, EvolutionResources.bnrPurchases, 0, MenuOptionFlag.RequiresNoPurchase);
            return model;
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.AllPurchasing)]
        public ActionResult GetPOStatusList() {
            var model = createModel();
            var list = LookupService.FindPurchaseOrderHeaderStatusListItemModel();
            list.Insert(0, new ListItemModel { Id = "0", Text = EvolutionResources.lblNone });
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.AllPurchasing)]
        public ActionResult GetLocationList() {
            var model = createModel();
            var list = LookupService.FindLocationListItemModel(model.CurrentCompany);
            list.Insert(0, new ListItemModel { Id = "0", Text = EvolutionResources.lblNone });
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.AllPurchasing)]
        public ActionResult GetBrandCategoryList() {
            var model = createModel();
            var list = ProductService.FindBrandCategoryListItemModel(model.CurrentCompany);
            list.Insert(0, new ListItemModel { Id = "0", Text = EvolutionResources.lblNone });
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.AllPurchasing)]
        public ActionResult GetPurchases(int index, int pageNo, int pageSize, string search,
                                         int poStatus, int warehouse, int brandCategory,
                                         string sortColumn, int sortOrder) {
            var model = createModel();
            return Json(PurchasingService.FindPurchaseOrderHeadersListModel(model.CurrentCompany.Id, 
                                                                            index, 
                                                                            pageNo, 
                                                                            pageSize, 
                                                                            search,
                                                                            poStatus, 
                                                                            warehouse, 
                                                                            brandCategory,
                                                                            sortColumn, 
                                                                            (SortOrder)sortOrder),
                        JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Add to Shipment Popup

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.AllPurchasing)]
        public ActionResult AddToShipmentPopup() {
            var model = new AddToShipmentPopupViewModel();
            model.ShipmentList = ShipmentService.FindShipmentsListItemModel(CurrentCompany, true);
            return View("AddToShipmentPopup", model);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.AllPurchasing)]
        public ActionResult DoAddToShipment(AddToShipmentPopupViewModel model, string command) {
            if (command.ToLower() == "add") {
                // Find the shipment or create a new one
                var shipmentModel = ShipmentService.FindShipmentModel(model.ShipmentId, CurrentCompany, false);
                if (shipmentModel == null) {
                    // Create a new Shipment
                    shipmentModel = ShipmentService.CreateShipment(CurrentCompany, CurrentUser);
                }

                // Add the PO's to the shipment
                ShipmentService.AddPurchaseOrders(CurrentCompany, CurrentUser, shipmentModel, model.SelectedPOs);

                return RedirectToAction("Edit", "Shipments", new { area = "Shipments", id = shipmentModel.Id });

            } else {
                return Purchases();
            }
        }

        #endregion
    }
}
