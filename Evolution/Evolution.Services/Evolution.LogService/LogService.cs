using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Evolution.CommonService;
using Evolution.DAL;
using Evolution.Enumerations;

namespace Evolution.LogService {
    public class LogService : CommonService.CommonService {

        #region Construction

        public LogService(EvolutionEntities dbEntities) : base(dbEntities) { }

        #endregion

        #region Public members

        public Log FindLog(int id) {
            return db.FindLog(id);
        }

        public void WriteLog(LogSection section, LogSeverity severity, string message) {
            WriteLog(section, severity, "", message, "");
        }

        public void WriteLog(LogSection section, LogSeverity severity, string url, Exception e) {
            WriteLog(section, severity, url, e.Message, e.StackTrace);
        }

        public void WriteLog(LogSection section, LogSeverity severity, string url = "", string message = "", string stackTrace = "") {
            db.WriteLog(section, severity, url, message, stackTrace);
        }

        public void WriteLog(Exception ex, string url = "") {
            db.WriteLog(ex, url);
        }

        #endregion
    }
}
