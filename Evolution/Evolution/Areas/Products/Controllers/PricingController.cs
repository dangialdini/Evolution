using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Security.Permissions;
using System.IO;
using Evolution.Security;
using Evolution.Controllers.Application;
using Evolution.Models.ViewModels;
using Evolution.Enumerations;
using Evolution.Models.Models;
using Evolution.Resources;

namespace Evolution.Areas.Products.Controllers
{
    public class PricingController : BaseController
    {
        // GET: Products/Pricing
        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.Purchasing)]
        public ActionResult Index() {
            return Pricing(0);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.Purchasing)]
        public ActionResult Pricing(int id) {
            var model = new EditProductViewModel();
            model.Product = ProductService.FindProductModel(id, 0, CurrentCompany);
            PrepareViewModel(model, EvolutionResources.bnrPricing + (id == 0 ? "" : " - " + model.Product.ItemName),
                             id, MakeMenuOptionFlags(0, 0, 0, 0, id));

            return View("Pricing", model);
        }
    }
}
