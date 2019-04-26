using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Security.Permissions;
using Evolution.Security;
using Evolution.Controllers.Application;
using Evolution.Models.ViewModels;
using Evolution.Enumerations;
using Evolution.Extensions;
using Evolution.Models.Models;
using Evolution.Resources;

namespace Evolution.Areas.SystemAdmin.Controllers {
    public class SystemLogController : BaseController {
        // GET: SystemAdmin/SystemLog
        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.SuperUser)]
        public ActionResult Index() {
            return SystemLog();
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.SuperUser)]
        public ActionResult SystemLog() {
            var model = createModel();
            return View("SystemLog", model);
        }

        ViewModelBase createModel() {
            var model = new ViewModelBase();
            PrepareViewModel(model, EvolutionResources.bnrSystemLogs);

            return model;
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.SuperUser)]
        public ActionResult GetSystemLog(int index, int pageNo, int pageSize, string search, int section, int severity,
                                         string dateFrom, string dateTo, int tz) {

            if (dateFrom.Trim().Length <= 10) dateFrom = dateFrom.Trim() + " 00:00:00";
            if (dateTo.Trim().Length <= 10) dateTo = dateTo.Trim() + " 23:59:59";

            return Json(SystemService.FindLogListModel(index,
                                                       pageNo,
                                                       pageSize,
                                                       search,
                                                       section,
                                                       severity,
                                                       dateFrom.ParseDateTime(CurrentUser.DateFormat + " HH:mm:ss", tz),   //dtFromO, 
                                                       dateTo.ParseDateTime(CurrentUser.DateFormat + " HH:mm:ss", tz)),    //dtToO), 
                        JsonRequestBehavior.AllowGet);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.SuperUser)]
        public ActionResult GetSectionList() {
            return Json(LookupService.FindLogSectionListItemModel(), JsonRequestBehavior.AllowGet);
        }
    }
}
