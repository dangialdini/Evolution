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
    public class CustomerAddressesController : BaseController {
        // GET: Customers/CustomerAddresses
        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.Sales)]
        public ActionResult Index() {
            return CustomerAddresses(0);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.Sales)]
        public ActionResult CustomerAddresses(int id) {
            var model = createModel(id);
            return View("CustomerAddresses", model);
        }

        ViewModelBase createModel(int customerId) {
            var model = new CustomerAddressListViewModel();
            var customer = CustomerService.FindCustomerModel(customerId, CurrentCompany);

            PrepareViewModel(model,
                             EvolutionResources.bnrCustomerAddresses + (customer == null ? "" : " - " + customer.Name), 
                             customerId, 
                             MakeMenuOptionFlags(customerId, 0));
            model.ParentId = customerId;

            return model;
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.Sales)]
        public ActionResult GetCustomerAddresses(int index, int customerId, int pageNo, int pageSize, string search) {
            return Json(CustomerService.FindCustomerAddressesListModel(customerId, index, pageNo, pageSize, search), JsonRequestBehavior.AllowGet);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.Sales)]
        public ActionResult Edit(int id, int customerId) {
            var model = new EditCustomerAddressViewModel();
            prepareEditModel(model, id, customerId);

            model.CustomerAddress = CustomerService.FindCustomerAddressModel(id, model.CurrentCompany, customerId);
            model.LGS = CustomerService.LockCustomerAddress(model.CustomerAddress);

            return View(model);
        }

        void prepareEditModel(EditCustomerAddressViewModel model, int id, int customerId) {
            var customer = CustomerService.FindCustomerModel(customerId, CurrentCompany);

            string title = EvolutionResources.bnrAddEditCustomerAddress + (customer == null ? "" : " - " + customer.Name);
            if (id <= 0) title += " - " + EvolutionResources.lblNewAddress;

            PrepareViewModel(model, title, customerId, MakeMenuOptionFlags(customerId, 0));

            model.AddressTypeList = LookupService.FindLOVItemsListItemModel(null, LOVName.AddressType);
            model.CountryList = LookupService.FindCountriesListItemModel();
            model.ParentId = customerId;
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.Sales)]
        public ActionResult Delete(int index, int id) {
            var model = new CustomerAddressListModel();
            model.GridIndex = index;

            try {
                CustomerService.DeleteCustomerAddress(id);
            } catch (Exception e1) {
                model.Error.SetError(e1);
            }
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        [ValidateAntiForgeryToken]
        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.Sales)]
        public ActionResult Save(EditCustomerAddressViewModel model, string command) {
            if (command.ToLower() == "save") {
                var modelError = CustomerService.InsertOrUpdateCustomerAddress(model.CustomerAddress, model.LGS);
                if (modelError.IsError) {
                    prepareEditModel(model, model.CurrentCompany.Id, model.CustomerAddress.CustomerId.Value);
                    model.SetErrorOnField(ErrorIcon.Error,
                                          modelError.Message,
                                          "CustomerAddress_" + modelError.FieldName);
                    return View("Edit", model);

                } else {
                    return RedirectToAction("CustomerAddresses", new { id = model.ParentId });
                }

            } else {
                return RedirectToAction("CustomerAddresses", new { id = model.ParentId });
            }
        }
    }
}