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

namespace Evolution.Areas.CompanyAdmin.Controllers
{
    public class TaxCodesController : BaseController
    {
        // GET: CompanyAdmin/TaxCodes
        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.SuperUser)]
        public ActionResult Index() {
            var model = createModel();
            return View("TaxCodes", model);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.SuperUser)]
        public ActionResult TaxCodes() {
            var model = createModel();
            return View("TaxCodes", model);
        }

        ViewModelBase createModel() {
            var model = new ViewModelBase();
            PrepareViewModel(model, EvolutionResources.bnrTaxCodes);

            return model;
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.SuperUser)]
        public ActionResult GetTaxCodes(int index, int pageNo, int pageSize, string search) {
            var model = createModel();
            return Json(LookupService.FindTaxCodesListModel(model.CurrentCompany.Id, index, pageNo, pageSize, search), JsonRequestBehavior.AllowGet);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.SuperUser)]
        public ActionResult Edit(int id) {
            var model = new EditTaxCodeViewModel();
            PrepareViewModel(model, EvolutionResources.bnrAddEditTaxCode);

            model.TaxCode = LookupService.FindTaxCodeModel(id);
            model.TaxCode.CompanyId = model.CurrentCompany.Id;
            model.LGS = LookupService.LockTaxCode(model.TaxCode);

            return View(model);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.SuperUser)]
        public ActionResult Delete(int index, int id) {
            var model = new TaxCodeListModel();
            model.GridIndex = index;
            try {
                LookupService.DeleteTaxCode(id);
            } catch (Exception e1) {
                model.Error.SetError(e1);
            }
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        [ValidateAntiForgeryToken]
        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.SuperUser)]
        public ActionResult Save(EditTaxCodeViewModel model, string command) {
            if (command.ToLower() == "save") {
                var modelError = LookupService.InsertOrUpdateTaxCode(model.TaxCode, CurrentUser, model.LGS);
                if (modelError.IsError) {
                    PrepareViewModel(model, EvolutionResources.bnrAddEditTaxCode);
                    model.SetErrorOnField(ErrorIcon.Error,
                                          modelError.Message,
                                          "TaxCode_" + modelError.FieldName);
                    return View("Edit", model);

                } else {
                    return RedirectToAction("TaxCodes");
                }

            } else {
                return RedirectToAction("TaxCodes");
            }
        }
    }
}
