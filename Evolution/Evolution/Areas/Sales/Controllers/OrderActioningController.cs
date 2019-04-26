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
    public class OrderActioningController : BaseController
    {
        // GET: Sales/OrderActioning
        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.SuperUser)]
        public ActionResult Index() {
            return OrderActioning();
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.SuperUser)]
        public ActionResult OrderActioning() {
            var model = createModel();
            return View("OrderActioning", model);
        }

        ViewModelBase createModel() {
            var model = new ViewModelBase();
            PrepareViewModel(model, EvolutionResources.bnrOrderActioning, 0, MenuOptionFlag.RequiresNoSale);

            return model;
        }

        [HttpGet]
        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.Sales)]
        public ActionResult GetOrderActioning(int index, int locationId, int regionId, int nextactionid, int brandCategoryId,
                                              int pageNo, int pageSize, string search,
                                              string sortColumn, int sortOrder) {
            return Json(SalesService.FindOrderActioning(CurrentCompany.Id,
                                                        locationId,
                                                        regionId,
                                                        nextactionid,
                                                        brandCategoryId,
                                                        index,
                                                        pageNo,
                                                        pageSize,
                                                        search,
                                                        sortColumn,
                                                        (SortOrder)sortOrder),
                        JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.Sales)]
        public ActionResult GetLocationList() {
            var list = LookupService.FindLocationListItemModel(CurrentCompany);
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.Sales)]
        public ActionResult GetRegionList() {
            var list = LookupService.FindRegionsListItemModel(CurrentCompany, true);
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.Sales)]
        public ActionResult GetNextActionList() {
            var list = LookupService.FindSaleNextActionListItemModel(true);
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.Sales)]
        public ActionResult GetBrandCategoryList() {
            var list = ProductService.FindBrandCategoryListItemModel(CurrentCompany);
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.Sales)]
        public ActionResult DoCreatePicks(string sohIds) {
            var result = new JSONResultModel();
            result.Error = SalesService.CreatePicks(CurrentCompany, CurrentUser, sohIds, false);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.Sales)]
        public ActionResult DoCombinePicks(string sohIds) {
            var result = new JSONResultModel();
            result.Error = SalesService.CreatePicks(CurrentCompany, CurrentUser, sohIds, true);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}