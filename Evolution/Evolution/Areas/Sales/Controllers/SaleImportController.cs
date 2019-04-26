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
using System.Diagnostics;

namespace Evolution.Areas.Sales.Controllers {
    public class SaleImportController : BaseController {
        // GET: Sales/SaleImport
        public ActionResult Index() {
            return SaleImport();
        }

        public ActionResult SaleImport() {
            var model = createModel();
            return View("SaleImport", model);
        }

        ViewModelBase createModel() {
            var model = new FileUploadViewModel();
            prepareViewModel(model);
            return model;
        }

        void prepareViewModel(FileUploadViewModel model) {
            PrepareViewModel(model,
                             EvolutionResources.bnrSaleImport,
                             0,
                             MenuOptionFlag.RequiresNoSale);
            model.MaxUploadFileSize = GetMaxFileUploadSize();
            model.ValidFileTypes = MediaServices.GetValidOrderImportTypes();

            model.ColumnHeadings = "";
            var headingList = FileImportService.GetHeadingList("Sale.dat", false, false);
            for (int i = 0; i < headingList.Count(); i++) {
                if (i > 0) model.ColumnHeadings += ", ";
                model.ColumnHeadings += headingList[i].Id;
            }
        }

        [HttpPost]
        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.Sales)]
        [ValidateAntiForgeryToken]
        public ActionResult Upload(FileUploadViewModel model, string command) {
            Server.ScriptTimeout = 600;     // Allow 10 minutes for an upload/import

            if (command.ToLower() == "upload") {
                if (model.Files != null && model.Files.Count() > 0) {

                    var targetFile = Path.GetTempPath();

                    var attachment = model.Files.FirstOrDefault();
                    string fileName = attachment.FileName;

                    if (!MediaServices.IsValidOrderImportType(fileName)) {
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
                                var mappingsModel = new EditSaleImportDataMappingsViewModel();
                                prepareViewModel(mappingsModel);

                                mappingsModel.Data = FileImportService.GetData(CurrentCompany, CurrentUser);

                                SalesService.ValidateOrders(CurrentCompany, CurrentUser, mappingsModel.Data.Headings, mappingsModel.DisplayDateFormat);

                                mappingsModel.Data = FileImportService.GetData(CurrentCompany, CurrentUser);

                                return View("ImportMappings", mappingsModel);
                            }

                        } catch (Exception e1) {
                            model.SetError(ErrorIcon.Error, EvolutionResources.errFailedToUploadFile.Replace("%1", fileName).Replace("%2", e1.Message));
                            LogService.WriteLog(e1, Request.RawUrl);
                        }
                    }

                    prepareViewModel(model);
                    return View("SaleImport", model);
                }
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.AllPurchasing)]
        [ValidateAntiForgeryToken]
        public ActionResult Import(EditSaleImportDataMappingsViewModel model, string command) {
            if (command.ToLower() == "import") {
                var error = SalesService.ValidateOrders(CurrentCompany, CurrentUser, model.Data.Headings, CurrentUser.DateFormat);
                if (error.IsError) {
                    prepareViewModel(model);
                    model.Data = FileImportService.GetData(CurrentCompany, CurrentUser);
                    model.SetError(ErrorIcon.Error, error.Message);

                    return View("ImportMappings", model);

                } else {
                    error = SalesService.ImportSales(CurrentCompany,
                                                     CurrentUser,
                                                     model.SOStatus,
                                                     model.SourceId,
                                                     model.Data.Headings,
                                                     CurrentUser.DateFormat,
                                                     model.TZ.ParseInt());
                    if (error.IsError) {
                        prepareViewModel(model);
                        model.Data = FileImportService.GetData(CurrentCompany, CurrentUser);
                        model.SetError(ErrorIcon.Error, error.Message);

                        return View("ImportMappings", model);

                    } else {
                        // Successfully imported, so redirect to purchase orders screen
                        return RedirectToAction("Sales", "Sales", new { Area = "Sales" });
                    }
                }

            } else {
                return RedirectToAction("Index");
            }
        }

        private void prepareViewModel(EditSaleImportDataMappingsViewModel model) {
            PrepareViewModel(model, EvolutionResources.bnrSaleImport, 0, 0);

            model.HeadingList = FileImportService.GetHeadingList("Sale.dat");
            model.SOStatusList = LookupService.FindSalesOrderHeaderStatusListItemModel();
            model.SourceList = LookupService.FindLOVItemsListItemModel(CurrentCompany, LOVName.OrderSource);
        }
    }
}
