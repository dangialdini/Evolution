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
    public class AllocationsController : BaseController
    {
        // GET: Sales/Allocations
        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.Sales)]
        public ActionResult Index() {
            return Allocations();
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.Sales)]
        public ActionResult Allocations() {
            var model = createModel();
            return View("Allocations", model);
        }

        ViewModelBase createModel() {
            var model = new AllocationsViewModel();
            PrepareViewModel(model, EvolutionResources.bnrAllocations, 0, MenuOptionFlag.RequiresNoSale);

            model.BrandCategoryId = CurrentUser.DefaultBrandCategoryId.Value;
            model.BrandCategoryList = ProductService.FindBrandCategoryListItemModel(CurrentCompany);
            model.LocationId = CurrentCompany.DefaultLocationID.Value;
            model.LocationList = LookupService.FindLocationListItemModel(CurrentCompany);

            return model;
        }

        [HttpGet]
        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.Sales)]
        public ActionResult GetProducts(int index, int brandId, int pageNo, int pageSize, string search,
                                        string sortColumn, int sortOrder) {
            return Json(ProductService.FindProductsListModel(brandId,
                                                             0,
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
        public ActionResult GetBrands() {
            return Json(ProductService.FindBrandListItemModel(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.Sales)]
        public ActionResult GetLocations() {
            return Json(LookupService.FindLocationListItemModel(CurrentCompany), JsonRequestBehavior.AllowGet);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.Sales)]
        public ActionResult GetAllocationInfo(int id, int brandId) {      // ProductId
            var model = new AllocationInfoViewModel();
            model.Product = ProductService.FindProductModel(id, brandId, CurrentCompany);
            return View("AllocationInfo", model);
        }

        [HttpGet]
        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.Sales)]
        public ActionResult GetAvailabilityDetails(int index, int productId, int locationId, int brandCategoryId) {
            return Json(AllocationService.FindAvailabilityDetails(index, productId, locationId, brandCategoryId),
                        JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.Sales)]
        public ActionResult GetSalesOrders(int index, int productId, int locationId, int brandCategoryId) {
            return Json(AllocationService.FindSaleDetails(index, productId, locationId, brandCategoryId),
                        JsonRequestBehavior.AllowGet);
        }
    }
}
