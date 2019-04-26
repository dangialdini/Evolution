using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Security.Permissions;
using Evolution.Security;
using Evolution.Controllers.Application;
using Evolution.Models.ViewModels;
using Evolution.CustomerService;
using Evolution.Enumerations;
using Evolution.Models.Models;
using Evolution.Resources;

namespace Evolution.Areas.Customers.Controllers
{
    public class CustomerAdditionalInfoController : BaseController {
        // GET: Customers/CustomerAdditionalInfo
        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.Sales)]
        public ActionResult Index() {
            return CustomerAdditionalInfo(0);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.Sales)]
        public ActionResult CustomerAdditionalInfo(int id) {
            var model = new EditCustomerAdditionalInfoViewModel();

            model.CustomerAdditionalInfo = CustomerService.FindCustomerAdditionalInfoModel(id, CurrentCompany);
            prepareEditModel(model, id);
            model.LGS = CustomerService.LockCustomerAdditionalInfo(model.CustomerAdditionalInfo);

            return View(model);
        }

        void prepareEditModel(EditCustomerAdditionalInfoViewModel model, int id) {
            PrepareViewModel(model, EvolutionResources.bnrCustomerAdditionalInfo + (id > 0 ? " - CustId:" + id.ToString() : ""), id, MakeMenuOptionFlags(id, 0));

            model.RegionList = LookupService.FindRegionsListItemModel(CurrentCompany.Id);

            // Default templates to list
            model.CustomerAdditionalInfo.ShippingTemplateType = (int)DocumentTemplateCategory.Invoice;

            if (model.CustomerAdditionalInfo.ShippingTemplateId != null) {
                // Customer has a shipping template, so set the default template type to the type of the template
                var shippingTemplate = db.FindDocumentTemplate(model.CustomerAdditionalInfo.ShippingTemplateId.Value);
                if(shippingTemplate != null) {
                    model.CustomerAdditionalInfo.ShippingTemplateType = shippingTemplate.TemplateCategory ?? (int)DocumentTemplateCategory.Invoice;
                }
            }

            // Get the customer's billing currency so that we can build a list of currency-relevant templates
            var customer = CustomerService.FindCustomerModel(id, null, false);
            int currencyId = (customer != null ? customer.CurrencyId : 0);

            model.InvoiceTemplateList = new JavaScriptSerializer().Serialize(LookupService.FindDocumentTemplatesListItemModel(DocumentTemplateCategory.Invoice));
            model.PacklistTemplateList = new JavaScriptSerializer().Serialize(LookupService.FindDocumentTemplatesListItemModel(DocumentTemplateCategory.Pickslip));

            model.SourceList = LookupService.FindLOVItemsListItemModel(CurrentCompany, LOVName.Source, true);
            model.OrderTypeList = LookupService.FindLOVItemsListItemModel(CurrentCompany, LOVName.OrderType);
        }

        [ValidateAntiForgeryToken]
        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.Sales)]
        public ActionResult Save(EditCustomerAdditionalInfoViewModel model, string command) {
            var modelError = CustomerService.InsertOrUpdateCustomerAdditionalInfo(model.CustomerAdditionalInfo, CurrentUser, model.LGS);
            if (modelError.IsError) {
                prepareEditModel(model, model.CustomerAdditionalInfo.Id);
                model.SetErrorOnField(ErrorIcon.Error,
                                        modelError.Message,
                                        "CustomerAdditionalInfo_" + modelError.FieldName);
                return View("CustomerAdditionalInfo", model);

            } else {
                prepareEditModel(model, model.CustomerAdditionalInfo.Id);
                model.SetErrorOnField(ErrorIcon.Information,
                                        EvolutionResources.infChangesSuccessfullySaved,
                                        "CustomerAdditionalInfo_DeliveryInstructions" + modelError.FieldName);
                return View("CustomerAdditionalInfo", model);
            }
        }
    }
}
