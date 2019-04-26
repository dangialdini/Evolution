using Evolution.Controllers.Application;
using Evolution.Enumerations;
using Evolution.Models.Models;
using Evolution.Models.ViewModels;
using Evolution.Resources;
using Evolution.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Web;
using System.Web.Mvc;

namespace Evolution.Areas.CompanyAdmin.Controllers
{
    public class LocationsController : BaseController {
        
        // GET: CompanyAdmin/Locations
        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.SuperUser)]
        public ActionResult Index() {
            var model = createModel();
            return View("Locations", model);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.SuperUser)]
        public ActionResult Locations() {
            var model = createModel();
            return View("Locations", model);
        }

        ViewModelBase createModel() {
            var model = new ViewModelBase();
            PrepareViewModel(model, EvolutionResources.bnrLocations);

            return model;
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.SuperUser)]
        public ActionResult GetLocations(int index, int pageNo, int pageSize, string search) {
            var model = createModel();
            return Json(LookupService.FindLocationsListModel(model.CurrentCompany.Id, index, pageNo, pageSize, search), JsonRequestBehavior.AllowGet);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.SuperUser)]
        public ActionResult Edit(int id) {
            var model = new EditLocationViewModel();
            PrepareViewModel(model, EvolutionResources.bnrAddEditLocation);

            model.Location = LookupService.FindLocationModel(id);
            model.Location.CompanyId = model.CurrentCompany.Id;
            model.LGS = LookupService.LockLocation(model.Location);

            return View(model);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.SuperUser)]
        public ActionResult Delete(int index, int id) {
            var model = new LocationListModel();
            model.GridIndex = index;
            try {
                LookupService.DeleteLocation(id);
            } catch (Exception e1) {
                model.Error.SetError(e1);
            }
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        [ValidateAntiForgeryToken]
        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.SuperUser)]
        public ActionResult Save(EditLocationViewModel model, string command) {
            if(command.ToLower() == "save") {
                var modelError = LookupService.InsertOrUpdateLocation(model.Location, CurrentUser, model.LGS);
                if (modelError.IsError) {
                    PrepareViewModel(model, EvolutionResources.bnrAddEditLocation);
                    model.SetErrorOnField(ErrorIcon.Error, modelError.Message, "Location_" + modelError.FieldName);
                    return View("Edit", model);
                } else {
                    return RedirectToAction("Locations");
                }
            } else {
                return RedirectToAction("Locations");
            }
        }
    }
}