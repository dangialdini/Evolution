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

namespace Evolution.Areas.Shipments.Controllers {
    public class ShipmentPhasePortTimesController : BaseController {
        // GET: Shipments/ShipmentPhasePortTimes
        public ActionResult Index() { 
            return ShipmentPhasePortTimes();
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.AllPurchasing)]
        public ActionResult ShipmentPhasePortTimes() {
            var model = createModel();
            return View("ShipmentPhasePortTimes", model);
        }

        ViewModelBase createModel() {
            var model = new ViewModelBase();
            PrepareViewModel(model, EvolutionResources.bnrShipmentPhasePortTimes);
            return model;
        }
    }
}
