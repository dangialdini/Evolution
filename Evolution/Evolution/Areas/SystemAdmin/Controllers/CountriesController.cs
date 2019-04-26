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

namespace Evolution.Areas.SystemAdmin.Controllers { 
    public class CountriesController : BaseController {
        // GET: CompanyAdmin/Countries
        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.SuperUser)]
        public ActionResult Index() {
            var model = createModel();
            return View("Countries", model);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.SuperUser)]
        public ActionResult Countries() {
            var model = createModel();
            return View("Countries", model);
        }

        ViewModelBase createModel() {
            var model = new ViewModelBase();
            PrepareViewModel(model, EvolutionResources.bnrCountries);

            return model;
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.SuperUser)]
        public ActionResult GetCountries(int index, int pageNo, int pageSize, string search) {
            return Json(LookupService.FindCountriesListModel(index, pageNo, pageSize, search), JsonRequestBehavior.AllowGet);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.SuperUser)]
        public ActionResult Edit(int id) {
            var model = new EditCountryViewModel();
            PrepareViewModel(model, EvolutionResources.bnrAddEditCountry);

            model.Country = LookupService.FindCountryModel(id);
            model.LGS = LookupService.LockCountry(model.Country);

            return View(model);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.SuperUser)]
        public ActionResult Delete(int index, int id) {
            var model = new CountryListModel();
            model.GridIndex = index;
            try {
                LookupService.DeleteCountry(id);
            } catch(Exception e1) {
                model.Error.SetError(e1);
            }
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        [ValidateAntiForgeryToken]
        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.SuperUser)]
        public ActionResult Save(EditCountryViewModel model, string command) {
            if (command.ToLower() == "save") {
                var modelError = LookupService.InsertOrUpdateCountry(model.Country, CurrentUser, model.LGS);
                if (modelError.IsError) {
                    PrepareViewModel(model, EvolutionResources.bnrAddEditCountry);
                    model.SetErrorOnField(ErrorIcon.Error,
                                          modelError.Message,
                                          "Country_" + modelError.FieldName);
                    return View("Edit", model);

                } else {
                    return RedirectToAction("Countries");
                }

            } else {
                return RedirectToAction("Countries");
            }
        }
    }
}
