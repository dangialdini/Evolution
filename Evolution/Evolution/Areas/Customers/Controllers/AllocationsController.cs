using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Security.Permissions;
using Evolution.Security;
using Evolution.Controllers.Application;
using Evolution.Models.Models;
using Evolution.Models.ViewModels;
using Evolution.Enumerations;
using Evolution.Extensions;
using Evolution.Resources;

namespace Evolution.Areas.Customers.Controllers {
    public class AllocationsController : BaseController {
        // GET: Allocations/Allocations
        public ActionResult Index() {
            return Allocations();
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.Sales)]
        public ActionResult Allocations() {
            var model = createModel();
            return View("Allocations", model);
        }

        ViewModelBase createModel() {
            var model = new ViewModelBase();
            PrepareViewModel(model, EvolutionResources.bnrAllocations, 0, MenuOptionFlag.RequiresNoCustomer);
            return model;
        }
    }
}
