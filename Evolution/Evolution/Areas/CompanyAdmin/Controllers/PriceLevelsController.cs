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

namespace Evolution.Areas.CompanyAdmin.Controllers {
    public class PriceLevelsController : BaseController {
        // GET: CompanyAdmin/PriceLevels
        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.SuperUser)]
        public ActionResult Index() {
            var model = createModel();
            return View("PriceLevels", model);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.SuperUser)]
        public ActionResult PriceLevels() {
            var model = createModel();
            return View("PriceLevels", model);
        }

        ViewModelBase createModel() {
            var model = new ViewModelBase();
            PrepareViewModel(model, EvolutionResources.bnrPriceLevels);

            return model;
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.SuperUser)]
        public ActionResult GetPriceLevels(int index, int pageNo, int pageSize, string search) {
            var model = createModel();
            return Json(LookupService.FindPriceLevelsListModel(model.CurrentCompany.Id, index, pageNo, pageSize, search), JsonRequestBehavior.AllowGet);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.SuperUser)]
        public ActionResult Edit(int id) {
            var model = new EditPriceLevelViewModel();
            PrepareViewModel(model, EvolutionResources.bnrAddEditPriceLevel);

            model.PriceLevel = LookupService.FindPriceLevelModel(id);
            model.PriceLevel.CompanyId = model.CurrentCompany.Id;
            model.LGS = LookupService.LockPriceLevel(model.PriceLevel);

            return View(model);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.SuperUser)]
        public ActionResult Delete(int index, int id) {
            var model = new PriceLevelListModel();
            model.GridIndex = index;
            try {
                LookupService.DeletePriceLevel(id);
            } catch (Exception e1) {
                model.Error.SetError(e1);
            }
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        [ValidateAntiForgeryToken]
        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.SuperUser)]
        public ActionResult Save(EditPriceLevelViewModel model, string command) {
            if (command.ToLower() == "save") {
                var modelError = LookupService.InsertOrUpdatePriceLevel(model.PriceLevel, CurrentUser, model.LGS);
                if (modelError.IsError) {
                    PrepareViewModel(model, EvolutionResources.bnrAddEditPriceLevel);
                    model.SetErrorOnField(ErrorIcon.Error,
                                          modelError.Message,
                                          "PriceLevel_" + modelError.FieldName);
                    return View("Edit", model);

                } else {
                    return RedirectToAction("PriceLevels");
                }

            } else {
                return RedirectToAction("PriceLevels");
            }
        }
    }
}
