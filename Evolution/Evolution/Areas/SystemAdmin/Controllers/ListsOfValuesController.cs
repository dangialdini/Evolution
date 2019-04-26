using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Security.Permissions;
using Evolution.Security;
using Evolution.LookupService;
using Evolution.Controllers.Application;
using Evolution.Models.ViewModels;
using Evolution.Enumerations;
using Evolution.Models.Models;
using Evolution.Resources;

namespace Evolution.Areas.SystemAdmin.Controllers {
    public class ListsOfValuesController : BaseController {

        // GET: SystemAdmin/ListsOfValues
        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.SuperUser)]
        public ActionResult Index() {
            return ListsOfValues();
        }
        
        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.SuperUser)]
        public ActionResult ListsOfValues(int lovId = 0) {
            var model = createModel(lovId);
            return View("ListsOfValues", model);
        }

        ListsOfValuesViewModel createModel(int lovId) {
            var model = new ListsOfValuesViewModel();
            PrepareViewModel(model, EvolutionResources.bnrListsOfValues);

            model.Lists = LookupService.FindLOVsModel(false);
            if (lovId != 0) {
                model.SelectedListId = lovId;
            } else {
                model.SelectedListId = (model.Lists.Count > 0 ? model.Lists[0].Id : 0);
            }

            return model;
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.SuperUser)]
        public ActionResult GetLOVs() {
            return Json(LookupService.FindLOVsListItemModel(false), JsonRequestBehavior.AllowGet);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.SuperUser)]
        public ActionResult GetLOVItems(int index, int lovId, int pageNo, int pageSize, string search) {
            var model = createModel(lovId);
            var selectedList = model.Lists
                                    .Where(ml => ml.Id == lovId)
                                    .FirstOrDefault();

            return Json(LookupService.FindLOVItemsModel(null, index, lovId, pageNo, pageSize, search, true), JsonRequestBehavior.AllowGet);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.SuperUser)]
        public ActionResult Edit(int id, int lovId) {
            var model = new EditLOVItemViewModel();
            PrepareViewModel(model, EvolutionResources.bnrAddEditLOVItem);

            model.LovItem = LookupService.FindLOVItemModel(id, lovId);
            model.LovItem.CompanyId = null;
            model.ColourList = LookupService.FindColourListItemModel();
            model.LGS = LookupService.LockLOVItem(model.LovItem);

            return View(model);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.SuperUser)]
        public ActionResult Delete(int index, int id, int lovId) {
            var model = new ListsOfValuesViewModel();
            model.GridIndex = index;
            try {
                LookupService.DeleteLOVItem(id);
            } catch (Exception e1) {
                model.Error.SetError(e1);
            }
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.SuperUser)]
        public ActionResult MoveUp(int index, int lovId, int id, int pageNo, int pageSize, string search) {
            var model = createModel(lovId);
            LookupService.MoveLOVItemUp(null, lovId, id);
            return GetLOVItems(index, lovId, pageNo, pageSize, search);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.SuperUser)]
        public ActionResult MoveDown(int index, int lovId, int id, int pageNo, int pageSize, string search) {
            var model = createModel(lovId);
            LookupService.MoveLOVItemDown(null, lovId, id);
            return GetLOVItems(index, lovId, pageNo, pageSize, search);
        }

        [ValidateAntiForgeryToken]
        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.SuperUser)]
        public ActionResult Save(EditLOVItemViewModel model, string command) {
            if (command.ToLower() == "save") {
                var modelError = LookupService.InsertOrUpdateLOVItem(model.LovItem, CurrentUser, model.LGS);
                if (modelError.IsError) {
                    PrepareViewModel(model, EvolutionResources.bnrAddEditLOVItem);
                    model.SetErrorOnField(ErrorIcon.Error,
                                          modelError.Message,
                                          "LOVItem_" + modelError.FieldName);
                    return View("Edit", model);

                } else {
                    return RedirectToAction("ListsOfValues", new { LovId = model.LovItem.LovId });
                }

            } else {
                return RedirectToAction("ListsOfValues", new { LovId = model.LovItem.LovId });
            }
        }
    }
}
