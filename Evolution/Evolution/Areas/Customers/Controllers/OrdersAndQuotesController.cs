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
    public class OrdersAndQuotesController : BaseController {
        // GET: OrdersAndQuotes/OrdersAndQuotes
        public ActionResult Index() {
            return OrdersAndQuotes();
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.Sales)]
        public ActionResult OrdersAndQuotes() {
            var model = createModel();
            return View("OrdersAndQuotes", model);
        }

        ViewModelBase createModel() {
            var model = new ViewModelBase();
            PrepareViewModel(model, EvolutionResources.bnrOrdersAndQuotes);
            return model;
        }
    }
}
