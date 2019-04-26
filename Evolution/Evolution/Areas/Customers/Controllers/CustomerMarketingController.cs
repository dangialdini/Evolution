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
    public class CustomerMarketingController : BaseController {
        // GET: Customers/CustomerMarkting
        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.Sales)]
        public ActionResult Index() {
            return CustomerMarketing(0);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.Sales)]
        public ActionResult CustomerMarketing(int id) {
            var model = createModel(id);
            return View("CustomerMarketing", model);
        }

        ViewModelBase createModel(int customerId) {
            var model = new CustomerMarketingListViewModel();
            var customer = CustomerService.FindCustomerModel(customerId, CurrentCompany);

            PrepareViewModel(model,
                             EvolutionResources.bnrCustomerMarketing + (customer == null ? "" : " - " + customer.Name),
                             customerId,
                             MakeMenuOptionFlags(customerId, 0, 0));
            model.ParentId = customerId;

            return model;
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.Sales)]
        public ActionResult GetCustomerMarketing(int index, int customerId, int pageNo, int pageSize, string search) {
            return Json(CustomerService.FindCustomerMarketingListModel(customerId, index, pageNo, pageSize), JsonRequestBehavior.AllowGet);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.Sales)]
        public ActionResult Edit(int id, int customerId) {
            var model = new EditCustomerMarketingViewModel();
            prepareEditModel(model, id, customerId);

            model.CustomerMarketing = CustomerService.FindCustomerMarketingModel(id, model.CurrentCompany, customerId);
            model.LGS = CustomerService.LockCustomerMarketing(model.CustomerMarketing);

            return View(model);
        }

        void prepareEditModel(EditCustomerMarketingViewModel model, int id, int customerId) {
            var customer = CustomerService.FindCustomerModel(customerId, CurrentCompany);

            string title = EvolutionResources.bnrAddEditCustomerMarketing + (customer == null ? "" : " - " + customer.Name);
            if (id <= 0) title += " - " + EvolutionResources.lblNewAddress;

            PrepareViewModel(model, title, customerId, MakeMenuOptionFlags(customerId, 0, 0));

            model.MarketingGroupList = LookupService.FindMarketingGroupsListItemModel(CurrentCompany);
            model.CustomerContactList = CustomerService.FindCustomerContactsListItemModel(customerId, true);
            model.ParentId = customerId;
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.Sales)]
        public ActionResult Delete(int index, int id) {
            var model = new CustomerMarketingListModel { GridIndex = index };

            try {
                CustomerService.DeleteCustomerMarketing(id);
            } catch (Exception e1) {
                model.Error.SetError(e1);
            }
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        [ValidateAntiForgeryToken]
        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.Sales)]
        public ActionResult Save(EditCustomerMarketingViewModel model, string command) {
            if (command.ToLower() == "save") {
                var modelError = CustomerService.InsertOrUpdateCustomerMarketing(model.CustomerMarketing, CurrentUser, model.LGS);
                if (modelError.IsError) {
                    prepareEditModel(model, model.CurrentCompany.Id, model.CustomerMarketing.CustomerId);
                    model.SetErrorOnField(ErrorIcon.Error,
                                          modelError.Message,
                                          "CustomerMarketing_" + modelError.FieldName);
                    return View("Edit", model);

                } else {
                    return RedirectToAction("CustomerMarketing", new { id = model.ParentId });
                }

            } else {
                return RedirectToAction("CustomerMarketing", new { id = model.ParentId });
            }
        }
    }
}