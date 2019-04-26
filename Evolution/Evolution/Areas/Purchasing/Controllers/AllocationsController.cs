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
using Evolution.Resources;

namespace Evolution.Areas.Purchasing.Controllers
{
    public class AllocationsController : BaseController
    {
        // GET: Purchasing/Allocations
        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.AllPurchasing)]
        public ActionResult Index() {
            return Allocations();
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.AllPurchasing)]
        public ActionResult Allocations(int id = 0) {
            var model = new ViewModelBase();

            // On entry, the Id is the Id of the PurchaseOrderHeaderTemp
            var poht = PurchasingService.FindPurchaseOrderHeaderTempModel(id, CurrentCompany, false);

            model.ParentId = id;
            PrepareViewModel(model, EvolutionResources.bnrAllocations + (poht == null ? "" : " - Order Number: " + poht.OrderNumber.ToString()), id, MakeMenuOptionFlags(0, poht.OriginalRowId));

            return View("Allocations", model);
        }

        ViewModelBase createModel(int parentId) {
            var model = new ViewModelBase();
            PrepareViewModel(model, EvolutionResources.bnrAllocations);

            model.ParentId = parentId;

            return model;
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.AllPurchasing)]
        public ActionResult GetAllocations(int index, int parentId, int pageNo, int pageSize, string search) {
            // On entry, parentId is the Id of the PurchaseOrderHeaderTemp
            var model = createModel(parentId);
            return Json(AllocationService.FindAllocationsListModel(parentId, index, pageNo, pageSize, search), JsonRequestBehavior.AllowGet);
        }
    }
}
