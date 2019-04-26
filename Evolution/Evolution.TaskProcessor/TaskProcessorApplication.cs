using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Evolution.DAL;
using Evolution.Enumerations;

namespace Evolution.TaskProcessor {
    public class TaskProcessorApplication {

        #region Private Members

        protected EvolutionEntities db = new EvolutionEntities();

        #endregion

        #region Main processing

        public void Run(string[] args) {

            if (args.Count() == 0) {
                WriteLog("Error: Task Processor started with no parameters!", LogSeverity.Warning);
                WriteLog("Usage: TASKPROCESSOR ACTION [parameters]", LogSeverity.Warning);

            } else {
                TaskBase task = null;

                string taskName = args[0].ToLower().Replace(" ", "");

                if (taskName == TaskName.HouseKeepingTask.ToLower().Replace(" ", "")) {
                    task = new HouseKeepingTask(db);

                } else if (taskName == TaskName.MailSenderTask.ToLower().Replace(" ", "")) {
                    task = new MailSenderTask(db);

                } else if (taskName == TaskName.DataTransferTask.ToLower().Replace(" ", "")) {
                    task = new DataTransferTask(db);

                } else if (taskName == TaskName.UnpackSlipReceiverTask.ToLower().Replace(" ", "")) {
                    task = new UnpackSlipReceiverTask(db);

                } else if (taskName == TaskName.CleanupAllTests.ToLower().Replace(" ", "")) {
                    task = new CleanupAllTestsTask(db);

                } else if (taskName == TaskName.NotificationTask.ToLower().Replace(" ", "")) {
                    task = new NotificationTask(db);

                } else if (taskName == TaskName.FileImportTask.ToLower().Replace(" ", "")) {
                    task = new NotificationTask(db);

                } else if (taskName == TaskName.PepperiImportTask.ToLower().Replace(" ", "")) {
                    task = new PepperiImportTask(db);

                } else if (taskName == TaskName.NuOrderImportTask.ToLower().Replace(" ", "")) {
                    task = new NuOrderImportTask(db);

                } else if (taskName == TaskName.ShopifyImportTask.ToLower().Replace(" ", "")) {
                    task = new ShopifyImportTask(db);

                } else {
                    WriteLog("Usage: TASKPROCESSOR ACTION [parameters]", LogSeverity.Warning);
                }

                if (task != null) task.Run(args);
            }
        }

        public void WriteLog(string message, LogSeverity severity = LogSeverity.Normal, LogSection logSection = LogSection.SystemError) {
            Console.WriteLine(message);
            db.WriteLog(logSection, severity, message);
        }

        #endregion
    }
}
