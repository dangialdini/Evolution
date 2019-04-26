using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Web.Configuration;
using System.Configuration;
using Evolution.CommonService;
using Evolution.DAL;
using Evolution.Models.Models;
using Evolution.Extensions;
using Evolution.Enumerations;
using Evolution.Resources;
using AutoMapper;
using System.Drawing;

namespace Evolution.MediaService {
    public partial class MediaService : CommonService.CommonService {

        public void AddFileToLog(string fileName, int minutes) {
            db.AddFileToLog(fileName, minutes);
        }

        public void AddFolderToLog(string fileName, int minutes) {
            db.AddFolderToLog(fileName, minutes);
        }

        public void CleanFileLog(ref int folderDeletes, ref int fileDeletes, bool bAll = false) {
            db.CleanupFileLogs(ref folderDeletes, ref fileDeletes, bAll);
        }
    }
}
