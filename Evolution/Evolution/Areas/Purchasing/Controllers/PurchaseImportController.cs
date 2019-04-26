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

namespace Evolution.Areas.Purchasing.Controllers {
    public class PurchaseImportController : BaseController {
        // GET: Purchasing/PurchaseImport
        public ActionResult Index() {
            return PurchaseImport();
        }

        public ActionResult PurchaseImport() {
            var model = createModel();
            return View("PurchaseImport", model);
        }

        ViewModelBase createModel() {
            var model = new FileUploadViewModel();
            prepareViewModel(model);
            return model;
        }

        void prepareViewModel(FileUploadViewModel model) {
            PrepareViewModel(model,
                             EvolutionResources.bnrPurchaseImport,
                             0,
                             MenuOptionFlag.RequiresNoPurchase);
            model.MaxUploadFileSize = GetMaxFileUploadSize();
            model.ValidFileTypes = MediaServices.GetValidOrderImportTypes();

            model.ColumnHeadings = "";
            var headingList = FileImportService.GetHeadingList("Purchase.dat", false, false);
            for(int i = 0; i < headingList.Count(); i++) {
                if(i > 0) model.ColumnHeadings += ", ";
                model.ColumnHeadings += headingList[i].Id;
            }
        }

        [HttpPost]
        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.AllPurchasing)]
        [ValidateAntiForgeryToken]
        public ActionResult Upload(FileUploadViewModel model, string command) {
            Server.ScriptTimeout = 600;     // Allow 10 minutes for an upload/import

            if (command.ToLower() == "upload") {
                if (model.Files != null && model.Files.Count() > 0) {

                    var targetFile = Path.GetTempPath();

                    var attachment = model.Files.FirstOrDefault();
                    string fileName = attachment.FileName;

                    if(!MediaServices.IsValidOrderImportType(fileName)) {
                        model.SetError(ErrorIcon.Error, EvolutionResources.errInvalidImportFile.Replace("%1", MediaServices.GetValidOrderImportTypes()));

                    } else if (attachment != null && attachment.ContentLength > 0) {
                        try {
                            targetFile += fileName.FileName();
                            attachment.SaveAs(targetFile);

                            var error = FileImportService.UploadFile(CurrentCompany, CurrentUser, targetFile,
                                                                     model.FirstLineContainsHeader);
                            if (error.IsError) {
                                model.SetError(ErrorIcon.Error, error.Message);

                            } else {
                                // Display the table
                                var mappingsModel = new EditPurchaseImportDataMappingsViewModel();
                                prepareViewModel(mappingsModel);

                                mappingsModel.LocationId = CurrentCompany.DefaultLocationID.Value;
                                mappingsModel.Data = FileImportService.GetData(CurrentCompany, CurrentUser);

                                PurchasingService.ValidateOrders(CurrentCompany, CurrentUser, mappingsModel.Data.Headings);
                                mappingsModel.Data = FileImportService.GetData(CurrentCompany, CurrentUser);

                                return View("ImportMappings", mappingsModel);
                            }

                        } catch (Exception e1) {
                            model.SetError(ErrorIcon.Error, EvolutionResources.errFailedToUploadFile.Replace("%1", fileName).Replace("%2", e1.Message));
                            LogService.WriteLog(e1, Request.RawUrl);
                        }
                    }

                    PrepareViewModel(model, EvolutionResources.bnrPurchaseImport, 0, 0);
                    return View("PurchaseImport", model);
                }
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.AllPurchasing)]
        [ValidateAntiForgeryToken]
        public ActionResult Import(EditPurchaseImportDataMappingsViewModel model, string command) {
            if (command.ToLower() == "import") {
                var error = PurchasingService.ValidateOrders(CurrentCompany, CurrentUser, model.Data.Headings);
                if (error.IsError) {
                    prepareViewModel(model);
                    model.Data = FileImportService.GetData(CurrentCompany, CurrentUser);
                    model.SetError(ErrorIcon.Error, error.Message);

                    return View("ImportMappings", model);

                } else {
                    error = PurchasingService.ImportOrders(CurrentCompany, 
                                                           CurrentUser,
                                                           model.LocationId, 
                                                           model.Data.Headings);
                    if (error.IsError) {
                        prepareViewModel(model);
                        model.Data = FileImportService.GetData(CurrentCompany, CurrentUser);
                        model.SetError(ErrorIcon.Error, error.Message);

                        return View("ImportMappings", model);

                    } else {
                        // Successfully imported, so redirect to purchase orders screen
                        return RedirectToAction("Purchases", "Purchasing", new { Area = "Purchasing" });
                    }
                }

            } else {
                return RedirectToAction("Index");
            }
        }

        private void prepareViewModel(EditPurchaseImportDataMappingsViewModel model) {
            PrepareViewModel(model, EvolutionResources.bnrPurchaseImport, 0, 0);

            model.LocationList = LookupService.FindLocationListItemModel(CurrentCompany);
            model.HeadingList = FileImportService.GetHeadingList("Purchase.dat");
        }
    }
}
