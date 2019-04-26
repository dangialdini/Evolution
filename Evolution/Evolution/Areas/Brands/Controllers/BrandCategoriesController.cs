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
using Evolution.Extensions;

namespace Evolution.Areas.Brands.Controllers
{
    public class BrandCategoriesController : BaseController {
        // GET: BrandCategories/BrandCategories
        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.PurchasingSuper)]
        public ActionResult Index() {
            var model = createModel();
            return View("BrandCategories", model);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.PurchasingSuper)]
        public ActionResult BrandCategories() {
            var model = createModel();
            return View("BrandCategories", model);
        }

        ViewModelBase createModel() {
            var model = new ViewModelBase();
            PrepareViewModel(model, EvolutionResources.bnrBrandCategories, 0, MenuOptionFlag.RequiresNoProduct);

            return model;
        }

        [HttpGet]
        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.PurchasingSuper)]
        public ActionResult GetBrandCategories(int index, int pageNo, int pageSize, string search) {
            var model = createModel();
            return Json(ProductService.FindBrandCategoriesListModel(model.CurrentCompany.Id, index, pageNo, pageSize, search), JsonRequestBehavior.AllowGet);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.PurchasingSuper)]
        public ActionResult Edit(int id) {
            var model = new EditBrandCategoryViewModel();
            prepareEditModel(model);

            model.BrandCategory = ProductService.FindBrandCategoryModel(id, CurrentCompany);

            model.BrandLists.SetAvailableItems(ProductService.FindBrandListItemModel());
            model.BrandLists.SetSelectedItems(ProductService.FindBrandBrandCategoriesListItemModel(model.BrandCategory));

            model.LGS = ProductService.LockBrandCategory(model.BrandCategory);

            return View(model);
        }

        void prepareEditModel(EditBrandCategoryViewModel model) {
            PrepareViewModel(model, EvolutionResources.bnrAddEditBrandCategory, 0, MenuOptionFlag.RequiresNoProduct);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.PurchasingSuper)]
        public ActionResult Delete(int index, int id) {
            var model = new BrandCategoryListModel();
            model.GridIndex = index;
            try {
                ProductService.DeleteBrandCategory(id);
            } catch (Exception e1) {
                model.Error.SetError(e1);
            }
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        [ValidateAntiForgeryToken]
        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.PurchasingSuper)]
        public ActionResult Save(EditBrandCategoryViewModel model, string command) {
            if (command.ToLower() == "save") {
                string selectedIds = "";
                try {
                    selectedIds = Request.Form["SelectedIds"];
                } catch { }

                var modelError = ProductService.InsertOrUpdateBrandCategory(model.BrandCategory, 
                                                                            selectedIds.ToIntList(), 
                                                                            CurrentUser, 
                                                                            model.LGS);
                if (modelError.IsError) {
                    prepareEditModel(model);
                    model.SetErrorOnField(ErrorIcon.Error,
                                          modelError.Message,
                                          "BrandCategory_" + modelError.FieldName);
                    return View("Edit", model);

                } else {
                    return RedirectToAction("BrandCategories");
                }

            } else {
                return RedirectToAction("BrandCategories");
            }
        }
    }
}
