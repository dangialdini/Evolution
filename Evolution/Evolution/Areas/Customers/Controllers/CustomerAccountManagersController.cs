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
    public class CustomerAccountManagersController : BaseController {
        // GET: Customers/CustomerAccountManagers
        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.Sales)]
        public ActionResult Index() {
            return CustomerAccountManagers(0);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.Sales)]
        public ActionResult CustomerAccountManagers(int id) {
            var model = createModel(id);
            return View("CustomerAccountManagers", model);
        }

        ViewModelBase createModel(int customerId) {
            var model = new BrandCategorySalesPersonListViewModel();
            var customer = CustomerService.FindCustomerModel(customerId, CurrentCompany);

            PrepareViewModel(model,
                             EvolutionResources.bnrCustomerAccountManagers + (customer == null ? "" : " - " + customer.Name),
                             customerId,
                             MakeMenuOptionFlags(customerId, 0));
            model.ParentId = customerId;

            return model;
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.Sales)]
        public ActionResult GetCustomerAccountManagers(int index, int customerId, int pageNo, int pageSize, string search) {
            return Json(CustomerService.FindBrandCategorySalesPersonsListModel(customerId, index, pageNo, pageSize, search), JsonRequestBehavior.AllowGet);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.Sales)]
        public ActionResult Edit(int id, int customerId) {
            var model = new EditBrandCategorySalesPersonViewModel();
            prepareEditModel(model, id, customerId);

            model.CustomerAccountManager = CustomerService.FindBrandCategorySalesPersonModel(id, model.CurrentCompany, customerId);
            model.LGS = CustomerService.LockBrandCategorySalesPerson(model.CustomerAccountManager);

            return View(model);
        }

        void prepareEditModel(EditBrandCategorySalesPersonViewModel model, int id, int customerId) {
            var customer = CustomerService.FindCustomerModel(customerId, CurrentCompany);

            string title = EvolutionResources.bnrAddEditCustomerAccountManager + (customer == null ? "" : " - " + customer.Name);
            if (id <= 0) title += " - " + EvolutionResources.lblNewAddress;

            PrepareViewModel(model, title, customerId, MakeMenuOptionFlags(customerId, 0));

            model.BrandCategoryList = ProductService.FindBrandCategoryListItemModel(model.CurrentCompany);
            model.SalesPersonList = MembershipManagementService.FindUserListItemModel();
            model.SalesPersonTypeList = LookupService.FindLOVItemsListItemModel(null, LOVName.SalesPersonType);
            model.ParentId = customerId;
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.Sales)]
        public ActionResult Delete(int index, int id) {
            var model = new BrandCategorySalesPersonListModel();
            model.GridIndex = index;

            try {
                CustomerService.DeleteBrandCategorySalesPerson(id);
            } catch (Exception e1) {
                model.Error.SetError(e1);
            }
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        [ValidateAntiForgeryToken]
        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.Sales)]
        public ActionResult Save(EditBrandCategorySalesPersonViewModel model, string command) {
            if (command.ToLower() == "save") {
                var modelError = CustomerService.InsertOrUpdateBrandCategorySalesPerson(model.CustomerAccountManager, CurrentUser, model.LGS);
                if (modelError.IsError) {
                    prepareEditModel(model, model.CurrentCompany.Id, model.CustomerAccountManager.CustomerId);
                    model.SetErrorOnField(ErrorIcon.Error,
                                          modelError.Message,
                                          "CustomerAccountManager_" + modelError.FieldName);
                    return View("Edit", model);

                } else {
                    return RedirectToAction("CustomerAccountManagers", new { id = model.ParentId });
                }

            } else {
                return RedirectToAction("CustomerAccountManagers", new { id = model.ParentId });
            }
        }
    }
}