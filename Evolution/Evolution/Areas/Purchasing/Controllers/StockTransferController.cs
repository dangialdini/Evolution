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

namespace Evolution.Areas.Purchasing.Controllers {
    public class StockTransferController : BaseController {
        // GET: StockTransfer/StockTransfer
        public ActionResult Index() {
            return StockTransfer();
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.AllPurchasing)]
        public ActionResult StockTransfer() {
            var model = createModel();
            return View("StockTransfer", model);
        }

        ViewModelBase createModel() {
            var model = new ViewModelBase();
            PrepareViewModel(model, EvolutionResources.bnrStockTransfer, 0, MenuOptionFlag.RequiresNoPurchase);
            return model;
        }
    }
}