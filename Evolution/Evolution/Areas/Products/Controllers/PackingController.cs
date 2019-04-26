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
    public class PackingController : BaseController {
        // GET: Products/Packing
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
            PrepareViewModel(model, EvolutionResources.bnrPackingInfo + (id == 0 ? "" : " - " + model.Product.ItemName),
                             id, MakeMenuOptionFlags(0, 0, 0, 0, id));

            model.PackPackingTypeList = LookupService.FindLOVItemsListItemModel(CurrentCompany, LOVName.PackPackingType);
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
                        product.InnerQuantity = model.Product.InnerQuantity;
                        product.OuterQuantity = model.Product.OuterQuantity;
                        product.MasterQuantity = model.Product.MasterQuantity;
                        product.PackingTypeId = model.Product.PackingTypeId;

                        product.Length = model.Product.Length;
                        product.Width = model.Product.Width;
                        product.Height = model.Product.Height;
                        product.Weight = model.Product.Weight;

                        product.PackedLength = model.Product.PackedLength;
                        product.PackedWidth = model.Product.PackedWidth;
                        product.PackedHeight = model.Product.PackedHeight;
                        product.PackedWeight = model.Product.PackedWeight;

                        product.InnerLength = model.Product.InnerLength;
                        product.InnerWidth = model.Product.InnerWidth;
                        product.InnerHeight = model.Product.InnerHeight;
                        product.InnerWeight = model.Product.InnerWeight;

                        product.MasterLength = model.Product.MasterLength;
                        product.MasterWidth = model.Product.MasterWidth;
                        product.MasterHeight = model.Product.MasterHeight;
                        product.MasterWeight = model.Product.MasterWeight;

                        product.UnitCBM = model.Product.UnitCBM;
                        product.WeightPerProduct = model.Product.WeightPerProduct;

                    } else {
                        product = model.Product;
                    }

                    var modelError = ProductService.ValidateModel(product);
                    if (!modelError.IsError) modelError = ProductService.InsertOrUpdateProduct(product, CurrentUser, model.LGS);

                    if (modelError.IsError) {
                        prepareEditModel(model, product.Id);
                        model.SetErrorOnField(ErrorIcon.Error,
                                              modelError.Message,
                                              "Product_" + modelError.FieldName);
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
