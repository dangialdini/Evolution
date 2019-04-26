using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Security.Permissions;
using Evolution.Security;
using Evolution.Controllers.Application;
using Evolution.Models.ViewModels;
using Evolution.Enumerations;
using Evolution.Models.Models;
using Evolution.Resources;

namespace Evolution.Areas.Sales.Controllers
{
    public class HoldingOrderReAllocationController : BaseController
    {
        // GET: Sales/HoldingOrderReAllocation
        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.SuperUser)]
        public ActionResult Index() {
            return HoldingOrderReAllocations();
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.SuperUser)]
        public ActionResult HoldingOrderReAllocations() {
            var model = createModel();
            return View("HoldingOrderReAllocations", model);
        }

        ViewModelBase createModel() {
            var model = new ViewModelBase();
            PrepareViewModel(model, EvolutionResources.bnrHoldingOrderReAllocations, 0, MenuOptionFlag.RequiresNoSale);

            return model;
        }
    }
}