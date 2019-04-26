using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Evolution.DAL;
using Evolution.TaskService;
using Evolution.Models.Models;
using Evolution.Extensions;
using Evolution.Enumerations;
using Evolution.NuOrderImportService;
using System.IO;

namespace Evolution.TaskProcessor {
    public class NuOrderImportTask : TaskBase {

        public NuOrderImportTask(EvolutionEntities dbEntities) : base(dbEntities) { }

        public override string GetTaskName() { return TaskName.NuOrderImportTask; }

        public override int DoProcessing(string[] args) {
            var nuOrderImportService = new NuOrderImportService.NuOrderImportService(_db);
            DataTransferService.DataTransferService dts = new DataTransferService.DataTransferService(_db);

            int i = 1;
            var profileName = GetTaskParameter($"DataTransfer{1}", "");
            var taskUser = GetTaskUser();
            string errorMsg = "";

            var config = dts.FindDataTransferConfigurationModel(profileName);
            if (config == null) {
                TaskService.WriteTaskLog(this.Task, $"Error: Failed to find Data Transfer configuration '{profileName}' !");

            } else {
                string fileLoc = config.TargetFolder;
                while (!string.IsNullOrEmpty(fileLoc)) {
                    string businessName = GetTaskParameter($"BusinessName{i}", ""); ;
                    string[] files = null;
                    try {
                        files = Directory.GetFiles(fileLoc);
                    } catch (Exception ex) {
                        TaskService.WriteTaskLog(this.Task, $"Error: There was a problem getting files from '{fileLoc}'\r\n" + ex, LogSeverity.Severe);
                    }

                    if (files.Length > 0 && files != null) {
                        foreach (string fileName in files) {

                            // ProcessFile
                            TaskService.WriteTaskLog(this.Task, $"Success: Processing file '{fileName}'", LogSeverity.Normal);
                            List<Dictionary<string, string>> orderLines = null;
                            try {
                                orderLines = nuOrderImportService.ProcessFile(fileName, businessName);
                            } catch (Exception ex) {
                                TaskService.WriteTaskLog(this.Task, $"Error: Could not process file '{fileName}'\r\n" + ex, LogSeverity.Severe);
                            }

                            if (orderLines != null || orderLines.Count == 0) {
                                List<NuOrderImportTemp> nuOrderImportTempList = new List<NuOrderImportTemp>();

                                // MapFileToTemp
                                try {
                                    nuOrderImportTempList = nuOrderImportService.MapFileToTemp(businessName, orderLines, taskUser);
                                    TaskService.WriteTaskLog(this.Task, $"Success: Saved '{fileName}' temp table", LogSeverity.Normal);

                                    // GetTempTableData
                                    try {
                                        nuOrderImportTempList = nuOrderImportService.GetTempTableData();
                                    } catch (Exception ex) {
                                        TaskService.WriteTaskLog(this.Task, $"Error: Failed to get temp data from database\r\n" + ex, LogSeverity.Severe);
                                    }

                                    // CopyTempToProduction & Move to file to appropriate folder
                                    if (nuOrderImportTempList.Count > 0) {
                                        try {
                                            if (nuOrderImportService.CopyTempDataToProduction(nuOrderImportTempList, businessName)) {
                                                TaskService.WriteTaskLog(this.Task, $"Success: Data Saved to Sales tables", LogSeverity.Normal);

                                                if (dts.MoveToArchive(config, fileName, ref errorMsg)) {
                                                    TaskService.WriteTaskLog(this.Task, $"Failed to move to Archive folder\r\n{errorMsg}", LogSeverity.Severe);
                                                } else {
                                                    TaskService.WriteTaskLog(this.Task, $"Successfully moved file '{fileName}' to Archive folder", LogSeverity.Normal);
                                                }
                                            } else {
                                                MoveFileToErrorFolder(dts, config, fileName);
                                            }
                                        } catch (Exception ex) {
                                            TaskService.WriteTaskLog(this.Task, $"Error: Could not copy data to sales table/s\r\n" + ex, LogSeverity.Severe);
                                            MoveFileToErrorFolder(dts, config, fileName);
                                        }
                                    }

                                } catch (Exception ex) {
                                    TaskService.WriteTaskLog(this.Task, $"Error: Failed to map '{fileName}'\r\n" + ex, LogSeverity.Severe);
                                    MoveFileToErrorFolder(dts, config, fileName);
                                }
                            } else {
                                TaskService.WriteTaskLog(this.Task, $"Error: The file '{fileName}' was empty", LogSeverity.Severe);
                                MoveFileToErrorFolder(dts, config, fileName);
                            }
                        }
                    } else {
                        TaskService.WriteTaskLog(this.Task, $"INFO: There were no files to process.'", LogSeverity.Normal);
                    }
                    i++;
                    profileName = GetTaskParameter($"DataTransfer{i}", "");
                    config = dts.FindDataTransferConfigurationModel(profileName);
                    fileLoc = (config != null) ? config.TargetFolder : "";
                }
            }
            return 0;
        }

        private void MoveFileToErrorFolder(DataTransferService.DataTransferService dts, FileTransferConfigurationModel config, string fileName) {
            string errorMsg = "";
            if (dts.MoveToError(config, fileName, ref errorMsg)) {
                TaskService.WriteTaskLog(this.Task, $"Failed to move to Error folder\r\n{errorMsg}", LogSeverity.Severe);
            } else {
                TaskService.WriteTaskLog(this.Task, $"File '{fileName}' has been moved to the Error folder", LogSeverity.Severe);
            }
        }
    }
}
