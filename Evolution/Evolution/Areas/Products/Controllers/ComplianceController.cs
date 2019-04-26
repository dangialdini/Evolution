using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Security.Permissions;
using Evolution.Security;
using Evolution.Controllers.Application;
using Evolution.Models.ViewModels;
using Evolution.ProductService;
using Evolution.Extensions;
using Evolution.Enumerations;
using Evolution.Models.Models;
using Evolution.Resources;

namespace Evolution.Areas.Products.Controllers
{
    public class ComplianceController : BaseController
    {
        // GET: Products/Compliance
        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.Purchasing)]
        public ActionResult Index() {
            return Compliance(0);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.Purchasing)]
        public ActionResult Compliance(int id) {
            var model = createModel(id);
            return View("Compliance", model);
        }

        ViewModelBase createModel(int productId) {
            var model = new NoteListViewModel();

            var product = ProductService.FindProductModel(productId, 0, CurrentCompany);

            PrepareViewModel(model, EvolutionResources.bnrCompliance + (productId == 0 ? "" : " - " + product.ItemName),
                             productId, MakeMenuOptionFlags(0, 0, 0, 0, productId));
            model.ParentId = productId;

            return model;
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.AllPurchasing)]
        public ActionResult GetCompliance(int index, int productId, int pageNo, int pageSize, string search,
                                          string sortColumn, int sortOrder) {
            return Json(ProductService.FindProductComplianceListModel(productId,
                                                                      index,
                                                                      pageNo,
                                                                      pageSize,
                                                                      search,
                                                                      sortColumn,
                                                                      (SortOrder)sortOrder),
                        JsonRequestBehavior.AllowGet);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.AllPurchasing)]
        public ActionResult Edit(int id, int productId) {
            var model = new EditProductComplianceViewModel();
            model.ProductCompliance = ProductService.FindProductComplianceModel(id, productId, true);
            prepareEditModel(model, id, productId);

            return View("Edit", model);
        }

        void prepareEditModel(EditProductComplianceViewModel model, int id, int productId) {
            var product = ProductService.FindProductModel(productId, null, null, false);

            string title = EvolutionResources.bnrAddEditCompliance + (product == null ? "" : " - " + product.ItemName);
            if (id <= 0) title += " - " + EvolutionResources.lblNewNote;

            PrepareViewModel(model,
                             title,
                             productId,
                             MakeMenuOptionFlags(0, 0, 0, 0, productId));
            model.ParentId = productId;

            model.ComplianceCategoryList = LookupService.FindLOVItemsListItemModel(CurrentCompany, LOVName.ComplianceCategory);
            model.MarketList = LookupService.FindLOVItemsListItemModel(CurrentCompany, LOVName.MarketingLocation);

            model.MaxUploadFileSize = GetMaxFileUploadSize();
            model.ValidFileTypes = MediaServices.GetValidMediaTypes();

            ModelState.Remove("LGS");   // Forces view to get from model instead of Razor modelstate cache
            model.LGS = ProductService.LockProductCompliance(model.ProductCompliance);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.AllPurchasing)]
        public ActionResult Delete(int index, int id) {
            var model = new ProductComplianceListModel();
            model.GridIndex = index;

            try {
                ProductService.DeleteProductCompliance(id);
            } catch (Exception e1) {
                model.Error.SetError(e1);
            }
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.AllPurchasing)]
        public ActionResult Save(EditProductComplianceViewModel model, string command) {
            string cmd = command.ToLower();
            if (cmd == "save") {
                var modelError = ProductService.InsertOrUpdateProductCompliance(model.ProductCompliance, model.LGS);
                if (modelError.IsError) {
                    prepareEditModel(model, model.ProductCompliance.Id, model.ProductCompliance.Id);
                    model.SetErrorOnField(ErrorIcon.Error,
                                          modelError.Message,
                                          "ProductCompliance_" + modelError.FieldName);
                } else {
                    model.SetErrorOnField(ErrorIcon.Information, EvolutionResources.infChangesSuccessfullySaved, "", null, null, null, null, true);

                    // Delete existing attachments
                    foreach (var attachment in model.ProductCompliance.Attachments) {
                        if(attachment.Selected) ProductService.DeleteProductComplianceAttachment(attachment);
                    }

                    // Upload the attachments
                    if (model.Files != null || model.Files.Count() > 0) {
                        var fileList = new List<string>();
                        var targetFolder = MediaServices.GetMediaFolder(CurrentCompany.Id) + "\\Temp\\";
                        MediaService.MediaService.CreateDirectory(targetFolder);

                        foreach (var file in model.Files) {
                            if (file != null) {
                                string targetFile = targetFolder + file.FileName.FileName();
                                try {
                                    db.AddFileToLog(targetFile, 2);
                                    file.SaveAs(targetFile);

                                    var media = new MediaModel();
                                    model.Error = MediaServices.InsertOrUpdateMedia(media, CurrentCompany, CurrentUser,
                                                                              MediaFolder.ProductCompliance,
                                                                              targetFile, "",
                                                                              model.ProductCompliance.ProductId,
                                                                              model.ProductCompliance.Id,
                                                                              FileCopyType.Move);
                                    if (model.Error.IsError) { 
                                        break;
                                    } else {
                                        ProductService.AddMediaToProductCompliance(model.ProductCompliance, media);

                                        model.SetErrorOnField(ErrorIcon.Information, EvolutionResources.infFilesSuccessfullyUploaded, "", null, null, null, null, true);
                                    }

                                } catch (Exception ex) {
                                    model.SetError(ex, "", true);
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            if (!model.Error.IsError) {
                return RedirectToAction("Compliance", new { id = model.ParentId });
            } else {
                return View("Edit", model);
            }
        }
    }
}
