using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Evolution.Models.Models;
using Evolution.Enumerations;
using System.Web;

namespace Evolution.Models.ViewModels {
    public class ViewModelBase {

        #region Public Members
        public string PageTitle { set; get; } = "Default Page Title";
        public string UserName {
            get {
                string rc = Thread.CurrentPrincipal.Identity.Name;
                int pos = rc.IndexOf('\\');
                if (pos != -1) rc = rc.Substring(pos + 1);
                return rc;
            }
        }
        public string TZ { set; get; }
        public string LGS { set; get; }
        public MenuDataModel Menu { set; get; } = new MenuDataModel();
        public string MarginLogo { set; get; }
        public Error Error { set; get; } = new Error();
        public string FocusField { set; get; }

        public CompanyModel CurrentCompany { set; get; } = new CompanyModel();
        public List<ListItemModel> AvailableCompanies { set; get; }
        public int ParentId { set; get; }       // Customer id, Lov id etc

        public string JQDateFormat = "dd/mm/yy";
        public string DisplayDateFormat = "dd/MM/yyyy";

        #endregion

        #region Construction

        public ViewModelBase() {
        }

        public ViewModelBase(string pageTitle) {
            PageTitle = pageTitle;
        }

        #endregion

        #region Public Accessors

        public void SetError(ErrorIcon icon, string message,
                             string p1 = null, string p2 = null, string p3 = null, string p4 = null,
                             bool bCookie = false) {
            string msg = message;
            if (p1 != null) msg = msg.Replace("%1", p1);
            if (p2 != null) msg = msg.Replace("%2", p2);
            if (p3 != null) msg = msg.Replace("%3", p3);
            if (p4 != null) msg = msg.Replace("%4", p4);
            msg = msg.Replace("\\", "&#92;");

            if (bCookie) {
                setCookie("ErrorIcon", ((int)icon).ToString(), 100);
                setCookie("ErrorText", msg, 100);
            } else {
                Error = new Error();
                Error.SetError(msg);
                Error.Icon = icon;
            }
        }

        public void SetError(Exception ex, string fieldName = "", bool bCookie = false) {
            string msg = "Error: " + ex.Message;
            if (ex.InnerException != null) {
                if (ex.InnerException.InnerException != null && !string.IsNullOrEmpty(ex.InnerException.InnerException.Message)) msg += "||" + ex.InnerException.InnerException.Message;
                if (!string.IsNullOrEmpty(ex.InnerException.Message)) msg += "||" + ex.InnerException.Message;
            }
            SetError(ErrorIcon.Error, msg, fieldName, null, null, null, bCookie);
        }

        public void SetErrorOnField(ErrorIcon icon, string message, string focusField, string p1 = null, string p2 = null, string p3 = null, string p4 = null, bool bCookie = false) {
            SetError(icon, message, p1, p2, p3, p4, bCookie);
            if(bCookie) setCookie("ErrorField", focusField, 100);
            FocusField = focusField;
        }

        public void SetRecordError(string tableName, int rowId, bool bCookie = false) {
            SetError(ErrorIcon.Error,
                     "Error: Record #%1 could not be found in table '%2' !",
                     rowId.ToString(), tableName, null, null, bCookie);
        }

        private void setCookie(string cookieName, string value, int secs) {
            var cookie = new HttpCookie(cookieName, value);
            cookie.Expires = DateTime.Now.AddSeconds(secs);

            // When running tests, the follow fails, so swallow any exception
            try {
                HttpContext.Current.Response.SetCookie(cookie);
            } catch { }
        }

        #endregion
    }
}
