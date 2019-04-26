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
    public class FreightForwardersController : BaseController {
        // GET: CompanyAdmin/FreightForwarders
        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.SuperUser)]
        public ActionResult Index() {
            return FreightForwarders();
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.SuperUser)]
        public ActionResult FreightForwarders() {
            var model = createModel();
            return View("FreightForwarders", model);
        }

        ViewModelBase createModel() {
            var model = new ViewModelBase();
            PrepareViewModel(model, EvolutionResources.bnrFreightForwarders);

            return model;
        }

        [HttpGet]
        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.SuperUser)]
        public ActionResult GetFreightForwarders(int index, int pageNo, int pageSize, string search) {
            var model = createModel();
            return Json(LookupService.FindFreightForwardersListModel(model.CurrentCompany.Id, index, pageNo, pageSize, search), JsonRequestBehavior.AllowGet);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.SuperUser)]
        public ActionResult Edit(int id) {
            var model = new EditFreightForwarderViewModel();
            PrepareViewModel(model, EvolutionResources.bnrAddEditFreightForwarder);

            model.FreightForwarder = LookupService.FindFreightForwarderModel(id);
            model.FreightForwarder.CompanyId = model.CurrentCompany.Id;
            model.LGS = LookupService.LockFreightForwarder(model.FreightForwarder);

            return View(model);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.SuperUser)]
        public ActionResult Delete(int index, int id) {
            var model = new FreightForwarderListModel();
            model.GridIndex = index;
            try {
                LookupService.DeleteFreightForwarder(id);
            } catch (Exception e1) {
                model.Error.SetError(e1);
            }
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        [ValidateAntiForgeryToken]
        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.SuperUser)]
        public ActionResult Save(EditFreightForwarderViewModel model, string command) {
            if (command.ToLower() == "save") {
                var modelError = LookupService.InsertOrUpdateFreightForwarder(model.FreightForwarder, CurrentUser, model.LGS);
                if (modelError.IsError) {
                    PrepareViewModel(model, EvolutionResources.bnrAddEditFreightForwarder);
                    model.SetErrorOnField(ErrorIcon.Error,
                                          modelError.Message,
                                          "FreightForwarder_" + modelError.FieldName);
                    return View("Edit", model);

                } else {
                    return RedirectToAction("FreightForwarders");
                }

            } else {
                return RedirectToAction("FreightForwarders");
            }
        }
    }
}
