using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.IO;
using Evolution.DAL;
using Evolution.CommonService;
using Evolution.Models.Models;
using Evolution.CSVFileService;
using Evolution.Enumerations;
using Evolution.Extensions;
using Evolution.FileCompressionService;
using Evolution.FileManagerService;
using Evolution.EMailService;
using Evolution.MediaService;
using Evolution.PDFService;
using Evolution.TemplateService;
using Evolution.Resources;

namespace Evolution.FilePackagerService {
    public partial class FilePackagerService : CommonService.CommonService {

        #region Public methods

        public Error SendPickToWarehouse(PickHeaderModel pickM) {
            var error = new Error();

            var pickH = db.FindPickHeader(pickM.Id);
            if (pickH == null) {
                error.SetRecordError("PickHeader", pickM.Id);

            } else {
                var location = pickH.Location ?? new Location { Id = 0, LocationName = "[Unknown]" };
                var transferConfig = DataTransferService.FindFileTransferConfigurationModel(location.Id,
                                                                      FileTransferType.Send, 
                                                                      FileTransferDataType.WarehousePick);
                if (transferConfig == null) {
                    error.SetError(EvolutionResources.errCannotDropOrderNoDataTransfer, pickH.Id.ToString(), location.LocationName);

                } else {
                    error = processPick(pickM, transferConfig);
                }
            }
            return error;
        }

        #endregion

        #region Private members

        private Error processPick(PickHeaderModel pickH,
                                  FileTransferConfigurationModel template) {
            var error = new Error();

            // Load the template configuration file
            var configFile = getTemplateFileName(template);                   

            XElement doc = XElement.Load(configFile);
            var file = doc.Element("File");
            var extn = file.Attribute("DataFileExtension").Value;

            var tempFile = Path.GetTempPath() + pickH.Id + ".CSV";   // extn;
            var zipFile = "";

            // Check if the pick's files are to be compressed and sent in a ZIP.
            // A pick can be a single CSV file or a CSV with onr or more PDF's.
            // A single file can optionally be compressed whereas multiple files must be compressed
            // as a package of files.
            // The requirement is that a single file is dropped for FTP.
            bool bCompress = file.Attribute("CompressFile").Value.ParseBool();
            if (bCompress || pickH.PickFiles.Count() > 1) {
                // File(s) are to be compressed/combined
                zipFile = tempFile.ChangeExtension(".zip");

                error = Zip.ZipFiles(pickH.PickFiles, zipFile);

                if (error.IsError) {
                    FileManagerService.FileManagerService.DeleteFile(zipFile);
                } else {
                    tempFile = zipFile;
                }
            }

            if (!error.IsError) {
                if (file.Attribute("FTPFile").Value.ParseBool()) {
                    // Copy the file to the FTP pickup folder
                    error = moveFileToFTPFolder(tempFile,
                                                template.SourceFolder, 
                                                DataTransferService.GetTargetFileName(template,
                                                                                      tempFile,
                                                                                      pickH.Id));
                }
            }
            FileManagerService.FileManagerService.DeleteFile(tempFile);

            return error;
        }

        #endregion
    }
}
