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
    public class RegionsController : BaseController {
        // GET: CompanyAdmin/Regions
        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.SuperUser)]
        public ActionResult Index() {
            var model = createModel();
            return View("Regions", model);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.SuperUser)]
        public ActionResult Regions() {
            var model = createModel();
            return View("Regions", model);
        }

        ViewModelBase createModel() {
            var model = new ViewModelBase();
            PrepareViewModel(model, EvolutionResources.bnrRegions);

            return model;
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.SuperUser)]
        public ActionResult GetRegions(int index, int pageNo, int pageSize, string search) {
            var model = createModel();
            return Json(LookupService.FindRegionsListModel(model.CurrentCompany.Id, index, pageNo, pageSize, search), JsonRequestBehavior.AllowGet);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.SuperUser)]
        public ActionResult Edit(int id) {
            var model = new EditRegionViewModel();
            PrepareViewModel(model, EvolutionResources.bnrAddEditRegion);

            model.Region = LookupService.FindRegionModel(id);
            model.Region.CompanyId = model.CurrentCompany.Id;
            model.LGS = LookupService.LockRegion(model.Region);

            return View(model);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.SuperUser)]
        public ActionResult Delete(int index, int id) {
            var model = new RegionListModel();
            model.GridIndex = index;
            try {
                LookupService.DeleteRegion(id);
            } catch (Exception e1) {
                model.Error.SetError(e1);
            }
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        [ValidateAntiForgeryToken]
        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.SuperUser)]
        public ActionResult Save(EditRegionViewModel model, string command) {
            if (command.ToLower() == "save") {
                var modelError = LookupService.InsertOrUpdateRegion(model.Region, CurrentUser, model.LGS);
                if (modelError.IsError) {
                    PrepareViewModel(model, EvolutionResources.bnrAddEditRegion);
                    model.SetErrorOnField(ErrorIcon.Error,
                                          modelError.Message,
                                          "Region_" + modelError.FieldName);
                    return View("Edit", model);

                } else {
                    return RedirectToAction("Regions");
                }

            } else {
                return RedirectToAction("Regions");
            }
        }
    }
}
