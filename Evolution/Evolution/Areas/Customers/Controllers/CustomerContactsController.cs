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

namespace Evolution.Areas.Customers.Controllers {
    public class CustomerContactsController : BaseController {
        // GET: Customers/CustomerContacts
        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.Sales)]
        public ActionResult Index() {
            return CustomerContacts(0);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.Sales)]
        public ActionResult CustomerContacts(int id) {
            var model = createModel(id);
            return View("CustomerContacts", model);
        }

        ViewModelBase createModel(int customerId) {
            var model = new CustomerContactListViewModel();
            var customer = CustomerService.FindCustomerModel(customerId, CurrentCompany);

            PrepareViewModel(model,
                             EvolutionResources.bnrCustomerContacts + (customer == null ? "" : " - " + customer.Name),
                             customerId,
                             MakeMenuOptionFlags(customerId, 0));
            model.ParentId = customerId;

            return model;
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.Sales)]
        public ActionResult GetCustomerContacts(int index, int customerId, int pageNo, int pageSize, string search) {
            return Json(CustomerService.FindCustomerContactsListModel(customerId, index, pageNo, pageSize, search), JsonRequestBehavior.AllowGet);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.Sales)]
        public ActionResult Edit(int id, int customerId) {
            var model = new EditCustomerContactViewModel();
            prepareEditModel(model, id, customerId);

            model.CustomerContact = CustomerService.FindCustomerContactModel(id, model.CurrentCompany, customerId);

            model.LGS = CustomerService.LockCustomerContact(model.CustomerContact);

            return View(model);
        }

        void prepareEditModel(EditCustomerContactViewModel model, int id, int customerId) {
            var customer = CustomerService.FindCustomerModel(customerId, CurrentCompany);

            string title = EvolutionResources.bnrAddEditCustomerContact + (customer == null ? "" : " - " + customer.Name);
            if (id <= 0) title += " - " + EvolutionResources.lblNewContact;

            PrepareViewModel(model, title, customerId, MakeMenuOptionFlags(customerId, 0));

            model.SalutationList = LookupService.FindLOVItemsListItemModel(null, LOVName.Salutation);
            model.ParentId = customerId;
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.Sales)]
        public ActionResult Delete(int index, int id) {
            var model = new CustomerContactListModel();
            model.GridIndex = index;

            try {
                CustomerService.DeleteCustomerContact(id);
            } catch (Exception e1) {
                model.Error.SetError(e1);
            }
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        [ValidateAntiForgeryToken]
        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.Sales)]
        public ActionResult Save(EditCustomerContactViewModel model, string command) {
            if (command.ToLower() == "save") {
                var modelError = CustomerService.InsertOrUpdateCustomerContact(model.CustomerContact, CurrentUser, model.LGS);
                if (modelError.IsError) {
                    prepareEditModel(model, model.CurrentCompany.Id, model.CustomerContact.CustomerId.Value);
                    model.SetErrorOnField(ErrorIcon.Error,
                                          modelError.Message,
                                          "CustomerContact_" + modelError.FieldName);
                    return View("Edit", model);

                } else {
                    return RedirectToAction("CustomerContacts", new { id = model.ParentId });
                }

            } else {
                return RedirectToAction("CustomerContacts", new { id = model.ParentId });
            }
        }
    }
}
