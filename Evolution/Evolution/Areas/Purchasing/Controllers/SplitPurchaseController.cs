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
    public class SplitPurchaseController : BaseController {
        // GET: Purchasing/SplitPurchase
        public ActionResult Index(int id) {
            return Split(id);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.AllPurchasing)]
        public ActionResult Split(int id) {
            // Called when a user clicks to split an order
            var model = createModel(id, true);
            return View("SplitPurchase", model);
        }

        PurchaseOrderSplitViewModel createModel(int id, bool bLock = false) {
            var model = new PurchaseOrderSplitViewModel();
            PrepareViewModel(model, EvolutionResources.bnrSplitPurchase, id);
            model.ParentId = id;

            model.OrderDetails.PurchaseOrderHeaderId = id;
            var poh = PurchasingService.FindPurchaseOrderHeaderModel(id, CurrentCompany);
            if(poh != null) {
                model.OrderDetails.SupplierName = poh.SupplierName;
                model.OrderDetails.OrderNumber = poh.OrderNumber.Value;
                if(poh.RequiredDate != null) model.OrderDetails.AdvertisedETA = poh.RequiredDate.Value.ToString(model.DisplayDateFormat);

                if(bLock) model.LGS = PurchasingService.LockPurchaseOrderHeader(poh);
            }

            model.OrderDetails.NewOrderAdvertisedETA = DateTimeOffset.Now;
            model.LocationList = LookupService.FindLocationListItemModel(CurrentCompany, true);
            model.PurchaseOrderList = PurchasingService.FindPurchaseOrderHeadersString(CurrentCompany, true);

            return model;
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.AllPurchasing)]
        public ActionResult GetSplitPurchaseDetails(int index, int parentId, int pageNo, int pageSize) {
            var model = createModel(parentId);
            return Json(PurchasingService.FindSplitPurchaseDetailsListModel(model.CurrentCompany, parentId, index, pageNo, pageSize), JsonRequestBehavior.AllowGet);
        }

        [ValidateAntiForgeryToken]
        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.AllPurchasing)]
        public ActionResult Save(PurchaseOrderSplitViewModel model, string command) {
            var modelError = new Error();

            if (command.ToLower() == "split") {
                // Count the number of items to check
                var numRows = 0;
                for(int i = 0; i < Request.Form.AllKeys.Count(); i++) {
                    if (Request.Form.AllKeys[i].IndexOf("txtSplitToNewOrderQty") != -1) numRows++;
                }

                for (int i = 0; i < numRows; i++) {
                    var newSplit = new SplitPurchaseItemModel();

                    newSplit.NewOrderQty = Request.Form[$"txtSplitToNewOrderQty{i}_4_0"].ParseInt();
                    newSplit.TargetOrderQty = Request.Form[$"txtTargetOrderQty{i}_5_0"].ParseInt();
                    newSplit.TargetOrderId = Request.Form[$"ddlTargetOrder{i}_6_0"].ParseInt();
                    newSplit.PurchaseOrderDetailId = Request.Form[$"hdnPodId{i}_6_1"].ParseInt();
                    newSplit.RowNumber = i;

                    if (newSplit.NewOrderQty > 0 || newSplit.TargetOrderQty > 0)
                        model.OrderDetails.SplitItems.Add(newSplit);
                }

                if (model.OrderDetails.SplitItems.Count() > 0) {
                    // Apply the splits
                    int updatedPOId = 0,
                        newPOId = 0;
                    modelError = PurchasingService.SplitOrder(CurrentCompany, model.OrderDetails, CurrentUser, model.LGS, ref updatedPOId, ref newPOId);
                    if (modelError.IsError) {
                        model = createModel(model.OrderDetails.PurchaseOrderHeaderId);
                        model.SetErrorOnField(ErrorIcon.Error,
                                                modelError.Message,
                                                "OrderDetails_" + modelError.FieldName,
                                                null, null, null, null,
                                                true);
                        return View("SplitPurchase", model);
                    }

                } else {
                    model = createModel(model.OrderDetails.PurchaseOrderHeaderId);
                    model.SetErrorOnField(ErrorIcon.Error,
                                          EvolutionResources.errNoItemsSelectedToSplit,
                                          "OrderDetails_NewOrderAdvertisedETA",
                                          null, null, null, null,
                                          true);
                    return View("SplitPurchase", model);
                }
            }

            return RedirectToAction("Purchases", "Purchasing");
        }
    }
}
