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
    public class ImagesController : BaseController {
        // GET: Products/Images
        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.Purchasing)]
        public ActionResult Index() {
            return Images(0);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.Purchasing)]
        public ActionResult Images(int id) {
            var model = new ProductImageUploadViewModel();
            model.Product = ProductService.FindProductModel(id, 0, CurrentCompany);
            model.MaxUploadFileSize = GetMaxFileUploadSize();
            model.ValidFileTypes = MediaServices.GetValidImageTypes();

            PrepareViewModel(model, EvolutionResources.bnrImages + (id == 0 ? "" : " - " + model.Product.ItemName),
                             id, MakeMenuOptionFlags(0, 0, 0, 0, id));

            return View("Images", model);
        }

        [HttpGet]
        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.Purchasing)]
        public ActionResult GetProductMedia(int index, int id) {
            return Json(ProductService.FindProductMediaListModel(id,
                                                                 index),
                        JsonRequestBehavior.AllowGet);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.Purchasing)]
        public ActionResult DeleteMedia(int index, int id) {
            var model = new ProductMediaListModel();
            model.GridIndex = index;
            try {
                var error = ProductService.DeleteProductMedia(id);
                model.Error.SetError(error.Message);
            } catch (Exception e1) {
                model.Error.SetError(e1);
            }
            return Json(model, JsonRequestBehavior.AllowGet);
        }
        
        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.Purchasing)]
        public ActionResult SetPrimaryImage(int id, int productMediaId) {
            JSONResultModel model = new JSONResultModel();
            var product = ProductService.FindProductModel(id, null, null, false);
            if (product == null) {
                model.Error.SetRecordError("Product", id);
            } else {
                model.Error = ProductService.SetPrimaryMedia(product, productMediaId, CurrentUser, ProductService.LockProduct(product));
            }
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.Purchasing)]
        public ActionResult DoUpload(ProductImageUploadViewModel model) {
            var error = new Error();

            var product = ProductService.FindProductModel(model.Product.Id, null, null, false);
            if (product == null) {
                error.SetRecordError("Product", model.Product.Id);

            } else { 
                if (model.Images != null || model.Images.Count() > 0) {
                    var fileList = new List<string>();
                    var targetFolder = MediaServices.GetMediaFolder(CurrentCompany.Id) + "\\Temp\\";

                    foreach (var file in model.Images) {
                        string targetFile = targetFolder + file.FileName;
                        try {
                            db.AddFileToLog(targetFile, 2);
                            file.SaveAs(targetFile);

                            var prodMedia = new ProductMediaModel();
                            error = ProductService.AddMediaToProduct(product, CurrentCompany, CurrentUser, targetFile, prodMedia, FileCopyType.Move);

                        } catch (Exception ex) {
                            model.SetError(ex, "", true);
                            break;
                        }
                    }
                    if (!error.IsError) model.SetErrorOnField(ErrorIcon.Information, EvolutionResources.infImagesSuccessfullyUploaded, "", null, null, null, null, true);
                }
            }
            return RedirectToAction("Images", "Images", new { Id = model.Product.Id });
        }
    }
}
