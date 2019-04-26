using System;
using System.Collections.Generic;
using System.IO;
using Ionic.Zip;
using Evolution.CommonService;
using Evolution.Models.Models;
using Evolution.Extensions;

namespace Evolution.FileCompressionService {
    public class Zip {

        public static Error ZipFolder(string sourcePath, string outputZipFilename) {
            var error = new Error();
            try {
                using (ZipFile zip = new ZipFile()) {
                    string[] fileNames = Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories);
                    foreach (string file in fileNames) {
                        string folderName = file.Replace(sourcePath, "");
                        folderName = folderName.Replace("\\" + folderName.FileName(), "");
                        zip.AddFile(file, folderName);
                    }
                    zip.Save(outputZipFilename);
                }
            } catch(Exception e1) {
                error.SetError(e1.Message, "");
            }

            return error;
        }

        public static Error UnzipFile(string sourceFile, string targetFolder) {
            var error = new Error();
            try {
                var zip = new ZipFile();
                zip = Ionic.Zip.ZipFile.Read(sourceFile);
                zip.ExtractAll(targetFolder, Ionic.Zip.ExtractExistingFileAction.OverwriteSilently);
           
            } catch (Exception e1) {
                error.SetError(e1.Message, "");
            }

            return error;
        }

        public static Error ZipFile(string fileName, string outputZipFilename) {
            List<string> fileNames = new List<string>();
            fileNames.Add(fileName);
            return ZipFiles(fileNames, outputZipFilename);
        }

        public static Error ZipFiles(List<string> fileNames, string outputZipFilename) {
            var error = new Error();
            try {
                using (ZipFile zip = new ZipFile()) {
                    foreach (string file in fileNames) {
                        zip.AddFile(file, "");
                    }
                    zip.Save(outputZipFilename);
                }
            } catch(Exception e1) {
                error.SetError(e1.Message, "");
            }

            return error;
        }
    }
}
