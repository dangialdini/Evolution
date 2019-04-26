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
    public class SupplierInfoController : BaseController
    {
        // GET: Products/SupplierInfo
        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.Purchasing)]
        public ActionResult Index() {
            return Edit(0);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.Purchasing)]
        public ActionResult Edit(int id) {
            var model = new EditProductViewModel();
            model.Product = ProductService.FindProductModel(id, null, null, false);
            prepareEditModel(CurrentCompany, model, id);

            model.LGS = ProductService.LockProduct(model.Product);

            return View(model);
        }

        void prepareEditModel(CompanyModel company, EditProductViewModel model, int id) {
            PrepareViewModel(model, EvolutionResources.bnrSupplierInfo + (id == 0 ? "" : " - " + model.Product.ItemName),
                             id, MakeMenuOptionFlags(0, 0, 0, 0, id));

            model.SupplierList = SupplierService.FindSupplierListItemModel(company);
            model.ManufacturerList = LookupService.FindLOVItemsListItemModel(CurrentCompany, LOVName.Manufacturer);
            model.CountryList = LookupService.FindCountriesListItemModel();
        }

        [ValidateAntiForgeryToken]
        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.SuperUser)]
        public ActionResult Save(EditProductViewModel model, string command) {
            string cmd = command.ToLower();
            if (cmd == "save" || cmd == "saveandexit") {
                if (ModelState.IsValid) {
                    // Merge the properties on this page with those already in the database
                    var product = ProductService.FindProductModel(model.Product.Id, null, null, false);
                    if(product != null) {
                        product.CountryOfOriginId = model.Product.CountryOfOriginId;
                        product.PrimarySupplierId = model.Product.PrimarySupplierId;
                        product.ManufacturerId = model.Product.ManufacturerId;
                        product.SupplierItemNumber = model.Product.SupplierItemNumber;
                        product.SupplierItemName = model.Product.SupplierItemName;
                    } else {
                        product = model.Product;
                    }

                    var modelError = ProductService.ValidateModel(product);
                    if (!modelError.IsError) modelError = ProductService.InsertOrUpdateProduct(product, CurrentUser, model.LGS);

                    if (modelError.IsError) {
                        prepareEditModel(CurrentCompany, model, product.Id);
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
                    prepareEditModel(CurrentCompany, model, model.Product.Id);
                    return View("Edit", model);
                }

            } else {
                return RedirectToAction("Products", "Products", new { BrandId = model.Product.BrandId });
            }
        }
    }
}
