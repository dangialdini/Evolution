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
    public class CreditsController : BaseController {
        // GET: Credits/Credits
        public ActionResult Index() {
            return Credits();
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.Sales)]
        public ActionResult Credits() {
            var model = createModel();
            return View("Credits", model);
        }

        ViewModelBase createModel() {
            var model = new ViewModelBase();
            PrepareViewModel(model, EvolutionResources.bnrCredits, 0, MenuOptionFlag.RequiresNoCustomer);
            return model;
        }
    }
}
