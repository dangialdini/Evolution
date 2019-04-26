using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Xml;
using System.Xml.Serialization;
using Evolution.DAL;
using Evolution.Enumerations;
using Evolution.Models.Models;
using Evolution.PepperiImportService;

namespace Evolution.TaskProcessor
{
    public class FileImportTask : TaskBase
    {
        public FileImportTask(EvolutionEntities db) : base(db)
        {
        }

        public override string GetTaskName()
        {
            return TaskName.FileImportTask;
        }

        public override int DoProcessing(string[] args)
        {
            PepperiImportService.PepperiImportService pis = new PepperiImportService.PepperiImportService(_db);

            string fileLocation = GetTaskParameter("FileLoc", "");
            string businessName = GetTaskParameter("BusinessName", "");
            var taskUser = GetTaskUser();

            if(pis.ProcessXml(fileLocation, businessName, taskUser, this.Task)) {
               
            } else {
                TaskService.WriteTaskLog(this.Task, $"Error: There was a problem tyring to import files.", LogSeverity.Normal);
            }


            
            return 0;
        }
    }
}
