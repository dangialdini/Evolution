using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Security.Permissions;
using Evolution.Security;
using Evolution.Controllers.Application;
using Evolution.Models.Models;
using Evolution.Models.ViewModels;
using Evolution.Enumerations;
using Evolution.Extensions;
using Evolution.Resources;

namespace Evolution.Areas.Products.Controllers
{
    public class ProductImportController : BaseController {
        // GET: Products/Product
        public ActionResult Index() {
            return ProductImport();
        }

        public ActionResult ProductImport() {
            var model = createModel();
            return View("ProductImport", model);
        }

        ViewModelBase createModel() {
            var model = new FileUploadViewModel();
            prepareViewModel(model);
            return model;
        }

        void prepareViewModel(FileUploadViewModel model) {
            PrepareViewModel(model,
                             EvolutionResources.bnrProductImport,
                             0,
                             MenuOptionFlag.RequiresNoProduct);
            model.MaxUploadFileSize = GetMaxFileUploadSize();
            model.ValidFileTypes = MediaServices.GetValidOrderImportTypes();

            model.ColumnHeadings = "";
            var headingList = FileImportService.GetHeadingList("Product.dat", false, false);
            for (int i = 0; i < headingList.Count(); i++) {
                if (i > 0) model.ColumnHeadings += ", ";
                model.ColumnHeadings += headingList[i].Id;
            }
        }

        [HttpPost]
        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.AllPurchasing)]
        [ValidateAntiForgeryToken]
        public ActionResult Upload(FileUploadViewModel model, string command) {
            Server.ScriptTimeout = 600;     // Allow 10 minutes for an upload/import

            if (command.ToLower() == "upload") {
                if (model.Files != null || model.Files.Count() == 1 &&
                    model.Images != null || model.Images.Count() == 1) {

                    var targetFolder = Path.GetTempPath();
                    string  prodFile = "",
                            zipFile = "";

                    try {
                        var attachment = model.Files.FirstOrDefault();
                        if (attachment != null) {
                            string fileName = attachment.FileName;

                            if (!MediaServices.IsValidOrderImportType(fileName)) {
                                model.SetError(ErrorIcon.Error, EvolutionResources.errInvalidImportFile.Replace("%1", MediaServices.GetValidOrderImportTypes()));
                            } else if (attachment.ContentLength > 0) {
                                prodFile = targetFolder + fileName.FileName();
                                attachment.SaveAs(prodFile);
                            }

                            if (!model.Error.IsError) {
                                attachment = model.Images.FirstOrDefault();
                                if(attachment != null) { 
                                    fileName = attachment.FileName;

                                    if (fileName.FileExtension().ToLower() != "zip") {
                                        model.SetError(ErrorIcon.Error, EvolutionResources.errInvalidImportFile.Replace("%1", "zip"));
                                    } else if (attachment.ContentLength > 0) {
                                        zipFile = targetFolder + fileName.FileName();
                                        attachment.SaveAs(zipFile);

                                        MembershipManagementService.SaveProperty("ProdImportZip", zipFile);
                                    }
                                }
                            }
                        }
                        
                    } catch(Exception e1) {
                        model.Error.SetError(e1);    
                    }

                    if(!model.Error.IsError) { 
                        try { 
                            model.Error = FileImportService.UploadFile(CurrentCompany, CurrentUser, prodFile,
                                                                       model.FirstLineContainsHeader);
                            if (!model.Error.IsError) {
                                // Display the table
                                var mappingsModel = new EditDataMappingViewModel();
                                prepareViewModel(mappingsModel);

                                mappingsModel.Data = FileImportService.GetData(CurrentCompany, CurrentUser);

                                ProductService.ValidateProducts(CurrentCompany, CurrentUser, mappingsModel.Data.Headings);
                                mappingsModel.Data = FileImportService.GetData(CurrentCompany, CurrentUser);

                                return View("ImportMappings", mappingsModel);
                            }

                        } catch (Exception e1) {
                            model.SetError(ErrorIcon.Error, EvolutionResources.errFailedToUploadFile.Replace("%1", prodFile).Replace("%2", e1.Message));
                            LogService.WriteLog(e1, Request.RawUrl);
                        }
                    }

                    MediaService.MediaService.DeleteFile(prodFile);
                    MediaService.MediaService.DeleteFile(zipFile);

                    PrepareViewModel(model, EvolutionResources.bnrProductImport, 0, 0);
                    return View("ProductImport", model);
                }
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.AllPurchasing)]
        [ValidateAntiForgeryToken]
        public ActionResult Import(EditDataMappingViewModel model, string command) {
            if (command.ToLower() == "import") {
                var error = ProductService.ValidateProducts(CurrentCompany, CurrentUser, model.Data.Headings);
                if (error.IsError) {
                    prepareViewModel(model);
                    model.Data = FileImportService.GetData(CurrentCompany, CurrentUser);
                    model.SetError(ErrorIcon.Error, error.Message);

                    return View("ImportMappings", model);

                } else {
                    error = ProductService.ImportProducts(CurrentCompany,
                                                          CurrentUser,
                                                          model.Data.Headings,
                                                          MembershipManagementService.GetProperty("ProdImportZip", ""));
                    if (error.IsError) {
                        prepareViewModel(model);
                        model.Data = FileImportService.GetData(CurrentCompany, CurrentUser);
                        model.SetError(ErrorIcon.Error, error.Message);

                        return View("ImportMappings", model);

                    } else {
                        // Successfully imported, so redirect to purchase orders screen
                        MembershipManagementService.SaveProperty("ProdImportZip", "");
                        return RedirectToAction("Products", "Products", new { Area = "Products" });
                    }
                }

            } else {
                return RedirectToAction("Index");
            }
        }

        private void prepareViewModel(EditDataMappingViewModel model) {
            PrepareViewModel(model, EvolutionResources.bnrProductImport, 0, 0);

            model.HeadingList = FileImportService.GetHeadingList("Product.dat");
        }
    }
}
