using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Security.Permissions;
using System.Reflection;
using Evolution.Security;
using Evolution.Controllers.Application;
using Evolution.Models.ViewModels;
using Evolution.Enumerations;
using Evolution.Extensions;
using Evolution.Models.Models;
using Evolution.Resources;

namespace Evolution.Areas.SystemAdmin.Controllers {
    public class SystemStatusController : BaseController {
        // GET: SystemAdmin/SystemStatus
        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.SuperUser)]
        public ActionResult Index() {
            return SystemStatus();
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.SuperUser)]
        public ActionResult SystemStatus() {
            var model = createModel();
            return View("SystemStatus", model);
        }

        ViewModelBase createModel() {
            var model = new SystemStatusViewModel();
            PrepareViewModel(model, EvolutionResources.bnrSystemStatus);

            // Software version info
            Assembly assembly = Assembly.GetExecutingAssembly();
            model.Attributes.Add(new ListItemModel("", EvolutionResources.lblSoftware));
            model.Attributes.Add(new ListItemModel(LookupService.GetExecutableName(assembly), EvolutionResources.lblAssembly));
            model.Attributes.Add(new ListItemModel(LookupService.GetSoftwareVersionInfo(assembly), EvolutionResources.lblSoftwareVersion));
            model.Attributes.Add(new ListItemModel(LookupService.GetExecutableDate(assembly).ToString(CurrentUser.DateFormat), EvolutionResources.lblBuildDate));
            model.Attributes.Add(new ListItemModel(LookupService.GetSoftwareCopyrightInfo(assembly), EvolutionResources.lblCopyright));

            // Create a list of statistics
            model.Attributes.Add(new ListItemModel("", "<br/>"));
            model.Attributes.Add(new ListItemModel("", EvolutionResources.lblCompanies));
            var companyList = CompanyService.FindCompaniesListModel(0, 1, 1000, "", true);
            foreach(var company in companyList.Items) {
                int purchaseCount = PurchasingService.GetPurchaseCount(company);
                int customerCount = CustomerService.GetCustomerCount(company);

                model.Attributes.Add(new ListItemModel($"{customerCount} Customer(s), {purchaseCount} Purchase(s)", company.FriendlyName));
            }

            return model;
        }
    }
}
