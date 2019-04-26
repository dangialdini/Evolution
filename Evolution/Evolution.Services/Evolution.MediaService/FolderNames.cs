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

        #region Folder methods

        public string GetSiteFolder(bool bHttp = false) {
            if(bHttp) {
                return GetConfigurationSetting("SiteHttp", "");
            } else {
                return GetConfigurationSetting("SiteFolder", "");
            }
        }

        public string GetTempFolder() {
            return Path.GetTempPath();
        }

        public string GetMediaFolder(int companyId = -1, bool bHttp = false, bool bRelative = false) {
            string rc = "";
            if (bHttp) {
                if (bRelative) {
                    rc = "";
                } else {
                    rc = GetConfigurationSetting("MediaHttp", "");
                }
                if (companyId != -1) rc += "/" + companyId.ToString();
            } else {
                if (bRelative) {
                    rc = "";
                } else {
                    rc = GetConfigurationSetting("MediaFolder", "");
                }
                if (companyId != -1) rc += "\\" + companyId.ToString();
            }
            return rc;
        }

        public string GetMediaFolder(Enumerations.MediaFolder mediaFolder, int companyId = -1,
                                     int parentId = -1, int noteId = -1, bool bHttp = false, bool bRelative = false) {
            string rc = "",
                   sepChar = SepChar(bHttp);

            switch (mediaFolder) {
            case MediaFolder.Customer:
            case MediaFolder.Purchase:
            case MediaFolder.Sale:
            case MediaFolder.Supplier:
                rc = GetMediaFolder(companyId, bHttp, bRelative);
                rc += sepChar + mediaFolder.ToString();
                if (parentId != -1) {
                    rc += sepChar + parentId.ToString();
                    if (noteId != -1) {
                        rc += sepChar + noteId.ToString();
                    }
                }
                break;

            case MediaFolder.Product:
                rc = GetMediaFolder(-1, bHttp, bRelative);
                rc += sepChar + mediaFolder.ToString();
                break;

            case MediaFolder.ProductCompliance:
                rc = GetMediaFolder(-1, bHttp, bRelative);
                rc += sepChar + mediaFolder.ToString();
                rc += sepChar + parentId.ToString();
                rc += sepChar + noteId.ToString();
                break;

            case MediaFolder.User:
                rc = GetMediaFolder(-1, bHttp, bRelative);
                rc += sepChar + mediaFolder.ToString();
                rc += sepChar + parentId.ToString();
                break;

            case MediaFolder.Temp:
                rc = GetMediaFolder(companyId, bHttp, bRelative);
                rc += sepChar + mediaFolder.ToString();
                break;
            }
            return rc;
        }

        public string GetDataTransferFolder(int companyId) {
            return GetConfigurationSetting("DataTransferFolder", "") + "\\" + companyId.ToString();
        }

        public string CreateCompanyFolders(int companyId) {
            // Create the folders for the company media
            string companyfolder = GetMediaFolder(companyId, false);
            db.LogTestFolder(companyfolder);

            CreateDirectory(companyfolder);
            CreateDirectory(GetMediaFolder(MediaFolder.Customer, companyId));
            CreateDirectory(GetMediaFolder(MediaFolder.Purchase, companyId));
            CreateDirectory(GetMediaFolder(MediaFolder.Sale, companyId));
            CreateDirectory(GetMediaFolder(MediaFolder.Supplier, companyId));
            

            // Create the folders for the company data transfers
            string dataTransferFolder = GetDataTransferFolder(companyId);
            db.LogTestFolder(dataTransferFolder);

            CreateDirectory(dataTransferFolder);
            CreateDirectory(dataTransferFolder + "\\UnpackSlips");
            CreateDirectory(dataTransferFolder + "\\UnpackSlips\\Errors");

            return companyfolder;
        }

        public string GetProductImageFolder(bool bHttp) {
            return GetMediaFolder(MediaFolder.Product, -1, -1, -1, bHttp) + SepChar(bHttp);
        }

        #endregion
    }
}
