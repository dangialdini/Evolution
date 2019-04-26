using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Evolution.CommonService;
using Evolution.DAL;
using Evolution.Models.Models;
using Evolution.Extensions;
using Evolution.Enumerations;
using Evolution.Resources;
using AutoMapper;

namespace Evolution.DataTransferService {
    public partial class DataTransferService : CommonService.CommonService {

        #region Public members    

        public List<FileTransferConfigurationModel> FindDataTransferConfigurationsModel(bool bShowHidden = false) {
            List<FileTransferConfigurationModel> model = new List<FileTransferConfigurationModel>();
            foreach (var transfer in db.FindFileTransferConfigurations(bShowHidden)) {
                model.Add(MapToModel(transfer));
            }
            return model;
        }

        public List<ListItemModel> FindDataTransferTemplatesListItemModel() {
            List<ListItemModel> model = new List<ListItemModel>();

            string templateFolder = GetConfigurationSetting("SiteFolder", "") + "\\App_Data\\DataTransferTemplates";
            foreach(var file in Directory.GetFiles(templateFolder, "*.xml")) {
                model.Add(new ListItemModel(file.FileName(), file.FileName()));
            }
            model.Insert(0, new ListItemModel(EvolutionResources.lblNone, "0"));

            return model;
        }

        public FileTransferConfigurationListModel FindDataTransferConfigurationsListModel(int index, int pageNo, int pageSize, string search) {
            var model = new FileTransferConfigurationListModel { GridIndex = index };

            // Do a case-insensitive search
            var allItems = db.FindFileTransferConfigurations(true)
                             .Where(pl => string.IsNullOrEmpty(search) ||
                                          (pl.TransferName != null && pl.TransferName.ToLower().Contains(search.ToLower()) ||
                                          (pl.FTPHost != null && pl.FTPHost.ToLower().Contains(search.ToLower())) ||
                                          (pl.UserId != null && pl.UserId.ToLower().Contains(search.ToLower())) ||
                                          (pl.Password != null && pl.Password.ToLower().Contains(search.ToLower())) ||
                                          (pl.SourceFolder != null && pl.SourceFolder.ToLower().Contains(search.ToLower())) ||
                                          (pl.TargetFolder != null && pl.TargetFolder.ToLower().Contains(search.ToLower())) ||
                                          (pl.ConfigurationTemplate != null && pl.ConfigurationTemplate.ToLower().Contains(search.ToLower()))))
                             .ToList();

            model.TotalRecords = allItems.Count();
            foreach(var transfer in allItems.Skip((pageNo - 1) * pageSize)
                                            .Take(pageSize)) {
                model.Items.Add(MapToModel(transfer));
            }
            return model;
        }

        public FileTransferConfigurationModel FindDataTransferConfigurationModel(string transferName, bool bCreateIfNotFound = false) {
            var item = db.FindFileTransferConfiguration(transferName);
            if (item == null && bCreateIfNotFound) item = new FileTransferConfiguration();
            return MapToModel(item);
        }

        public FileTransferConfigurationModel FindDataTransferConfigurationModel(int id, bool bCreateIfNotFound = false) {
            var item = db.FindFileTransferConfiguration(id);
            if (item == null && bCreateIfNotFound) item = new FileTransferConfiguration();
            return MapToModel(item);
        }

        public FileTransferConfigurationModel FindDataTransferConfigurationModel(SalesOrderHeaderModel soh) {
            var item = db.FindFileTransferConfigurations()
                         .Where(ftc => ftc.CompanyId == soh.CompanyId &&
                                       ftc.LocationId == soh.LocationId)
                         .FirstOrDefault();
            return MapToModel(item);
        }

        public FileTransferConfigurationModel FindFileTransferConfigurationModel(int locationId,
                                                                                 FileTransferType transferType,
                                                                                 FileTransferDataType transferDataType) {
            var item = db.FindFileTransferConfigurations()
                         .Where(ftc => ftc.LocationId == locationId &&
                                       ftc.TransferType == (int)transferType &&
                                       ftc.LOVItem_DataType.ItemValue1 == ((int)transferDataType).ToString() &&
                                       ftc.Enabled == true)
                         .FirstOrDefault();
            return MapToModel(item);
        }

        public FileTransferConfigurationModel FindFileTransferConfigurationForWarehouseModel(LocationModel warehouse,
                                                                                             FileTransferDataType dataType) {
            return FindFileTransferConfigurationModel(warehouse.Id,
                                                      FileTransferType.Send,
                                                      dataType);
        }

        public FileTransferConfigurationModel FindFileTransferConfigurationForFreightForwarder(FreightForwarderModel freightForearder,
                                                                                               FileTransferDataType dataType) {
            var item = db.FindFileTransferConfigurations()
                         .Where(ftc => ftc.FreightForwarderId == freightForearder.Id &&
                                       ftc.TransferType == (int)FileTransferType.Send &&
                                       ftc.LOVItem_DataType.ItemValue1 == ((int)dataType).ToString() &&
                                       ftc.Enabled == true)
                         .FirstOrDefault();
            return MapToModel(item);
        }

        public FileTransferConfigurationModel MapToModel(FileTransferConfiguration config) {
            if (config == null) {
                return null;
            } else {
                var newItem = Mapper.Map<FileTransferConfiguration, FileTransferConfigurationModel>(config);

                //newItem.DataTransferFolder = GetConfigurationSetting("DataTransferFolder", "");

                if (config.User != null) newItem.CreatedByText = config.User.Name;

                newItem.TransferTypeText = ((FileTransferType)config.TransferType).ToString();
                newItem.DataTypeText = (config.LOVItem_DataType == null ? "" : config.LOVItem_DataType.ItemText);
                newItem.ProtocolText = ((FTPProtocol)config.Protocol).ToString();

                return newItem;
            }
        }

        private void mapToEntity(FileTransferConfigurationModel model, FileTransferConfiguration entity) {
            Mapper.Map<FileTransferConfigurationModel, FileTransferConfiguration>(model, entity);
        }

        public Error InsertOrUpdateDataTransferConfiguration(FileTransferConfigurationModel config, UserModel user, string lockGuid = "") {
            var error = validateModel(config);
            if (!error.IsError) {
                // Check that the lock is still current
                if (!db.IsLockStillValid(typeof(FileTransferConfiguration).ToString(), config.Id, lockGuid)) {
                    error.SetError(EvolutionResources.errRecordChangedByAnotherUser, "TransferName");

                } else {
                    FileTransferConfiguration temp = null;
                    if (config.Id != 0) {
                        temp = db.FindFileTransferConfiguration(config.Id);
                    } else {
                        temp = db.FindFileTransferConfiguration(config.TransferName);
                    }
                    if (temp == null) temp = new FileTransferConfiguration();

                    mapToEntity(config, temp);

                    if(temp.CreatedById == 0) {
                        temp.CreatedById = user.Id;
                        temp.CreatedDate = DateTimeOffset.Now;
                    }

                    temp.Location = db.FindLocation(temp.LocationId ?? 0);
                    temp.FreightForwarder = db.FindFreightForwarder(temp.FreightForwarderId ?? 0);
                    if (temp.ConfigurationTemplate == "0") temp.ConfigurationTemplate = "";

                    db.InsertOrUpdateFileTransferConfiguration(temp);
                    config.Id = temp.Id;
                }
            }
            return error;
        }

        public void DeleteDataTransferConfiguration(int id) {
            db.DeleteFileTransferConfiguration(id);
        }

        public string LockDataTransferConfiguration(FileTransferConfigurationModel model) {
            return db.LockRecord(typeof(FileTransferConfiguration).ToString(), model.Id);
        }

        public bool MoveToArchive(FileTransferConfigurationModel transferConfig, string fileName,
                                  ref string errorMsg) {
            // Used to move a file to its transfer's Archive folder
            return moveFile(transferConfig, fileName, transferConfig.ArchiveFolder, ref errorMsg);
        }

        public bool MoveToError(FileTransferConfigurationModel transferConfig, string fileName,
                                ref string errorMsg) {
            // Used to move a file to its transfer's Error folder
            return moveFile(transferConfig, fileName, transferConfig.ErrorFolder, ref errorMsg);
        }

        public string GetTargetFileName(FileTransferConfigurationModel config,
                                        string qualSourceFile,
                                        int pickNo,
                                        int orderNo = 0,
                                        int invoiceNo = 0) {
            string rc = config.TargetFolder,
                   tempExtn = "CSV";
            if(config.TransferType == FileTransferType.Send) {
                rc = rc.AddString("/");
            } else {
                rc = rc.AddString("\\");
            }
            if (!string.IsNullOrEmpty(config.TargetFileNameFormat)) {
                // If the template specifies a format, use it
                rc += config.TargetFileNameFormat;
                tempExtn = config.TargetFileNameFormat.FileExtension();
            } else {
                // Otherwise, use the filename provided
                rc += qualSourceFile.FileName();
            }
            rc = rc.Replace("{PICKNO}", pickNo.ToString());
            rc = rc.Replace("{ORDERNO}", orderNo.ToString());
            rc = rc.Replace("{INVOICENO}", invoiceNo.ToString());
            rc = rc.Replace("{EXTN}", tempExtn);

            return rc;
        }

        #endregion

        #region Private methods

        private Error validateModel(FileTransferConfigurationModel model) {
            var error = isValidRequiredString(getFieldValue(model.TransferName), 50, "TransferName", EvolutionResources.errTextDataRequiredInField);
            if (!error.IsError) error = isValidNonRequiredString(getFieldValue(model.FTPHost), 100, "FTPHost", EvolutionResources.errTextDataRequiredInField);
            if (!error.IsError) error = isValidNonRequiredString(getFieldValue(model.UserId), 50, "UserId", EvolutionResources.errTextDataRequiredInField);
            if (!error.IsError) error = isValidNonRequiredString(getFieldValue(model.Password), 50, "Password", EvolutionResources.errTextDataRequiredInField);
            if (!error.IsError) error = isValidNonRequiredString(getFieldValue(model.SourceFolder), 255, "SourceFolder", EvolutionResources.errTextDataRequiredInField);
            if (!error.IsError) error = isValidNonRequiredString(getFieldValue(model.TargetFolder), 64, "TargetFileNameFormat", EvolutionResources.errTextDataRequiredInField);
            if (!error.IsError) error = isValidNonRequiredString(getFieldValue(model.TargetFolder), 255, "TargetFolder", EvolutionResources.errTextDataRequiredInField);
            if (!error.IsError) error = isValidNonRequiredString(getFieldValue(model.ArchiveFolder), 255, "ArchiveFolder", EvolutionResources.errTextDataRequiredInField);
            if (!error.IsError) error = isValidNonRequiredString(getFieldValue(model.ConfigurationTemplate), 64, "ConfigurationTemplate", EvolutionResources.errTextDataRequiredInField);
            if (!error.IsError) error = isValidNonRequiredString(getFieldValue(model.PostTransferCommand), 128, "PostTransferCommand", EvolutionResources.errTextDataRequiredInField);
            if (!error.IsError) error = isValidNonRequiredString(getFieldValue(model.PostTransferParameters), 128, "PostTransferParameters", EvolutionResources.errTextDataRequiredInField);

            return error;
        }

        private bool moveFile(FileTransferConfigurationModel transferConfig,
                              string sourceQualName, string targetFolder,
                              ref string errorMsg) {
            bool bRc = false;

            // Create the folder
            FileManagerService.FileManagerService.CreateFolder(targetFolder);

            // Move the file
            var error = FileManagerService.FileManagerService.MoveFile(sourceQualName, targetFolder, true);
            if (error.IsError) {
                errorMsg = error.Message;
                bRc = true;
            }

            return bRc;
        }

        #endregion
    }
}
