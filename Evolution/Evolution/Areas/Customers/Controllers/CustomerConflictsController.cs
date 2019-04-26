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
using Evolution.Models.Models;
using Evolution.Extensions;
using Evolution.Enumerations;
using Evolution.Resources;

namespace Evolution.Areas.Customers.Controllers
{
    public class CustomerConflictsController : BaseController {
        // GET: Customers/CustomerConflicts
        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.Sales)]
        public ActionResult Index() {
            return CustomerConflicts(0);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.Sales)]
        public ActionResult CustomerConflicts(int id) {
            var model = createModel(id);
            return View("CustomerConflicts", model);
        }

        ViewModelBase createModel(int customerId) {
            var model = new CustomerConflictListViewModel();
            var customer = CustomerService.FindCustomerModel(customerId, CurrentCompany);

            PrepareViewModel(model,
                             EvolutionResources.bnrCustomerConflicts + (customer == null ? "" : " - " + customer.Name),
                             customerId,
                             MakeMenuOptionFlags(customerId, 0));
            model.ParentId = customerId;

            return model;
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.Sales)]
        public ActionResult GetCustomerConflicts(int index, int customerId, int pageNo, int pageSize, string search) {
            return Json(CustomerService.FindCustomerConflictsListModel(customerId, index, pageNo, pageSize), JsonRequestBehavior.AllowGet);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.Sales)]
        public ActionResult Edit(int id, int customerId) {
            var model = new EditCustomerConflictViewModel();
            prepareEditModel(model, id, customerId);

            model.CustomerConflict = CustomerService.FindCustomerConflictModel(id, model.CurrentCompany, customerId);

            model.LGS = CustomerService.LockCustomerConflict(model.CustomerConflict);

            return View(model);
        }

        void prepareEditModel(EditCustomerConflictViewModel model, int id, int customerId) {
            var customer = CustomerService.FindCustomerModel(customerId, CurrentCompany);

            string title = EvolutionResources.bnrAddEditCustomerConflict + (customer == null ? "" : " - " + customer.Name);
            if (id <= 0) title += " - " + EvolutionResources.lblNewConflict;

            PrepareViewModel(model, title, customerId, MakeMenuOptionFlags(customerId, 0));

            model.ParentId = customerId;
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.Sales)]
        public ActionResult Delete(int index, int id) {
            var model = new CustomerConflictListModel();
            model.GridIndex = index;

            try {
                CustomerService.DeleteCustomerConflict(id);
            } catch (Exception e1) {
                model.Error.SetError(e1);
            }
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        [ValidateAntiForgeryToken]
        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.Sales)]
        public ActionResult Save(EditCustomerConflictViewModel model, string command) {
            if (command.ToLower() == "addselected") {
                if(!string.IsNullOrEmpty(model.SelectedItems)) {
                    string[] items = model.SelectedItems.Split(',');
                    foreach(var item in items) { 
                        CustomerConflictModel temp = new CustomerConflictModel {
                            CompanyId = model.CustomerConflict.CompanyId,
                            CustomerId = model.CustomerConflict.CustomerId,
                            SensitiveWithId = item.ParseInt()
                        };

                        CustomerService.InsertOrUpdateCustomerConflict(temp, CurrentUser, model.LGS);
                    }
                }
            }
            return RedirectToAction("CustomerConflicts", new { id = model.ParentId });
        }
    }
}
