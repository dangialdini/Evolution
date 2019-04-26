using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Security.Permissions;
using Evolution.Security;
using Evolution.Controllers.Application;
using Evolution.Models.ViewModels;
using Evolution.CustomerService;
using Evolution.Enumerations;
using Evolution.Models.Models;
using Evolution.Resources;

namespace Evolution.Areas.Customers.Controllers {
    public class CreditCardsController : BaseController {
        // GET: Customers/CreditCards
        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.Sales)]
        public ActionResult Index() {
            return CreditCards(0);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.Sales)]
        public ActionResult CreditCards(int id) {
            var model = createModel(id);
            return View("CreditCards", model);
        }

        ViewModelBase createModel(int customerId) {
            var model = new CreditCardListViewModel();
            var customer = CustomerService.FindCustomerModel(customerId, CurrentCompany);

            PrepareViewModel(model,
                             EvolutionResources.bnrCreditCards + (customer == null ? "" : " - " + customer.Name),
                             customerId,
                             MakeMenuOptionFlags(customerId, 0));
            model.ParentId = customerId;

            return model;
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.Sales)]
        public ActionResult GetCustomerCreditCards(int index, int customerId, int pageNo, int pageSize) {
            return Json(CustomerService.FindCreditCardsListModel(CurrentCompany.Id, customerId, index, pageNo, pageSize), JsonRequestBehavior.AllowGet);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.Sales)]
        public ActionResult Edit(int id, int customerId) {
            var model = new EditCreditCardViewModel();
            prepareEditModel(model, id, customerId);

            model.CreditCard = CustomerService.FindCreditCardModel(id, model.CurrentCompany, customerId);

            model.ParentId = customerId;
            model.LGS = CustomerService.LockCreditCard(model.CreditCard);

            return View(model);
        }

        void prepareEditModel(EditCreditCardViewModel model, int id, int customerId) {
            var customer = CustomerService.FindCustomerModel(customerId, CurrentCompany);

            string title = EvolutionResources.bnrAddEditCreditCard + (customer == null ? "" : " - " + customer.Name);
            if (id <= 0) title += " - " + EvolutionResources.lblNewContact;

            PrepareViewModel(model, title, customerId, MakeMenuOptionFlags(customerId, 0));

            model.CardProviderList = LookupService.FindCreditCardProviders();
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.Sales)]
        public ActionResult Delete(int index, int id) {
            var model = new CreditCardListModel();
            model.GridIndex = index;

            // Check if the card is attached to any sales
            int saleCount = SalesService.FindCreditCardSales(CurrentCompany, id).Items.Count();
            if (saleCount > 0) {
                model.Error.SetError(EvolutionResources.errCantDeleteCreditCardAttachedToSales, "", saleCount.ToString());

            } else {
                try {
                    CustomerService.DeleteCreditCard(id);
                } catch (Exception e1) {
                    model.Error.SetError(e1);
                }
            }
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        [ValidateAntiForgeryToken]
        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.Sales)]
        public ActionResult Save(EditCreditCardViewModel model, string command) {
            if (command.ToLower() == "save") {
                var modelError = CustomerService.InsertOrUpdateCreditCard(model.CreditCard, model.LGS);
                if (modelError.IsError) {
                    prepareEditModel(model, model.CurrentCompany.Id, model.CreditCard.CustomerId);
                    model.SetErrorOnField(ErrorIcon.Error,
                                          modelError.Message,
                                          "CreditCard_" + modelError.FieldName);
                    return View("Edit", model);

                } else {
                    return RedirectToAction("CreditCards", new { id = model.ParentId });
                }

            } else {
                return RedirectToAction("CreditCards", new { id = model.ParentId });
            }
        }
    }
}
