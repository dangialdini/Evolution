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
using Evolution.Models.Models;
using Evolution.Resources;

namespace Evolution.Areas.SystemAdmin.Controllers
{
    public class EMailQueueController : BaseController
    {
        // GET: SystemAdmin/EMailQueue
        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.SuperUser)]
        public ActionResult Index() {
            return EMailQueue();
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.SuperUser)]
        public ActionResult EMailQueue() {
            var model = createModel();
            return View("EMailQueue", model);
        }

        ViewModelBase createModel() {
            var model = new ViewModelBase();
            PrepareViewModel(model, EvolutionResources.bnrEMailQueue + " - " + EMailService.GetQueueCount().ToString() + " item(s)");
            return model;
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.SuperUser)]
        public ActionResult GetEMailQueue(int index, int pageNo, int pageSize, string search) {
            return Json(EMailService.FindEMailQueueListModel(index, pageNo, pageSize, search), JsonRequestBehavior.AllowGet);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.SuperUser)]
        public ActionResult Delete(int index, int id) {
            var model = new EMailQueueListModel();
            model.GridIndex = index;
            try {
                EMailService.DeleteEMailQueue(id);
            } catch (Exception e1) {
                model.Error.SetError(e1);
            }
            return Json(model, JsonRequestBehavior.AllowGet);
        }
    }
}
