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

        #region File sizes

        public static int GetMaxUploadFileSize() {
            HttpRuntimeSection section = ConfigurationManager.GetSection("system.web/httpRuntime") as HttpRuntimeSection;
            return section.MaxRequestLength * 1024;     // Convert to bytes
        }

        #endregion

        #region Templates and temporary files

        public static string GetTempFile(string fileExtn = ".tmp") {
            return Path.GetTempPath() + Guid.NewGuid().ToString() + fileExtn;
        }

        public static string SepChar(bool bHttp) {
            return (bHttp ? "/" : "\\");
        }

        #endregion

        #region Company logos

        public List<ListItemModel> FindCompanyLogos() {
            List<ListItemModel> logoList = new List<ListItemModel>();

            try {
                foreach (var qualName in Directory.GetFiles(GetConfigurationSetting("SiteFolder", "") + "\\Content\\Logos")) {
                    string fileName = qualName.FileName();

                    ListItemModel item = new ListItemModel {
                        Id = fileName,
                        Text = fileName,
                        ImageURL = "/Content/Logos/" + fileName
                    };
                    logoList.Add(item);
                }

            } catch { }

            return logoList;
        }

        #endregion

        #region Disk methods

        public static Error CopyOrMoveFile(string sourceFile, string targetFile, FileCopyType copyType) {
            var error = new Error();

            switch (copyType) {
            case FileCopyType.Move:
                DeleteFile(targetFile);
                CreateDirectory(targetFile.FolderName());

                try {
                    File.Move(sourceFile, targetFile);
                    dbStatic.LogTestFile(targetFile);
                } catch (Exception ex) {
                    error.SetError(ex);
                }
                break;

            case FileCopyType.Copy:
                CreateDirectory(targetFile.FolderName());

                try {
                    File.Copy(sourceFile, targetFile, true);
                    dbStatic.LogTestFile(targetFile);
                } catch (Exception ex) {
                    error.SetError(ex);
                }
                break;
            }
            return error;
        }

        public static Error DeleteFile(string fileName) {
            var error = new Error();
            try {
                File.Delete(fileName);
            } catch (Exception ex) {
                error.SetError(ex);
            }
            return error;
        }

        public static Error CreateDirectory(string folderName) {
            var error = new Error();
            try {
                Directory.CreateDirectory(folderName);
                dbStatic.LogTestFolder(folderName);
            } catch (Exception ex) {
                error.SetError(ex);
            }
            return error;
        }

        public static Error DeleteDirectory(string folderName, bool bRecursive = false) {
            var error = new Error();
            try {
                Directory.Delete(folderName, bRecursive);
            } catch (Exception ex) {
                error.SetError(ex);
            }
            return error;
        }

        #endregion
    }
}
