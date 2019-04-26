using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using Evolution.Models.ViewModels;
using Evolution.Enumerations;
using Evolution.Resources;

namespace Evolution.Controllers.Application {
    public class ErrorController : BaseController {
        // GET: Error
        public ActionResult Index(int id) {
            // Id parameter is the id of the error log record
            var model = new ErrorViewModel();
            PrepareViewModel(model, EvolutionResources.bnrError);

            var logRecord = LogService.FindLog(id);
            if (logRecord == null) {
                model.SetError(ErrorIcon.Error,
                               EvolutionResources.errLogRecordNotFound,
                               id.ToString());
            } else {
                string msg,
                       setting = ConfigurationManager.AppSettings["ShowFullErrors"].ToString().ToLower();

                if (setting == "true") {
                    // Show full error including stack trace
                    msg =  "<table style=\"width:100%\">";
                    msg += "  <tr><td style=\"width:120px; vertical-align:top\">URL:</td><td style=\"vertical-align:top\">" + logRecord.Url + "</td></tr>";
                    msg += "  <tr><td><br/></td><td></td></tr>";
                    msg += "  <tr><td style=\"vertical-align:top\">Error:</td><td style=\"vertical-align:top\">" + logRecord.Message.Replace("\r\n", "<br/>\r\n") + "</td></tr>";
                    msg += "  <tr><td><br/></td><td></td></tr>";
                    msg += "  <tr><td style=\"vertical-align:top\">Stack Trace:</td><td style=\"vertical-align:top\">" + logRecord.StackTrace.Replace("\r\n", "<br/>\r\n") + "</td></tr>";
                    msg += "</table>";

                } else {
                    // Only display the short message
                    msg = logRecord.Message;
                }
                model.Message = msg;
            }

            return View("Error", model);
        }

        public ActionResult Error() {
            return View();
        }
    }
}