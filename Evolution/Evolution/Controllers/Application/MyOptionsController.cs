using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Security.Permissions;
using Evolution.Security;
using Evolution.Models.Models;
using Evolution.Models.ViewModels;
using Evolution.Extensions;
using Evolution.Enumerations;
using Evolution.Resources;

namespace Evolution.Controllers.Application {
    [Authorize]
    public class MyOptionsController : BaseController {
        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.AllUsers)]
        public ActionResult Index() {
            return MyOptions();
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.AllUsers)]
        public ActionResult MyOptions() {
            var model = new EditUserViewModel();

            model.UserData = MembershipManagementService.User;
            prepareEditModel(model, model.UserData.Id);
            model.LGS = MembershipManagementService.LockUser(model.UserData);

            return View("MyOptions", model);
        }

        void prepareEditModel(EditUserViewModel model, int id) {
            PrepareViewModel(model, EvolutionResources.bnrMyOptions, id);

            model.BrandCategoryList = ProductService.FindBrandCategoryListItemModel(CurrentCompany);
            model.CompanyList = CompanyService.FindCompaniesListItemModel();
            model.DateFormatList = LookupService.FindDateFormatListItemModel();
        }

        [ValidateAntiForgeryToken]
        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.Sales)]
        public ActionResult Save(EditUserViewModel model, string command) {
            if (command.ToLower() == "save") {
                var modelError = MembershipManagementService.InsertOrUpdateUser(model.UserData, CurrentUser, model.LGS);
                if (modelError.IsError) {
                    prepareEditModel(model, model.UserData.Id);
                    model.SetErrorOnField(ErrorIcon.Error,
                                            modelError.Message,
                                            "UserData_" + modelError.FieldName);
                    return View("MyOptions", model);

                } else {
                    // Return to dashboard
                    model.SetError(ErrorIcon.Information, EvolutionResources.infChangesSuccessfullySaved, null, null, null, null, true);
                    return RedirectToAction("Index", "Home");
                }

            } else {
                // Return to dashboard
                return RedirectToAction("Index", "Home");
            }
        }
    }
}