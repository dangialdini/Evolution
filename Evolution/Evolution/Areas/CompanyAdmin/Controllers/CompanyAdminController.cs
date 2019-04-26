using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Security.Permissions;
using System.IO;
using Evolution.Security;
using Evolution.Controllers.Application;
using Evolution.Models.ViewModels;
using Evolution.Enumerations;
using Evolution.Models.Models;
using Evolution.Resources;

namespace Evolution.Areas.CompanyAdmin.Controllers 
{
    public class CompanyAdminController : BaseController {
        // GET: CompanyAdmin/CompanyAdmin
        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.SuperUser)]
        public ActionResult Index() {
            var model = createModel();
            return View("Companies", model);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.SuperUser)]
        public ActionResult Companies() {
            var model = createModel();
            return View("Companies", model);
        }

        ViewModelBase createModel() {
            var model = new ViewModelBase();
            PrepareViewModel(model, EvolutionResources.bnrCompanies);

            return model;
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.SuperUser)]
        public ActionResult GetCompanies(int index, int pageNo, int pageSize, string search) {
            return Json(CompanyService.FindCompaniesListModel(index, pageNo, pageSize, search, true), JsonRequestBehavior.AllowGet);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.SuperUser)]
        public ActionResult Edit(int id) {
            var model = new EditCompanyViewModel();
            model.Company = CompanyService.FindCompanyModel(id);
            prepareEditModel(model);

            model.Company = CompanyService.FindCompanyModel(id);
            model.LGS = CompanyService.LockCompany(model.Company);

            return View(model);
        }

        void prepareEditModel(EditCompanyViewModel model) {
            PrepareViewModel(model, EvolutionResources.bnrAddEditCompany);
            model.LogoList = MediaServices.FindCompanyLogos();

            model.WarehouseTemplateList = LookupService.FindDocumentTemplatesListItemModel(DocumentTemplateCategory.PurchaseOrder, DocumentTemplateType.SendPOtoWarehouse);
            model.SupplierTemplateList = LookupService.FindDocumentTemplatesListItemModel(DocumentTemplateCategory.PurchaseOrder, DocumentTemplateType.SendPOtoSupplier);
            model.FreightForwarderTemplateList = LookupService.FindDocumentTemplatesListItemModel(DocumentTemplateCategory.PurchaseOrder, DocumentTemplateType.SendPOtoFreightForwarder);

            model.LocationList = LookupService.FindLocationListItemModel(model.Company);
            model.CountryList = LookupService.FindCountriesListItemModel();
            model.CurrencyList = LookupService.FindCurrenciesListItemModel();
            model.DateFormatList = LookupService.FindDateFormatListItemModel();

            model.UnitOfMeasureList = new List<ListItemModel>();
            model.UnitOfMeasureList.Add(new ListItemModel(EvolutionResources.lblMetric, ((int)UnitOfMeasure.Metric).ToString()));
            model.UnitOfMeasureList.Add(new ListItemModel(EvolutionResources.lblImperial, ((int)UnitOfMeasure.Imperial).ToString()));

            model.UserList = MembershipManagementService.FindUserListItemModel();
        }

        [ValidateAntiForgeryToken]
        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.SuperUser)]
        public ActionResult Save(EditCompanyViewModel model, string command) {
            if (command.ToLower() == "save") {
                var modelError = CompanyService.InsertOrUpdateCompany(model.Company, CurrentUser, model.LGS);
                if (modelError.IsError) {
                    prepareEditModel(model);
                    model.SetErrorOnField(ErrorIcon.Error,
                                          modelError.Message,
                                          "Company_" + modelError.FieldName);
                    return View("Edit", model);

                } else {
                    return RedirectToAction("Companies");
                }

            } else {
                return RedirectToAction("Companies");
            }
        }
    }
}
