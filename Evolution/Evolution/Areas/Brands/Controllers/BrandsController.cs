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

namespace Evolution.Areas.Brands.Controllers {
    public class BrandsController : BaseController {
        // GET: Brands/Brands
        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.PurchasingSuper)]
        public ActionResult Index() {
            var model = createModel();
            return View("Brands", model);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.PurchasingSuper)]
        public ActionResult Brands() {
            var model = createModel();
            return View("Brands", model);
        }

        ViewModelBase createModel() {
            var model = new ViewModelBase();
            PrepareViewModel(model, EvolutionResources.bnrBrands, 0, MenuOptionFlag.RequiresNoProduct);

            return model;
        }

        [HttpGet]
        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.PurchasingSuper)]
        public ActionResult GetBrands(int index, int pageNo, int pageSize, string search) {
            var model = createModel();
            return Json(ProductService.FindBrandListModel(index, pageNo, pageSize, search), JsonRequestBehavior.AllowGet);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.PurchasingSuper)]
        public ActionResult Edit(int id) {
            var model = new EditBrandViewModel();
            prepareEditModel(model);

            model.Brand = ProductService.FindBrandModel(id);
            model.LGS = ProductService.LockBrand(model.Brand);

            return View(model);
        }

        void prepareEditModel(EditBrandViewModel model) {
            PrepareViewModel(model, EvolutionResources.bnrAddEditBrand, 0, MenuOptionFlag.RequiresNoProduct);
            model.BrandCategoryList = ProductService.FindBrandCategoryListItemModel(model.CurrentCompany);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.PurchasingSuper)]
        public ActionResult Delete(int index, int id) {
            var model = new BrandListModel();
            model.GridIndex = index;
            try {
                var modelError = ProductService.DeleteBrand(id);
                if (modelError.IsError) model.Error.SetError(modelError.Message);

            } catch (Exception e1) {
                model.Error.SetError(e1);
            }
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        [ValidateAntiForgeryToken]
        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.PurchasingSuper)]
        public ActionResult Save(EditBrandViewModel model, string command) {
            if (command.ToLower() == "save") {
                var modelError = ProductService.InsertOrUpdateBrand(model.Brand, CurrentUser, model.LGS);
                if (modelError.IsError) {
                    prepareEditModel(model);
                    model.SetErrorOnField(ErrorIcon.Error,
                                          modelError.Message,
                                          "Brand_" + modelError.FieldName);
                    return View("Edit", model);

                } else {
                    return RedirectToAction("Brands");
                }

            } else {
                return RedirectToAction("Brands");
            }
        }
    }
}
