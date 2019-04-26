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
    public class FreightCarriersController : BaseController {
        // GET: CompanyAdmin/FreightCarriers
        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.SuperUser)]
        public ActionResult Index() {
            return FreightCarriers();
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.SuperUser)]
        public ActionResult FreightCarriers() {
            var model = createModel();
            return View("FreightCarriers", model);
        }

        ViewModelBase createModel() { 
            //var model = new FreightCarrierListViewModel();
            var model = new ViewModelBase();
            PrepareViewModel(model, EvolutionResources.bnrFreightCarriers);

            return model;
        }

        [HttpGet]
        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.SuperUser)]
        public ActionResult GetFreightCarriers(int index, int pageNo, int pageSize, string search) {
            var model = createModel();
            return Json(LookupService.FindFreightCarriersListModel(model.CurrentCompany.Id, index, pageNo, pageSize, search), JsonRequestBehavior.AllowGet);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.SuperUser)]
        public ActionResult Edit(int id) {
            var model = new EditFreightCarrierViewModel();
            PrepareViewModel(model, EvolutionResources.bnrAddEditFreightCarrier);

            model.FreightCarrier = LookupService.FindFreightCarrierModel(id);
            model.FreightCarrier.CompanyId = model.CurrentCompany.Id;
            model.LGS = LookupService.LockFreightCarrier(model.FreightCarrier);

            return View(model);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.SuperUser)]
        public ActionResult Delete(int index, int id) {
            var model = new FreightCarrierListModel();
            model.GridIndex = index;
            try {
                LookupService.DeleteFreightCarrier(id);
            } catch (Exception e1) {
                model.Error.SetError(e1);
            }
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        [ValidateAntiForgeryToken]
        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.SuperUser)]
        public ActionResult Save(EditFreightCarrierViewModel model, string command) {
            if (command.ToLower() == "save") {
                var modelError = LookupService.InsertOrUpdateFreightCarrier(model.FreightCarrier, CurrentUser, model.LGS);
                if (modelError.IsError) {
                    PrepareViewModel(model, EvolutionResources.bnrAddEditFreightCarrier);
                    model.SetErrorOnField(ErrorIcon.Error,
                                          modelError.Message,
                                          "FreightCarrier_" + modelError.FieldName);
                    return View("Edit", model);

                } else {
                    return RedirectToAction("FreightCarriers");
                }

            } else {
                return RedirectToAction("FreightCarriers");
            }
        }
    }
}
