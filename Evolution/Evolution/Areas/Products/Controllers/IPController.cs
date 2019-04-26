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
    public class IPController : BaseController {
        // GET: Products/IP
        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.Purchasing)]
        public ActionResult Index() {
            return Edit(0);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.Purchasing)]
        public ActionResult Edit(int id) {
            var model = createModel(id);
            prepareViewModel(model);
            return View("IP", model);
        }

        EditProductIPViewModel createModel(int productId) {
            var model = new EditProductIPViewModel();

            var product = ProductService.FindProductModel(productId, 0, CurrentCompany);
            model.ProductId = product.Id;

            PrepareViewModel(model, EvolutionResources.bnrIntellectualProperty + (productId == 0 ? "" : " - " + product.ItemName),
                             productId, MakeMenuOptionFlags(0, 0, 0, 0, productId));

            return model;
        }

        void prepareViewModel(EditProductIPViewModel model) {
            model.ProductIPs = ProductService.FindProductIPListModel(model.ProductId, 0);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.AllPurchasing)]
        public ActionResult Save(EditProductIPViewModel model, string command) {
            var product = ProductService.FindProductModel(model.ProductId, null, null, false);

            string cmd = command.ToLower();
            if (cmd == "save" || cmd == "saveandexit") {
                if (ModelState.IsValid) {
                    model.Error = ProductService.InsertOrUpdateProductIP(product, model.ProductIPs, model.LGS);

                    if (model.Error.IsError) {
                        prepareViewModel(model);
                        model.SetErrorOnField(ErrorIcon.Error,
                                              model.Error.Message,
                                              model.Error.FieldName);
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
                    prepareViewModel(model);
                    return View("Edit", model);
                }

            } else {
                return RedirectToAction("Products", "Products", new { BrandId = product.BrandId });
            }
        }
    }
}
