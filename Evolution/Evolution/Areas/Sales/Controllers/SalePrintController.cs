using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Security.Permissions;
using Evolution.Security;
using Evolution.Controllers.Application;
using Evolution.Models.Models;
using Evolution.Models.ViewModels;
using Evolution.Enumerations;
using Evolution.Extensions;
using Evolution.Resources;
using System.Globalization;

namespace Evolution.Areas.Sales.Controllers {
    public class SalePrintController : BaseController {

        // GET: Sales/SalePrint
        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.Sales)]
        public ActionResult Index() {
            return SalePrint(0);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.Sales)]
        public ActionResult SalePrint(int id) {
            var model = createModel(id);
            return View("SalePrint", model);
        }

        ViewModelBase createModel(int id) {
            var model = new SalePrintOptionsViewModel();
            model.SalesOrderHeaderTempId = id;

            var soht = SalesService.FindSalesOrderHeaderTempModel(id, CurrentCompany, true);
            PrepareViewModel(model, EvolutionResources.bnrPrintSale, id, MakeMenuOptionFlags(0, 0, 0, soht.OriginalRowId));

            var customer = CustomerService.FindCustomerModel(soht.CustomerId == null ? 0 : soht.CustomerId.Value, CurrentCompany);
            model.CustomerContact.CustomerId = customer.Id;

            prepareViewModel(model);

            return model;
        }

        void prepareViewModel(SalePrintOptionsViewModel model) {
            var soht = SalesService.FindSalesOrderHeaderTempModel(model.SalesOrderHeaderTempId, CurrentCompany, true);
            PrepareViewModel(model, EvolutionResources.bnrPrintSale, model.SalesOrderHeaderTempId, MakeMenuOptionFlags(0, 0, 0, soht.OriginalRowId));

            model.TemplateList = LookupService.FindDocumentTemplatesListItemModel(DocumentTemplateCategory.SalesOrders);

            model.AvailableRecipientsList = CustomerService.FindCustomerRecipients(soht, CurrentCompany, CurrentUser);
            model.AvailableRecipientsList.Add(new ListItemModel("Other Recipient", "OTH"));

            model.SalutationList = LookupService.FindLOVItemsListItemModel(CurrentCompany, LOVName.Salutation);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.Sales)]
        public ActionResult DoPrintSendSale(SalePrintOptionsViewModel model) {
            if (ModelState.IsValid) {
                // Prints the sale according to the user selections
                string selectedIds;
                try {
                    selectedIds = Request.Form["SelectedIds"].ToLower();
                } catch {
                    selectedIds = "";
                }
                model.Error = SalesService.PrintSale(model, CurrentCompany, CurrentUser, selectedIds);
                if (!model.Error.IsError && !string.IsNullOrEmpty(model.Error.URL))
                    return Redirect(model.Error.URL);
            }

            prepareViewModel(model);

            return View("SalePrint", model);
        }
    }
}
