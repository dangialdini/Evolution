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
    public class AdditionalCategoryController : BaseController {
        // GET: Products/ProductAdditionalCategory
        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.Purchasing)]
        public ActionResult Index() {
            return Edit(0);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.Purchasing)]
        public ActionResult Edit(int id) {
            var model = new EditProductViewModel();
            model.Product = ProductService.FindProductModel(id, null, null, false);
            prepareEditModel(model, id);

            model.LGS = ProductService.LockProduct(model.Product);

            return View(model);
        }

        void prepareEditModel(EditProductViewModel model, int id) {
            PrepareViewModel(model, EvolutionResources.bnrAdditionalCategories + (id == 0 ? "" : " - " + model.Product.ItemName),
                             id, MakeMenuOptionFlags(0, 0, 0, 0, id));

            model.CategoryList = LookupService.FindLOVItemsListItemModel(CurrentCompany, LOVName.ProductCategory);
            model.FormatList = LookupService.FindLOVItemsListItemModel(CurrentCompany, LOVName.Format);
            model.FormatTypeList = LookupService.FindLOVItemsListItemModel(CurrentCompany, LOVName.FormatType);
            model.SeasonList = LookupService.FindLOVItemsListItemModel(CurrentCompany, LOVName.Season);
            model.PackingTypeList = LookupService.FindLOVItemsListItemModel(CurrentCompany, LOVName.PackingType);
            model.KidsAdultsList = LookupService.FindLOVItemsListItemModel(CurrentCompany, LOVName.KidsAdults);
            model.AgeGroupList = LookupService.FindLOVItemsListItemModel(CurrentCompany, LOVName.AgeGroup);
            model.DvlptTypeList = LookupService.FindLOVItemsListItemModel(CurrentCompany, LOVName.DevelopmentType);
            model.PCProductList = LookupService.FindLOVItemsListItemModel(CurrentCompany, LOVName.PCProduct);
            model.PCDvlptList = LookupService.FindLOVItemsListItemModel(CurrentCompany, LOVName.PCDvlpt);
        }

        [ValidateAntiForgeryToken]
        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.SuperUser)]
        public ActionResult Save(EditProductViewModel model, string command) {
            string cmd = command.ToLower();
            if (cmd == "save" || cmd == "saveandexit") {
                if (ModelState.IsValid) {
                    // Merge the properties on this page with those already in the database
                    var product = ProductService.FindProductModel(model.Product.Id, null, null, false);
                    if (product != null) {
                        product.AdditionalCategory.CategoryId = model.Product.AdditionalCategory.CategoryId;
                        product.AdditionalCategory.FormatId = model.Product.AdditionalCategory.FormatId;
                        product.AdditionalCategory.FormatTypeId = model.Product.AdditionalCategory.FormatTypeId;
                        product.AdditionalCategory.SeasonId = model.Product.AdditionalCategory.SeasonId;
                        product.AdditionalCategory.PackingTypeId = model.Product.AdditionalCategory.PackingTypeId;
                        product.AdditionalCategory.KidsAdultsId = model.Product.AdditionalCategory.KidsAdultsId;
                        product.AdditionalCategory.AgeGroupId = model.Product.AdditionalCategory.AgeGroupId;
                        product.AdditionalCategory.DvlptTypeId = model.Product.AdditionalCategory.DvlptTypeId;
                        product.AdditionalCategory.PCProductId = model.Product.AdditionalCategory.PCProductId;
                        product.AdditionalCategory.PCDvlptId = model.Product.AdditionalCategory.PCDvlptId;

                    } else {
                        product = model.Product;
                    }

                    var modelError = ProductService.ValidateModel(product);
                    if (!modelError.IsError) modelError = ProductService.InsertOrUpdateProduct(product, CurrentUser, model.LGS);   

                    if (modelError.IsError) {
                        prepareEditModel(model, product.Id);
                        model.SetErrorOnField(ErrorIcon.Error,
                                              modelError.Message,
                                              "Product_AdditionalCategory_" + modelError.FieldName);
                        return View("Edit", model);

                    } else {
                        if (cmd == "saveandexit") {
                            return RedirectToAction("Products", "Products", new { BrandId = product.BrandId });
                        } else {
                            model.SetErrorOnField(ErrorIcon.Information, EvolutionResources.infChangesSuccessfullySaved, "", null, null, null, null, true);
                            return RedirectToAction("Edit", new { Id = product.Id });
                        }
                    }
                } else {
                    prepareEditModel(model, model.Product.Id);
                    return View("Edit", model);
                }

            } else {
                return RedirectToAction("Products", "Products", new { BrandId = model.Product.BrandId });
            }
        }
    }
}
