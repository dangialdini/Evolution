using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Security.Permissions;
using Evolution.Security;
using Evolution.Controllers.Application;
using Evolution.Models.ViewModels;
using Evolution.Enumerations;
using Evolution.Models.Models;
using Evolution.Resources;

namespace Evolution.Areas.CompanyAdmin.Controllers
{
    public class CustomerDefaultsController : BaseController {
        // GET: CompanyAdmin/CustomerDefaults
        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.SuperUser)]
        public ActionResult Index() {
            return CustomerDefaults();
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.SuperUser)]
        public ActionResult CustomerDefaults() {
            var model = createModel();
            return View("CustomerDefaults", model);
        }

        ViewModelBase createModel() {
            var model = new ViewModelBase();
            PrepareViewModel(model, EvolutionResources.bnrCustomerDefaults);

            return model;
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.SuperUser)]
        public ActionResult GetCustomerDefaults(int index, int pageNo, int pageSize, string search) {
            return Json(CustomerService.FindCustomerDefaultsListModel(CurrentCompany.Id, index, pageNo, pageSize, search),
                        JsonRequestBehavior.AllowGet);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.SuperUser)]
        public ActionResult Edit(int id) {
            var model = new EditCustomerDefaultViewModel();
            model.CustomerDefault = CustomerService.FindCustomerDefaultModel(id, CurrentCompany, true);
            prepareEditModel(model);

            model.LGS = CustomerService.LockCustomerDefault(model.CustomerDefault);

            return View(model);
        }

        void prepareEditModel(EditCustomerDefaultViewModel model) {
            PrepareViewModel(model, EvolutionResources.bnrAddEditCustomerDefaults);

            model.CountryList = LookupService.FindCountriesListItemModel();
            model.CountryList.Insert(0, new ListItemModel(EvolutionResources.lblNone, "0"));

            model.CurrencyList = LookupService.FindCurrenciesListItemModel();

            model.CustomerTypeList = LookupService.FindLOVItemsListItemModel(CurrentCompany, LOVName.CustomerType);
            model.CustomerTypeList.Insert(0, new ListItemModel(EvolutionResources.lblNone, "0"));

            model.SalesPersonList = MembershipManagementService.FindUserListItemModel();
            model.SalesPersonList.Insert(0, new ListItemModel(EvolutionResources.lblNone, "0"));

            model.PaymentTermsList = LookupService.FindPaymentTermsListItemModel(CurrentCompany);

            model.TaxCodeList = LookupService.FindTaxCodesListItemModel(CurrentCompany);
            model.TaxCodeList.Insert(0, new ListItemModel(EvolutionResources.lblNone, "0"));

            model.FreightCarrierList = LookupService.FindFreightCarriersListItemModel(model.CurrentCompany);

            model.InvoiceTemplateList = new JavaScriptSerializer().Serialize(LookupService.FindDocumentTemplatesListItemModel(DocumentTemplateCategory.Invoice));
            model.PacklistTemplateList = new JavaScriptSerializer().Serialize(LookupService.FindDocumentTemplatesListItemModel(DocumentTemplateCategory.Pickslip));

            model.PriceLevelsList = LookupService.FindPriceLevelsListItemModel(CurrentCompany);
            model.PriceLevelsList.Insert(0, new ListItemModel(EvolutionResources.lblNone, "0"));
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.SuperUser)]
        public ActionResult GetShippingTemplates(int currencyId) {
            EditCustomerDefaultViewModel model = new EditCustomerDefaultViewModel();
            model.InvoiceTemplateList = new JavaScriptSerializer().Serialize(LookupService.FindDocumentTemplatesListItemModel(DocumentTemplateCategory.Invoice));
            model.PacklistTemplateList = new JavaScriptSerializer().Serialize(LookupService.FindDocumentTemplatesListItemModel(DocumentTemplateCategory.Pickslip));

            return Json(model, JsonRequestBehavior.AllowGet);
        }

        [ValidateAntiForgeryToken]
        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.SuperUser)]
        public ActionResult Save(EditCustomerDefaultViewModel model, string command) {
            if (command.ToLower() == "save") {
                if(ModelState.IsValid) {
                    var modelError = CustomerService.InsertOrUpdateCustomerDefault(model.CustomerDefault, model.LGS);
                    if (modelError.IsError) {
                        model.SetErrorOnField(ErrorIcon.Error,
                                              modelError.Message,
                                              "Customerdefault_" + modelError.FieldName);
                    } else {
                        return RedirectToAction("CustomerDefaults");
                    }
                }
                prepareEditModel(model);
                return View("Edit", model);

            } else {
                return RedirectToAction("CustomerDefaults");
            }
        }
    }
}
