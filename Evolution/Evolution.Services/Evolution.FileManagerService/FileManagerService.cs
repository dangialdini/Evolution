using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Evolution.DAL;
using Evolution.Models.Models;
using Evolution.Extensions;

namespace Evolution.FileManagerService
{
    public class FileManagerService
    {
        public static Error CreateFolder(string folderName) {
            var error = new Error();
            try {
                Directory.CreateDirectory(folderName);
            } catch (Exception e1) {
                error.SetError(e1.Message);
            }
            return error;
        }

        public static Error DeleteFile(string fileName) {
            var error = new Error();
            try {
                File.Delete(fileName);
            } catch(Exception e1) {
                error.SetError(e1.Message);
            }
            return error;
        }

        public static Error MoveFile(string sourceFileName, string targetFolder, bool bCreateFolder) {
            var error = new Error();
            try {
                if (bCreateFolder) CreateFolder(targetFolder);

                string targetFileName = targetFolder + "\\" + sourceFileName.FileName();

                DeleteFile(targetFileName);
                File.Move(sourceFileName, targetFileName);

            } catch (Exception e1) {
                error.SetError(e1.Message);
            }
            return error;
        }
    }
}
