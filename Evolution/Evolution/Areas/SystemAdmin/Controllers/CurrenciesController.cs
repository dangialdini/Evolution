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
    public class CurrenciesController : BaseController
    {
        // GET: CompanyAdmin/Currencies
        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.SuperUser)]
        public ActionResult Index() {
            var model = createModel();
            return View("Currencies", model);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.SuperUser)]
        public ActionResult Currencies() {
            var model = createModel();
            return View("Currencies", model);
        }

        ViewModelBase createModel() {
            var model = new ViewModelBase();
            PrepareViewModel(model, EvolutionResources.bnrCurrencies);

            return model;
        }

        [HttpGet]
        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.SuperUser)]
        public ActionResult GetCurrencies(int index, int pageNo, int pageSize, string search) {
            return Json(LookupService.FindCurrenciesListModel(index, pageNo, pageSize, search), JsonRequestBehavior.AllowGet);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.SuperUser)]
        public ActionResult Edit(int id) {
            var model = new EditCurrencyViewModel();
            PrepareViewModel(model, EvolutionResources.bnrAddEditCurrency);

            model.Currency = LookupService.FindCurrencyModel(id);
            model.LGS = LookupService.LockCurrency(model.Currency);

            return View(model);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.SuperUser)]
        public ActionResult Delete(int index, int id) {
            var model = new CurrencyListModel();
            model.GridIndex = index;
            try {
                LookupService.DeleteCurrency(id);
            } catch (Exception e1) {
                model.Error.SetError(e1);
            }
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        [ValidateAntiForgeryToken]
        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.SuperUser)]
        public ActionResult Save(EditCurrencyViewModel model, string command) {
            if (command.ToLower() == "save") {
                var modelError = LookupService.InsertOrUpdateCurrency(model.Currency, CurrentUser, model.LGS);
                if (modelError.IsError) {
                    PrepareViewModel(model, EvolutionResources.bnrAddEditCurrency);
                    model.SetErrorOnField(ErrorIcon.Error,
                                          modelError.Message,
                                          "Currency_" + modelError.FieldName);
                    return View("Edit", model);

                } else {
                    return RedirectToAction("Currencies");
                }

            } else {
                return RedirectToAction("Currencies");
            }
        }
    }
}
