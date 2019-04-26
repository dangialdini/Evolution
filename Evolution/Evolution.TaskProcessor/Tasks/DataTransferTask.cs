using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Evolution.DAL;
using Evolution.TaskService;
using Evolution.SystemService;
using Evolution.Enumerations;
using Evolution.Extensions;
using Evolution.FTPService;
using Evolution.DataTransferService;
using Evolution.FileManagerService;
using Evolution.Models.Models;

namespace Evolution.TaskProcessor {
    public class DataTransferTask : TaskBase {

        public DataTransferTask(EvolutionEntities dbEntities) : base(dbEntities) { }

        public override string GetTaskName() { return TaskName.DataTransferTask; }

        public override int DoProcessing(string[] args) {
            // This service transfers files which have been placed in the folder specified
            // by the parameter profile

            // TaskProcessor.exe DATATRANSFERTASK datatransferprofilename
            if (args.Length != 2) {
                WriteTaskLog($"Error: Incorrect parameters!\r\nUsage: TaskProcessor.exe DATATRANSFERTASK datatransferprofilename");

            } else {
                DataTransferService.DataTransferService dts = new DataTransferService.DataTransferService(_db);

                var profileName = args[1];
                var config = dts.FindDataTransferConfigurationModel(profileName);
                if (config == null) {
                    WriteTaskLog($"Error: Parameter profile '{profileName}' could not be found!");

                } else {
                    WriteTaskLog($"Running transfer profile '{profileName}'");

                    if (config.TransferType == FileTransferType.Send) {
                        sendFiles(config);

                    } else if (config.TransferType == FileTransferType.Receive) {
                        receiveFiles(config);
                    }

                    // Handle the post-transfer processing
                    if (!string.IsNullOrEmpty(config.PostTransferCommand)) {
                        WriteTaskLog($"Running post-transfer process '{config.PostTransferCommand} {config.PostTransferParameters}'");

                        var error = SystemService.SystemService.WinExec(config.PostTransferCommand,
                                                                        config.PostTransferParameters);
                        if (error.IsError) WriteTaskLog(error.Message, LogSeverity.Severe);
                    }
                }
            }

            return 0;
        }

        private void sendFiles(FileTransferConfigurationModel config) {
            int numFiles = 0,
                numSuccess = 0,
                numErrors = 0;
            var error = new Error();

            WriteTaskLog($"Sending file(s) from '{config.SourceFolder}' to '{config.TargetFolder}'");

            // Start the FTP service
            var ftpService = new FTPService.FTPService(config.FTPHost, config.UserId, config.Password, config.Protocol);

            // Get a list of files on the local folder to send
            FileManagerService.FileManagerService.CreateFolder(config.SourceFolder);

            string[] fileList = null;
            try {
                fileList = Directory.GetFiles(config.SourceFolder);
                numFiles = fileList.Length;
            } catch(Exception e1) {
                WriteTaskLog(e1.Message, LogSeverity.Severe);
            }

            if(fileList != null) {
                foreach(var fileName in fileList) {
                    WriteTaskLog($"Sending '{fileName}' to '{config.TargetFolder}'");

                    string errorMsg = "";
                    if(ftpService.UploadFile(fileName, config.TargetFolder, ref errorMsg)) {
                        // Transfer failed
                        WriteTaskLog(errorMsg, LogSeverity.Severe);
                        numErrors++;

                        // Upon error, delete or archive local file
                        if (!string.IsNullOrEmpty(config.ErrorFolder)) {
                            // File is configured to be archived
                            error = FileManagerService.FileManagerService.MoveFile(fileName, config.ErrorFolder, true);

                        } else {
                            // No archive configuration, so just delete the file
                            error = FileManagerService.FileManagerService.DeleteFile(fileName);
                        }
                        if (error.IsError) WriteTaskLog(error.Message, LogSeverity.Severe);

                    } else {
                        // Transfer successful
                        WriteTaskLog(errorMsg, LogSeverity.Normal);
                        numSuccess++;

                        // Upon successful send, delete or archive local file
                        if (!string.IsNullOrEmpty(config.ArchiveFolder)) {
                            // File is configured to be archived
                            error = FileManagerService.FileManagerService.MoveFile(fileName, config.ArchiveFolder, true);

                        } else {
                            // No archive configuration, so just delete the file
                            error = FileManagerService.FileManagerService.DeleteFile(fileName);
                        }
                        if (error.IsError) WriteTaskLog(error.Message, LogSeverity.Severe);
                    }
                }
            }

            WriteTaskLog($"{numFiles} found, {numSuccess} sent successfully, {numErrors} error(s)");
        }

        private void receiveFiles(FileTransferConfigurationModel config) {
            int numFiles = 0,
                numSuccess = 0,
                numErrors = 0;

            WriteTaskLog($"Receiving file(s) from '{config.SourceFolder}' to '{config.TargetFolder}'");

            // Start the FTP service
            var ftpService = new FTPService.FTPService(config.FTPHost, config.UserId, config.Password, config.Protocol);

            // Get a list of files on the remote folder to receive
            string errorMsg = "";
            List<string> fileList = new List<string>();

            if(ftpService.GetFTPFileList(config.SourceFolder, ref fileList, ref errorMsg)) {
                WriteTaskLog(errorMsg, LogSeverity.Severe);

            } else {
                if(fileList != null) {
                    numFiles = fileList.Count();

                    string targetFolder = config.TargetFolder;
                    FileManagerService.FileManagerService.CreateFolder(targetFolder);

                    foreach (var fileName in fileList) {
                        // Receiving a file
                        string targetFile = targetFolder + "\\" + fileName.FileName();

                        WriteTaskLog($"Receiving '{fileName}' to '{targetFile}'");

                        if (ftpService.DownloadFile(fileName,
                                                    targetFile,
                                                    null,
                                                    ref errorMsg)) {
                            // Failed to perform transfer
                            WriteTaskLog(errorMsg, LogSeverity.Severe);
                            numErrors++;

                            // Upon error, delete or archive remote file
                            bool bError;
                            if (!string.IsNullOrEmpty(config.ErrorFolder)) {
                                // File is configured to be archived
                                bError = ftpService.MoveFile(fileName, config.ErrorFolder, ref errorMsg);

                            } else {
                                // No archive configuration, so just delete the file
                                bError = ftpService.DeleteFile(fileName, ref errorMsg);
                            }
                            if (bError) WriteTaskLog(errorMsg, LogSeverity.Severe);

                            // Delete the local file so we don't double-up
                            FileManagerService.FileManagerService.DeleteFile(targetFile);

                        } else {
                            // Transfer successful
                            WriteTaskLog(errorMsg, LogSeverity.Normal);
                            numSuccess++;

                            // Upon successful receipt, delete or archive remote file
                            bool bError;
                            if (!string.IsNullOrEmpty(config.ArchiveFolder)) {
                                // File is configured to be archived
                                bError = ftpService.MoveFile(fileName, config.ArchiveFolder, ref errorMsg);

                            } else {
                                // No archive configuration, so just delete the file
                                bError = ftpService.DeleteFile(fileName, ref errorMsg);
                            }
                            if (bError) WriteTaskLog(errorMsg, LogSeverity.Severe);
                        }
                    }
                }
            }

            WriteTaskLog($"{numFiles} found, {numSuccess} received successfully, {numErrors} error(s)");
        }
    }
}
