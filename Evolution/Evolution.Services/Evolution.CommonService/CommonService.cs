using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using Evolution.DAL;
using Evolution.Models.Models;
using Evolution.Extensions;

namespace Evolution.CommonService {
    public partial class CommonService {

        #region Private Members

        private AuditService.AuditService _auditService = null;

        #endregion

        #region Protected members

        protected EvolutionEntities db;

        protected static EvolutionEntities dbStatic { get { return new EvolutionEntities(); } }

        #endregion

        #region Construction

        public CommonService(EvolutionEntities dbEntities) {
            db = dbEntities;
        }

        #endregion

        #region Configuration file settings

        protected string GetConfigurationSetting(string key, string defaultValue) {
            string result = defaultValue;
            try {
                result = ConfigurationManager.AppSettings[key];
            } catch {}
            return result;
        }

        protected int GetConfigurationSetting(string key, int defaultValue) {
            try {
                return Convert.ToInt32(ConfigurationManager.AppSettings[key]);
            } catch {
                return defaultValue;
            }
        }

        protected bool GetConfigurationSetting(string key, bool defaultValue) {
            string temp = GetConfigurationSetting(key, defaultValue.ToString()).ToLower();
            return (temp == "true" || temp == "yes" || temp == "1");
        }

        #endregion

        #region Field methods

        protected string getFieldValue(string value) {
            if (value == null) {
                return "";
            } else {
                return value.Trim();
            }
        }
        
        protected string getFieldValueISODate(DateTimeOffset? dt) {
            if (dt == null) {
                return "";
            } else {
                return dt.Value.ToString("o");
            }
        }
        
        protected string formatDate(DateTimeOffset? dt, string format) {
            string rc = "";
            if (dt != null) rc = dt.Value.ToString(format);
            return rc;
        }
        
        #endregion

        #region Validation

        protected Error isValidRequiredString(string str, int maxLength, string fieldName, string errorMsg) {
            var error = new Error();
            if (string.IsNullOrEmpty(str) || str.Length > maxLength) {
                error.SetError(errorMsg, fieldName, maxLength.ToString(), fieldName);
            }
            return error;
        }

        protected Error isValidRequiredEMail(string str, int maxLength, string fieldName, string errorMsg) {
            var error = isValidRequiredString(str, maxLength, fieldName, errorMsg);
            if(!error.IsError) {
                if(!str.IsValidEMail()) error.SetError(errorMsg, fieldName, maxLength.ToString(), fieldName);
            }
            return error;
        }

        protected Error isValidNonRequiredEMail(string str, int maxLength, string fieldName, string errorMsg) {
            var error = new Error();
            if (!string.IsNullOrEmpty(str) && (str.Length > maxLength || !str.IsValidEMail())) {
                error.SetError(errorMsg, fieldName, maxLength.ToString(), fieldName);
            }
            return error;
        }

        protected Error isValidNonRequiredString(string str, int maxLength, string fieldName, string errorMsg) {
            var error = new Error();
            if (!string.IsNullOrEmpty(str) && str.Length > maxLength) {
                error.SetError(errorMsg, fieldName, maxLength.ToString(), fieldName);
            }
            return error;
        }

        protected Error isValidRequiredInt(int? intValue, string fieldName, string errorMsg) {
            var error = new Error();
            if (intValue == null || intValue == 0) {
                error.SetError(errorMsg, fieldName, fieldName);
            }
            return error;
        }

        protected Error isValidRequiredDate(DateTimeOffset? dateValue, string fieldName, string errorMsg) {
            var error = new Error();
            if (dateValue == null) {
                error.SetError(errorMsg, fieldName, fieldName);
            }
            return error;
        }

        #endregion

        #region Auditing

        protected AuditService.AuditService AuditService {
            get {
                if (_auditService == null) _auditService = new AuditService.AuditService(db);
                return _auditService;
            }
        }

        #endregion
    }
}
