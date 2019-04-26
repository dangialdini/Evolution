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
using System.IO;

namespace Evolution.TaskProcessor {
    public class PepperiImportTask : TaskBase {

        public PepperiImportTask(EvolutionEntities dbEntities) : base(dbEntities) { }

        public override string GetTaskName() { return TaskName.PepperiImportTask; }

        public override int DoProcessing(string[] args) {
            var pepperiImportService = new PepperiImportService.PepperiImportService(_db);
            DataTransferService.DataTransferService dts = new DataTransferService.DataTransferService(_db);

            int i = 1;
            var profileName = GetTaskParameter($"DataTransfer{i}", "");
            var taskUser = GetTaskUser();
            string errorMsg = "";

            var config = dts.FindDataTransferConfigurationModel(profileName);
            if (config == null) {
                TaskService.WriteTaskLog(this.Task, $"Error: Failed to find Data Transfer configuration '{profileName}' !");

            } else {
                string fileLoc = config.TargetFolder;
                while (!string.IsNullOrEmpty(fileLoc)) {
                    string businessName = GetTaskParameter($"BusinessName{i}", "");

                    string[] files = Directory.GetFiles(fileLoc);
                    if (files.Length > 0 && files != null) {
                        foreach (string fileName in files) {
                            if (pepperiImportService.ProcessXml(fileName, businessName, taskUser, this.Task)) {
                                if (dts.MoveToArchive(config, fileName, ref errorMsg)) {
                                    TaskService.WriteTaskLog(this.Task, $"Failed to move to Archive folder/r/n{errorMsg}", LogSeverity.Severe);
                                } else {
                                    TaskService.WriteTaskLog(this.Task, $"Successfully moved file '{fileName}' to Archive folder", LogSeverity.Normal);
                                }
                            } else {
                                if (dts.MoveToError(config, fileName, ref errorMsg)) {
                                    TaskService.WriteTaskLog(this.Task, $"Failed to move to Error folder/r/n{errorMsg}", LogSeverity.Severe);
                                } else {
                                    TaskService.WriteTaskLog(this.Task, $"File '{fileName}' has been moved to the Error folder", LogSeverity.Severe);
                                }
                            }
                        }
                    } else {
                        TaskService.WriteTaskLog(this.Task, $"Warning: There are no files to import", LogSeverity.Normal);
                    }

                    i++;
                    profileName = GetTaskParameter($"DataTransfer{i}", "");
                    config = dts.FindDataTransferConfigurationModel(profileName);
                    fileLoc = (config != null) ? config.TargetFolder : "";
                }
            }
            return 0;
        }
    }
}
