using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Security.Permissions;
using Evolution.Security;
using Evolution.Controllers.Application;
using Evolution.Models.ViewModels;
using Evolution.Enumerations;
using Evolution.Models.Models;
using Evolution.Resources;

namespace Evolution.Areas.Sales.Controllers {
    public class MarketingGroupsController : BaseController {
        // GET: Marketing/Marketing
        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.SuperUser)]
        public ActionResult Index() {
            var model = createModel();
            return View("MarketingGroups", model);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.SuperUser)]
        public ActionResult MarketingGroups() {
            var model = createModel();
            return View("MarketingGroups", model);
        }

        ViewModelBase createModel() {
            var model = new ViewModelBase();
            PrepareViewModel(model, EvolutionResources.bnrMarketingGroups);

            return model;
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.SuperUser)]
        public ActionResult GetMarketingGroups(int index, int pageNo, int pageSize, string search) {
            var model = createModel();
            return Json(LookupService.FindMarketingGroupsListModel(model.CurrentCompany.Id, index, pageNo, pageSize, search), JsonRequestBehavior.AllowGet);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.SuperUser)]
        public ActionResult Edit(int id) {
            var model = new EditMarketingGroupViewModel();
            PrepareViewModel(model, EvolutionResources.bnrAddEditMarketingGroup);

            model.MarketingGroup = LookupService.FindMarketingGroupModel(id);
            model.LGS = LookupService.LockMarketingGroup(model.MarketingGroup);

            return View(model);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.SuperUser)]
        public ActionResult Delete(int index, int id) {
            var model = new MarketingGroupListModel();
            model.GridIndex = index;
            try {
                LookupService.DeleteMarketingGroup(id);
            } catch (Exception e1) {
                model.Error.SetError(e1);
            }
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        [ValidateAntiForgeryToken]
        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.SuperUser)]
        public ActionResult Save(EditMarketingGroupViewModel model, string command) {
            if (command.ToLower() == "save") {
                var modelError = LookupService.InsertOrUpdateMarketingGroup(model.MarketingGroup, CurrentUser, model.LGS);
                if (modelError.IsError) {
                    PrepareViewModel(model, EvolutionResources.bnrAddEditMarketingGroup);
                    model.SetErrorOnField(ErrorIcon.Error,
                                          modelError.Message,
                                          "MarketingGroup_" + modelError.FieldName);
                    return View("Edit", model);

                } else {
                    return RedirectToAction("MarketingGroups");
                }

            } else {
                return RedirectToAction("MarketingGroups");
            }
        }
    }
}
