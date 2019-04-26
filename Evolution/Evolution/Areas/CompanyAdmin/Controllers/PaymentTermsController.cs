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
    public class PaymentTermsController : BaseController {
        // GET: CompanyAdmin/PaymentTerms
        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.SuperUser)]
        public ActionResult Index() {
            var model = createModel();
            return View("PaymentTerms", model);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.SuperUser)]
        public ActionResult PaymentTerms() {
            var model = createModel();
            return View("PaymentTerms", model);
        }

        ViewModelBase createModel() {
            var model = new ViewModelBase();
            PrepareViewModel(model, EvolutionResources.bnrPaymentTerms);

            return model;
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.SuperUser)]
        public ActionResult GetPaymentTerms(int index, int pageNo, int pageSize, string search) {
            var model = createModel();
            return Json(LookupService.FindPaymentTermsListModel(model.CurrentCompany.Id, index, pageNo, pageSize), JsonRequestBehavior.AllowGet);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.SuperUser)]
        public ActionResult Edit(int id) {
            var model = new EditPaymentTermViewModel();
            PrepareViewModel(model, EvolutionResources.bnrAddEditPaymentTerm);

            model.PaymentTerm = LookupService.FindPaymentTermModel(id);
            model.PaymentTerm.CompanyId = model.CurrentCompany.Id;
            model.LGS = LookupService.LockPaymentTerm(model.PaymentTerm);

            return View(model);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.SuperUser)]
        public ActionResult Delete(int index, int id) {
            var model = new PaymentTermListModel();
            model.GridIndex = index;
            try {
                LookupService.DeletePaymentTerm(id);
            } catch (Exception e1) {
                model.Error.SetError(e1);
            }
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        [ValidateAntiForgeryToken]
        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.SuperUser)]
        public ActionResult Save(EditPaymentTermViewModel model, string command) {
            if (command.ToLower() == "save") {
                var modelError = LookupService.InsertOrUpdatePaymentTerm(model.PaymentTerm, CurrentUser, model.LGS);
                if (modelError.IsError) {
                    PrepareViewModel(model, EvolutionResources.bnrAddEditPaymentTerm);
                    model.SetErrorOnField(ErrorIcon.Error,
                                          modelError.Message,
                                          "PaymentTerm_" + modelError.FieldName);
                    return View("Edit", model);

                } else {
                    return RedirectToAction("PaymentTerms");
                }

            } else {
                return RedirectToAction("PaymentTerms");
            }
        }
    }
}
