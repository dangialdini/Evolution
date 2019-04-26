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

namespace Evolution.Areas.Products.Controllers {
    public class ProductsController : BaseController {
        // GET: Products/Products
        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.Purchasing)]
        public ActionResult Index() {
            return Products();
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.Purchasing)]
        public ActionResult Products(int brandId = 0) {
            var model = createModel(brandId);
            return View("Products", model);
        }

        ProductListViewModel createModel(int brandId) {
            var model = new ProductListViewModel();
            PrepareViewModel(model, EvolutionResources.bnrProducts, 0, MenuOptionFlag.RequiresNoProduct);

            model.BrandList = ProductService.FindBrandListItemModel();
            if (brandId != 0) {
                model.SelectedBrandId = brandId;
            } else {
                model.SelectedBrandId = (model.BrandList.Count > 0 ? Convert.ToInt32(model.BrandList[0].Id) : 0);
            }

            return model;
        }

        [HttpGet]
        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.Purchasing + "," + UserRole.Sales)]
        public ActionResult GetProducts(int index, int brandId, int availabilityId,
                                        int pageNo, int pageSize, string search,
                                        string sortColumn, int sortOrder) {
            return Json(ProductService.FindProductsListModel(brandId, 
                                                             availabilityId,
                                                             index, 
                                                             pageNo, 
                                                             pageSize, 
                                                             search,
                                                             sortColumn, 
                                                             (SortOrder)sortOrder), 
                        JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.Purchasing)]
        public ActionResult GetBrands() {
            return Json(ProductService.FindBrandListItemModel(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.Purchasing)]
        public ActionResult GetAvailabilities() {
            var avail = LookupService.FindLOVItemsListItemModel(null, LOVName.ProductAvailability);
            avail.Insert(0, new ListItemModel(EvolutionResources.lblAny, 0));
            return Json(avail, JsonRequestBehavior.AllowGet);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.Purchasing)]
        public ActionResult Edit(int id, int? brandId = 0) {    // BrandId is only needed for new products
            var model = new EditProductViewModel();
            model.Product = ProductService.FindProductModel(id, brandId, CurrentCompany);
            prepareEditModel(model, id);

            model.LGS = ProductService.LockProduct(model.Product);

            return View(model);
        }

        void prepareEditModel(EditProductViewModel model, int id) {
            PrepareViewModel(model, EvolutionResources.bnrAddEditProduct + (model.Product.Id > 0 ? " - " + model.Product.ItemName : ""), id, MakeMenuOptionFlags(0, 0, 0, 0, model.Product.Id) + MenuOptionFlag.RequiresNewProduct);

            model.ProductAvailabilityList = LookupService.FindLOVItemsListItemModel(CurrentCompany, LOVName.ProductAvailability);
            model.MaterialList = LookupService.FindLOVItemsListItemModel(CurrentCompany, LOVName.Material);
            model.ABList = LookupService.FindLOVItemsListItemModel(CurrentCompany, LOVName.AB);
            model.UserList = MembershipManagementService.FindUserListItemModel();
            model.ProductStatusList = new List<ListItemModel>();
            foreach(var item in LookupService.FindLOVItemsModel(null, LOVName.ProductStatus)) {
                model.ProductStatusList.Add(new ListItemModel { Id = item.ItemValue1, Text = item.ItemText });
            }
            model.WebCategoryList = LookupService.FindLOVItemsListItemModel(CurrentCompany, LOVName.WebCategory);
            model.WebSubCategoryList = LookupService.FindLOVItemsListItemModel(CurrentCompany, LOVName.WebSubCategory);

            model.MediaPath = MediaServices.GetProductImageFolder(true);

            var bc = new Evolution.BarCodeService.BarCodeService(db);
            model.Product.BarCodeFile1 = bc.GetBarCode(model.Product.BarCode, true);
            model.Product.BarCodeFile2 = bc.GetBarCode(model.Product.InnerBarCode, true);
            model.Product.BarCodeFile3 = bc.GetBarCode(model.Product.MasterBarCode, true);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.Purchasing)]
        public ActionResult Delete(int index, int id) {
            var model = new ProductListModel();
            model.GridIndex = index;
            try {
                // Can't delete product if it is attached to anything
                var error = ProductService.DeleteProduct(id);
                model.Error.SetError(error.Message);
            } catch (Exception e1) {
                model.Error.SetError(e1);
            }
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        [ValidateAntiForgeryToken]
        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.SuperUser)]
        public ActionResult Save(EditProductViewModel model, string command) {
            string cmd = command.ToLower();
            if (cmd == "save" || cmd == "saveandexit" || cmd == "approve") {
                if (ModelState.IsValid) {
                    // Merge the properties on this page with those already in the database
                    var product = ProductService.FindProductModel(model.Product.Id, null, null, false);
                    if (product != null) {
                        product.PrimaryMediaId = model.Product.PrimaryMediaId;
                        product.ItemName = model.Product.ItemName;
                        product.ProductAvailabilityId = model.Product.ProductAvailabilityId;
                        product.ProductStatus = model.Product.ProductStatus;
                        product.ItemNameLong = model.Product.ItemNameLong;
                        product.ItemNameFormat = model.Product.ItemNameFormat;
                        product.ItemNameStyle = model.Product.ItemNameStyle;
                        product.ItemNumber = model.Product.ItemNumber;
                        product.AB = model.Product.AB;
                        product.Set = model.Product.Set;
                        product.ItemDescription = model.Product.ItemDescription;
                        product.MaterialId = model.Product.MaterialId;
                        product.BarCode = model.Product.BarCode;
                        product.InnerBarCode = model.Product.InnerBarCode;
                        product.MasterBarCode = model.Product.MasterBarCode;
                        product.HSCode = model.Product.HSCode;
                        product.WebCategoryId = model.Product.WebCategoryId;
                        product.WebSubCategoryId = model.Product.WebSubCategoryId;
                        if (model.Approved == 1) {
                            product.ApprovedById = CurrentUser.Id;
                            product.ApprovedDate = DateTimeOffset.Now;
                            product.ApprovedByText = CurrentUser.FullName;
                        }

                    } else {
                        product = model.Product;
                    }

                    var modelError = ProductService.InsertOrUpdateProduct(product, CurrentUser, model.LGS);
                    if (modelError.IsError) {
                        prepareEditModel(model, product.Id);
                        model.SetErrorOnField(ErrorIcon.Error,
                                              modelError.Message,
                                              "Product_" + modelError.FieldName);
                        return View("Edit", model);

                    } else {
                        switch(cmd) {
                        case "saveandexit":
                            return RedirectToAction("Products", "Products", new { BrandId = product.BrandId });
                        case "approve":
                            model.SetErrorOnField(ErrorIcon.Information, EvolutionResources.infProductHasBeenApproved, "", null, null, null, null, true);
                            return RedirectToAction("Edit", new { Id = product.Id });
                        default:
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
