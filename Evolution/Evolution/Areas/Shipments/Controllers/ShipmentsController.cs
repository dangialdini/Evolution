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

namespace Evolution.Areas.Shipments.Controllers
{
    public class ShipmentsController : BaseController {
        
        #region Shipment list

        // GET: Shipments/Shipments
        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.AllPurchasing)]
        public ActionResult Index() {
            return Shipments();
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.AllPurchasing)]
        public ActionResult Shipments() {
            var model = createModel();
            return View("Shipments", model);
        }

        ViewModelBase createModel() {
            var model = new ViewModelBase();
            PrepareViewModel(model, EvolutionResources.bnrShipments);
            return model;
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.AllPurchasing)]
        public ActionResult GetPurchaserList() {
            var model = createModel();
            var list = PurchasingService.FindPurchasersListItemModel(model.CurrentCompany);
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
        public ActionResult GetOrderStatusList() {
            var model = createModel();
            var list = LookupService.FindPurchaseOrderHeaderStatusListItemModel();
            list.Insert(0, new ListItemModel { Id = "0", Text = EvolutionResources.lblNone });
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.AllPurchasing)]
        public ActionResult GetOpenOrderList() {
            var list = new List<ListItemModel>();
            list.Add(new ListItemModel { Id = "0", Text = EvolutionResources.lblOpenOrders });
            list.Add(new ListItemModel { Id = "1", Text = EvolutionResources.lblAllOrders });
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.AllPurchasing)]
        public ActionResult GetShipments(int index, int pageNo, int pageSize, string search,
                                         int purchaserId, int brandCatId, int openorderid, int orderstatusid,
                                         string sortColumn, int sortOrder) {
            var model = createModel();
            return Json(ShipmentService.FindShipmentsListModel(model.CurrentCompany.Id,
                                                               index,
                                                               pageNo,
                                                               pageSize,
                                                               search,
                                                               purchaserId,
                                                               brandCatId,
                                                               openorderid,
                                                               orderstatusid,
                                                               sortColumn,
                                                               (SortOrder)sortOrder),
                        JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Add/Edit shipment details

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.AllPurchasing)]
        public ActionResult Add() {
            // Called when the user clicks 'Create' to create a new shipment
            var model = new EditShipmentViewModel();

            model.Shipment = ShipmentService.FindShipmentModel(0, CurrentCompany, true);
            prepareEditModel(model, model.Shipment);

            return View("Edit", model);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.AllPurchasing)]
        public ActionResult Edit(int id) {
            // Called when a user clicks to edit an order
            var model = new EditShipmentViewModel();

            model.Shipment = ShipmentService.FindShipmentModel(id, CurrentCompany, true);
            prepareEditModel(model, model.Shipment);

            model.LGS = ShipmentService.LockShipment(model.Shipment);

            return View("Edit", model);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.AllPurchasing)]
        public ActionResult GetShipmentDetails(int shipmentId, int index) {
            var model = createModel();
            return Json(ShipmentService.FindShipmentContentListModel(CurrentCompany, shipmentId, index),
                        JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Deleting a shipment

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.AllPurchasing)]
        public ActionResult Delete(int index, int? sci, int? si) {
            var model = new ShipmentListModel();
            model.GridIndex = index;
            try {
                if (sci != null) {
                    // Got a shipping content id to delete
                    ShipmentService.DeleteShipmentContent(sci.Value, true);
                } else if (si != null) {
                    // No shipping content id, but got a shipping id to delete
                    ShipmentService.DeleteShipment(si.Value);
                }
            } catch (Exception e1) {
                this.WriteLog(e1);
                model.Error.SetError(e1);
            }
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Saving Shipment details

        [ValidateAntiForgeryToken]
        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.AllPurchasing)]
        public ActionResult Save(EditShipmentViewModel model, string command) {
            if (command.ToLower() == "save") {
                // Save the screen data back to the db and exit
                adjustDates(model.Shipment, model.TZ);

                var modelError = ShipmentService.InsertOrUpdateShipment(model.Shipment, CurrentUser, model.LGS);
                if (modelError.IsError) {
                    prepareEditModel(model, model.Shipment);
                    model.SetErrorOnField(ErrorIcon.Error,
                                          modelError.Message,
                                          "Shipment_" + modelError.FieldName);
                    return View("Edit", model);

                } else {
                    return RedirectToAction("Shipments");
                }

            } else {
                return RedirectToAction("Shipments");
            }
        }

        private void adjustDates(ShipmentModel model, string tz) {
            model.Date100Shipped = GetFieldValue(model.Date100Shipped, tz);
            model.DatePreAlertETA = GetFieldValue(model.DatePreAlertETA, tz);
            model.DateExpDel = GetFieldValue(model.DateExpDel, tz);
            model.DateWarehouseAccept = GetFieldValue(model.DateWarehouseAccept, tz);
            model.DateUnpackSlipRcvd = GetFieldValue(model.DateUnpackSlipRcvd, tz);
        }

        #endregion

        #region Content Details list

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.AllPurchasing)]
        public ActionResult AddPurchaseOrderPopup() {
            // Called when the user clicks to add a purchase order to the content list
            var model = new ViewModelBase();
            return View("AddPurchaseOrderPopup", model);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.AllPurchasing)]
        public ActionResult DoAddPurchaseOrder(int si, string pos) {
            var model = new ShipmentListModel();
            try {
                var shipment = ShipmentService.FindShipmentModel(si, CurrentCompany, false);
                ShipmentService.AddPurchaseOrders(CurrentCompany, CurrentUser, shipment, pos);
            } catch (Exception e1) {
                this.WriteLog(e1);
                model.Error.SetError(e1);
            }
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.AllPurchasing)]
        public ActionResult EditContent(int shipmentId, int contentId) {
            // Called when a user clicks to edit an order - we
            // go straight to the lines screen
            var contentModel = ShipmentService.FindShipmentContentModel(contentId);
            if (contentModel != null) {
                return RedirectToAction("Edit", "Purchasing", new { area = "Purchasing", id = $"{contentModel.PurchaseOrderHeaderId}" });

            } else {
                var model = new EditShipmentViewModel();

                model.Shipment = ShipmentService.FindShipmentModel(shipmentId, CurrentCompany, true);
                prepareEditModel(model, model.Shipment);

                model.LGS = ShipmentService.LockShipment(model.Shipment);

                model.SetRecordError("ShipmentContent", contentId, true);

                return View("Edit", model);
            }
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.AllPurchasing)]
        public ActionResult DropContent(int index, int id) {
            // Called when a user clicks to drop a purchase order from the shipment content list
            var model = new ShipmentListModel();
            model.GridIndex = index;
            try {
                ShipmentService.DeleteShipmentContent(id, false);
            } catch (Exception e1) {
                this.WriteLog(e1);
                model.Error.SetError(e1);
            }
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        #endregion

        void prepareEditModel(EditShipmentViewModel model, ShipmentModel shipment) {
            PrepareViewModel(model, EvolutionResources.bnrAddEditShipment + " - " + EvolutionResources.lblShipment + ": " + model.Shipment.Id.ToString(), shipment.Id, MakeMenuOptionFlags(0, 0, shipment.Id));

            model.ShippingMethodList = LookupService.FindLOVItemsListItemModel(CurrentCompany, LOVName.ShippingMethod);
            model.CarrierVesselList = LookupService.FindCarrierVesselListItemModel();
            model.PortList = LookupService.FindPortsListItemModel();
            model.SeasonList = LookupService.FindLOVItemsListItemModel(CurrentCompany, LOVName.Season);
        }
    }
}
