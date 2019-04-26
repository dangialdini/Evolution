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

        #region Media type methods

        public bool IsValidMediaType(string fileName) {
            return GetMediaType(fileName) != null;
        }

        public DAL.MediaType GetMediaType(string fileName) {
            return db.FindMediaType(fileName);
        }

        public string GetValidMediaTypes() {
            string rc = "";
            foreach (var mediaType in db.FindMediaTypes()) {
                if (!string.IsNullOrEmpty(rc)) rc += ", ";
                rc += mediaType.Extension;
            }
            return rc;
        }

        public string GetValidImageTypes() {
            string rc = "";
            foreach (var mediaType in db.FindMediaTypes()
                                        .Where(mt => mt.CreateThumb == true)) {
                if (!string.IsNullOrEmpty(rc)) rc += ", ";
                rc += mediaType.Extension;
            }
            return rc;
        }

        public bool IsValidMediaUploadType(string fileName) {
            return isValidFileType(fileName, GetValidMediaTypes());
        }

        public string GetValidOrderImportTypes() {
            return "csv, txt";
        }

        public bool IsValidOrderImportType(string fileName) {
            return isValidFileType(fileName, GetValidOrderImportTypes());
        }

        private bool isValidFileType(string fileName, string typeList) {
            string validTypes = ("," + typeList + ",").Replace(" ", ",").ToLower();
            if (validTypes.IndexOf(fileName.ToLower().FileExtension()) != -1) {
                return true;
            } else {
                return false;
            }
        }

        #endregion
    }
}
