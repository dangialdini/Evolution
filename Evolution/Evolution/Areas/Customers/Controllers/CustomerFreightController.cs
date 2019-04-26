using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Security.Permissions;
using Evolution.Security;
using Evolution.Controllers.Application;
using Evolution.Models.ViewModels;
using Evolution.CustomerService;
using Evolution.Enumerations;
using Evolution.Models.Models;
using Evolution.Resources;

namespace Evolution.Areas.Customers.Controllers
{
    public class CustomerFreightController : BaseController {
        // GET: Customers/CustomerFeight
        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.Sales)]
        public ActionResult Index() {
            return CustomerFreight(0);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.Sales)]
        public ActionResult CustomerFreight(int id) {
            var model = new EditCustomerFreightViewModel();
            prepareEditModel(model, id);

            model.CustomerFreight = CustomerService.FindCustomerFreightModel(id, CurrentCompany);
            model.LGS = CustomerService.LockCustomerFreight(model.CustomerFreight);

            return View(model);
        }

        void prepareEditModel(EditCustomerFreightViewModel model, int id) {
            PrepareViewModel(model, EvolutionResources.bnrCustomerFreight + (id > 0 ? " - CustId:" + id.ToString() : ""), id, MakeMenuOptionFlags(id, 0));

            model.FreightCarrierList = LookupService.FindFreightCarriersListItemModel(CurrentCompany.Id);
            model.FreightTermList = LookupService.FindLOVItemsListItemModel(CurrentCompany, LOVName.FreightTerm);
        }

        [ValidateAntiForgeryToken]
        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.Sales)]
        public ActionResult Save(EditCustomerFreightViewModel model, string command) {
            var modelError = CustomerService.InsertOrUpdateCustomerFreight(model.CustomerFreight, CurrentUser, model.LGS);
            if (modelError.IsError) {
                prepareEditModel(model, model.CustomerFreight.CustomerId);
                model.SetErrorOnField(ErrorIcon.Error,
                                        modelError.Message,
                                        "CustomerFreight_" + modelError.FieldName);
                return View("CustomerFreight", model);

            } else {
                prepareEditModel(model, model.CustomerFreight.CustomerId);
                model.SetErrorOnField(ErrorIcon.Information,
                                        EvolutionResources.infChangesSuccessfullySaved,
                                        "CustomerFreight_FreightCarrierId" + modelError.FieldName);
                return View("CustomerFreight", model);
            }
        }
    }
}
