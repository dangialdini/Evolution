using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Web;
using System.Web.Mvc;
using Evolution.Security;
using Evolution.Controllers.Application;
using Evolution.Models.ViewModels;
using Evolution.Enumerations;
using Evolution.Models.Models;
using Evolution.Resources;
using Evolution.Extensions;

namespace Evolution.Areas.Sales.Controllers
{
    public class PicksController : BaseController {
        // GET: Sales/Picks
        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.Sales)]
        public ActionResult Index() {
            return Picks();
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.Sales)]
        public ActionResult Picks() {
            PicksListViewModel model = createModel();
            return View("Picks", model);
        }

        PicksListViewModel createModel() {
            PicksListViewModel model = new PicksListViewModel();
            PrepareViewModel(model, EvolutionResources.bnrPicks, 0, MenuOptionFlag.RequiresNoSale);
            return model;
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.Sales)]
        public ActionResult GetPicks(int index, int pageNo, int pageSize, string search,
                                     string sortColumn, int sortOrder,
                                     string dateFrom, string dateTo, int tz) {
            var model = createModel();

            Response.Cookies.Remove("picks_txtDateFrom");
            Response.Cookies.Remove("picks_txtDateTo");

            if (dateFrom.Trim().Length <= 10) dateFrom = dateFrom.Trim() + " 00:00:00";
            if (dateTo.Trim().Length <= 10) dateTo = dateTo.Trim() + " 23:59:59";

            return Json(PickService.FindPicksListModel(model.CurrentCompany.Id,
                                                        dateFrom.ParseDateTime(CurrentUser.DateFormat + " HH:mm:ss", tz),
                                                        dateTo.ParseDateTime(CurrentUser.DateFormat + " HH:mm:ss", tz),
                                                        index,
                                                        pageNo,
                                                        pageSize,
                                                        search,
                                                        sortColumn,
                                                        (SortOrder)sortOrder),
                                                        JsonRequestBehavior.AllowGet);
        }
    }
}