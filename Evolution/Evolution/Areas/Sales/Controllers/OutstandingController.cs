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

namespace Evolution.Areas.Sales.Controllers {
    public class OutstandingController : BaseController {
        // GET: Sales/Outstanding
        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.Sales)]
        public ActionResult Index(int id = 0) {
            return Outstanding(id);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.Sales)]
        public ActionResult Outstanding(int id) {     // The id is that of the Temp Table SOH
            var model = createModel(id);
            return View("Outstanding", model);
        }

        ViewModelBase createModel(int saleId) {     // Id of temp table POH
            var model = new NoteListViewModel();
            var soht = SalesService.FindSalesOrderHeaderTempModel(saleId, CurrentCompany);

            PrepareViewModel(model,
                             EvolutionResources.bnrOutstanding + (soht == null ? "" : " - Order Number: " + soht.OrderNumber),
                             saleId,
                             MakeMenuOptionFlags(0, 0, 0, soht.OriginalRowId));
            model.ParentId = saleId;

            return model;
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.Sales)]
        public ActionResult GetOutstanding(int index, int saleId, int pageNo, int pageSize, string search) {
            var soh = SalesService.FindSalesOrderHeaderModelFromTempId(saleId, CurrentCompany);
            return Json(SalesService.FindTransactionDrillDown(soh,
                                                              index,
                                                              pageNo,
                                                              pageSize),
                        JsonRequestBehavior.AllowGet);
        }
    }
}
