using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Evolution.DAL;
using Evolution.TaskService;
using Evolution.FTPService;
using Evolution.DataTransferService;
using Evolution.FileManagerService;
using Evolution.Models.Models;
using Evolution.Extensions;
using Evolution.Enumerations;

namespace Evolution.TaskProcessor {
    public class UnpackSlipReceiverTask : TaskBase {

        #region Construction

        public UnpackSlipReceiverTask(EvolutionEntities dbEntities) : base(dbEntities) { }

        public override string GetTaskName() { return TaskName.UnpackSlipReceiverTask; }

        #endregion

        #region Main processing loop

        UserModel user = null;

        public override int DoProcessing(string[] args) {
            // This task retrieves unpack slips from a warehouse.
            // It uses the transfers configured in the task properties.
            // TaskProcessor.exe UNPACKRECEIVER datatransferprofilename
            if (args.Length != 2) {
                WriteTaskLog($"Error: Incorrect parameters!\r\nUsage: TaskProcessor.exe UNPACKRECEIVER datatransferprofilename");

            } else {
                string errorMsg = "";

                user = GetTaskUser();
                if (user != null) {
                    DataTransferService.DataTransferService dts = new DataTransferService.DataTransferService(_db);

                    string taskName = args[1];
                    var config = dts.FindDataTransferConfigurationModel(taskName);
                    if (config == null) {
                        WriteTaskLog($"Warning: Task {taskName} cound not be found!", LogSeverity.Warning);

                    } else {
                        // Retrieve files from the warehouse
                        WriteTaskLog("Downloading files from: " + config.TransferName);
                        var ftpService = new FTPService.FTPService(config.FTPHost, config.UserId, config.Password, config.Protocol);

                        var fileList = new List<string>();
                        if (ftpService.GetFTPFileList(config.SourceFolder, ref fileList, ref errorMsg)) {
                            WriteTaskLog(errorMsg, LogSeverity.Severe);

                        } else {
                            // Download files one at a time and as each is successfully downloaded,
                            // archive of delete it on the FTP host.
                            // This ensures that if we have a comms failure, the local drive and FTP host are in
                            // sync and we prevent repeated loads of the same file.
                            FileManagerService.FileManagerService.CreateFolder(config.TargetFolder.TrimEnd('\\'));

                            foreach (var fileName in fileList) {
                                var downloadedFile = new List<string>();
                                var result = ftpService.DownloadFile(fileName,
                                                                     config.TargetFolder.TrimEnd('\\') + "\\" + fileName.FileName(),
                                                                     downloadedFile,
                                                                     ref errorMsg);
                                _db.LogTestFile(downloadedFile);
                                if (result) {
                                    WriteTaskLog(errorMsg, LogSeverity.Severe);
                                } else {
                                    // Delete or archive the source files of those which were retrieved
                                    result = ftpService.DeleteFile(fileName, ref errorMsg);
                                    if(result) WriteTaskLog(errorMsg, LogSeverity.Severe);
                                }
                            }

                            // Process the files received
                            processFiles(config);
                        }
                    }
                }
            }

            return 0;
        }

        void processFiles(FileTransferConfigurationModel config) {
            WriteTaskLog("Processing received files");

            foreach (var fileName in Directory.GetFiles(config.TargetFolder.TrimEnd('\\'))) {
                bool bError = false;
                string errorMsg = "";

                // Open the file to get the purchase order number from the first data line
                CSVFileService.CSVReader reader = new CSVFileService.CSVReader();

                reader.OpenFile(fileName, true);
                Dictionary<string, string> firstLine = reader.ReadLine();
                reader.Close();

                int poNumber = -1;
                if (Int32.TryParse(firstLine["PO_NUMBER"], out poNumber)) {
                    // Now we know what the PO Number is, attach the file to the purchase
                    // order notes/attachments so that it can be referenced in emails
                    CompanyModel company = new CompanyModel { Id = config.CompanyId };
                    var poh = PurchasingService.FindPurchaseOrderHeaderByPONumberModel(poNumber, company);
                    if (poh != null) {
                        var note = new NoteModel();
                        bError = createNoteWithAttachment(poh, company, fileName, ref note, ref errorMsg);
                        if (!bError) {
                            // A purchase order records who the 'sales person' was - actually the 'purchasor'.
                            // We need to email every user in the same team as the sales person to notify them
                            // of the slip file being received.

                            // To do this, we need to find what user groups the sales person is in and specifically
                            // look for one which has the name of the brand category of the order in it.
                            // We then email all users in that group. This ensures that when an order is received,
                            // the sales person's collegues are informed so that we don't create a situation where
                            // only one person knows about an order.

                            var userList = PurchasingService.FindOrderPurchasers(poh, 
                                                                                 company, 
                                                                                 poNumber,
                                                                                 ref errorMsg);
                            if(userList != null) {
                                // Send emails to all the users in the list
                                var error = sendUnpackSlipMessage(poh, company, note, poNumber, userList);
                                if(error.IsError) {
                                    errorMsg = error.Message;
                                    bError = true;
                                }

                            } else {
                                bError = true;
                            }
                        }

                    } else {
                        errorMsg = $"Error: Failed to find the PurchaseOrderHeaderRecord corresponding to Order Number {poNumber} !";
                        bError = true;
                    }

                } else {
                    errorMsg = $"Error: Failed to read PO_NUMBER column from '{fileName}' !";
                    bError = true;
                }

                if (bError) {
                    // Failed to process the file so move it to the error folder
                    // Move the file to an error location.
                    // It may not exist if note/attachment creation has already moved it.
                    // It can be moved back in for later re-processing.
                    WriteTaskLog(errorMsg, LogSeverity.Severe);

                    string moveTarget = fileName.FolderName() + "\\Errors";
                    FileManagerService.FileManagerService.DeleteFile(moveTarget + "\\" + fileName.FileName());
                    var error = FileManagerService.FileManagerService.MoveFile(fileName, moveTarget, false);
                    if(error.IsError) WriteTaskLog(error.Message, LogSeverity.Severe);
                }
            }
        }

        bool createNoteWithAttachment(PurchaseOrderHeaderModel poh, CompanyModel company,
                                      string fileName, ref NoteModel note, ref string errorMsg) {
            bool bError = false;

            note = new NoteModel {
                CompanyId = company.Id,
                NoteType = NoteType.Purchase,
                ParentId = poh.Id,
                DateCreated = DateTimeOffset.Now,
                CreatedById = user.Id,
                Subject = "Unpack Slip - Order " + poh.OrderNumber.ToString(),
                Message = ""
            };

            var error = NoteService.InsertOrUpdateNote(note, user, "");
            if (error.IsError) {
                errorMsg = error.Message;
                bError = true;

            } else {
                // Move the file to the purchase order note attachment folder for the
                // specific purchase order
                error = NoteService.AttachMediaItemToNote(note,
                                                          user, 
                                                          fileName, 
                                                          fileName.FileName(),
                                                          FileCopyType.Move);
                if (error.IsError) {
                    errorMsg = error.Message;
                    bError = true;
                }
            }

            return bError;
        }

        Error sendUnpackSlipMessage(PurchaseOrderHeaderModel poh,
                                         CompanyModel company,      // Company of the file transfer
                                         NoteModel note,
                                         int poNumber, 
                                         List<UserModel> userList) {

            Dictionary<string, string> dict = new Dictionary<string, string>();
            dict.AddProperty("PURCHASEORDERNO", poNumber);
            dict.AddProperty("COMPANYNAME", company.FriendlyName);

            UserModel purchaser = MembershipManagementService.FindUserModel(poh.SalespersonId.Value);
            dict.AddProperty("PURCHASER", (purchaser == null ? "" : (purchaser.FirstName + " " + purchaser.LastName).Trim()));

            dict.AddProperty("SUPPLIER", poh.SupplierName);

            var attachment = NoteService.FindNoteAttachmentsModel(note, MediaSize.Medium, 0, 0).FirstOrDefault();

            string url = "";
            if (attachment != null) url = MediaService.GetMediaFileName(attachment.Media, true);
            dict.AddProperty("URL", url);

            return SendMessage(company,
                               user, 
                               MessageTemplateType.UnpackSlipNotification,
                               TaskType.Default,
                               userList, 
                               dict);
        }

        #endregion
    }
}
