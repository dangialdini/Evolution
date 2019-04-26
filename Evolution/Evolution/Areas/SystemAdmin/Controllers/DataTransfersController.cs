using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Security.Permissions;
using Evolution.Security;
using Evolution.Controllers.Application;
using Evolution.Models.ViewModels;
using Evolution.Enumerations;
using Evolution.Models.Models;
using Evolution.Resources;

namespace Evolution.Areas.SystemAdmin.Controllers
{
    public class DataTransfersController : BaseController
    {
        // GET: SystemAdmin/DataTransfers
        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.SuperUser)]
        public ActionResult Index() {
            var model = createModel();
            return View("DataTransfers", model);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.SuperUser)]
        public ActionResult DataTransfers() {
            var model = createModel();
            return View("DataTransfers", model);
        }

        ViewModelBase createModel() {
            var model = new ViewModelBase();
            PrepareViewModel(model, EvolutionResources.bnrDataTransfers);

            return model;
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.SuperUser)]
        public ActionResult GetDataTransfers(int index, int pageNo, int pageSize, string search) {
            return Json(DataTransferService.FindDataTransferConfigurationsListModel(index, pageNo, pageSize, search), JsonRequestBehavior.AllowGet);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.SuperUser)]
        public ActionResult Edit(int id) {
            var model = new EditFileTransferConfigurationViewModel();
            prepareEditModel(model);

            model.FileTransferConfiguration = DataTransferService.FindDataTransferConfigurationModel(id, true);
            model.LGS = DataTransferService.LockDataTransferConfiguration(model.FileTransferConfiguration);

            return View(model);
        }

        void prepareEditModel(EditFileTransferConfigurationViewModel model) {
            PrepareViewModel(model, EvolutionResources.bnrAddEditDataTransfer);

            model.CompanyList = CompanyService.FindCompaniesListItemModel();
            model.TransferTypeList = LookupService.FindFileTransferTypeListItemModel();
            model.DataTypeList = LookupService.FindLOVItemsListItemModel(CurrentCompany, LOVName.FileTransferDataType);
            model.FTPProtocolList = LookupService.FindFTPProtocolListItemModel();
            model.LocationList = LookupService.FindLocationListItemModel(model.CurrentCompany, true);
            model.FreightForwarderList = LookupService.FindFreightForwardersListItemModel(model.CurrentCompany, true);
            model.ConfigurationTemplateList = DataTransferService.FindDataTransferTemplatesListItemModel();
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.SuperUser)]
        public ActionResult Delete(int index, int id) {
            var model = new FileTransferConfigurationListModel { GridIndex = index };
            try {
                DataTransferService.DeleteDataTransferConfiguration(id);
            } catch (Exception e1) {
                model.Error.SetError(e1);
            }
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        [ValidateAntiForgeryToken]
        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.SuperUser)]
        public ActionResult Save(EditFileTransferConfigurationViewModel model, string command) {
            if (command.ToLower() == "save") {
                var modelError = DataTransferService.InsertOrUpdateDataTransferConfiguration(model.FileTransferConfiguration, CurrentUser, model.LGS);
                if (modelError.IsError) {
                    PrepareViewModel(model, EvolutionResources.bnrAddEditDataTransfer);
                    model.SetErrorOnField(ErrorIcon.Error,
                                          modelError.Message,
                                          "FileTransferConfiguration_" + modelError.FieldName);
                    return View("Edit", model);

                } else {
                    return RedirectToAction("DataTransfers");
                }

            } else {
                return RedirectToAction("DataTransfers");
            }
        }
    }
}
