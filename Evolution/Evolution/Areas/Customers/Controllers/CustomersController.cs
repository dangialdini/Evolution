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
using Evolution.Extensions;
using Evolution.Enumerations;
using Evolution.Models.Models;
using Evolution.Resources;

namespace Evolution.Areas.Customers.Controllers {
    public class CustomersController : BaseController {
        // GET: Customers/Customers
        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.Sales)]
        public ActionResult Index() {
            var model = createModel();
            return View("Customers", model);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.Sales)]
        public ActionResult Customers() {
            var model = createModel();
            return View("Customers", model);
        }

        ViewModelBase createModel() {
            var model = new ViewModelBase();
            PrepareViewModel(model, EvolutionResources.bnrCustomers, 0, MenuOptionFlag.RequiresNoCustomer);
            return model;
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.Sales)]
        public ActionResult GetCustomers(int index, int pageNo, int pageSize, string search,
                                         string cardRecordId,
                                         int acctmgr,
                                         int country,
                                         int region,
                                         string sortColumn, int sortOrder) {
            var model = createModel();
            return Json(CustomerService.FindCustomersListModel(model.CurrentCompany.Id, 
                                                               index, 
                                                               pageNo, 
                                                               pageSize, 
                                                               search,
                                                               cardRecordId.ParseInt(),
                                                               acctmgr,
                                                               country,
                                                               region,
                                                               sortColumn,
                                                               (SortOrder)sortOrder), 
                        JsonRequestBehavior.AllowGet);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.Sales)]
        public ActionResult GetAccountMgrList() {
            var model = createModel();
            var list = SalesService.FindSalesPersonListItemModel(CurrentCompany);
            list.Insert(0, new ListItemModel { Id = "0", Text = EvolutionResources.lblNone });
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.Sales)]
        public ActionResult GetCountryList() {
            var model = createModel();
            var list = LookupService.FindCountriesListItemModel();
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.Sales)]
        public ActionResult GetRegionList() {
            var model = createModel();
            var list = LookupService.FindRegionsListItemModel(CurrentCompany);
            list.Insert(0, new ListItemModel { Id = "0", Text = EvolutionResources.lblNone });
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.Sales)]
        public ActionResult NewCustomer() {
            var model = new NewCustomerViewModel();
            prepareEditModel(model);
            return View("NewCustomer", model);
        }

        void prepareEditModel(NewCustomerViewModel model) {
            PrepareViewModel(model, EvolutionResources.bnrAddNewCustomer);

            model.CountryList = LookupService.FindCountriesListItemModel();
            model.CustomerTypeList = LookupService.FindLOVItemsListItemModel(model.CurrentCompany, LOVName.CustomerType);
            model.RegionList = LookupService.FindRegionsListItemModel(model.CurrentCompany);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.Sales)]
        public ActionResult Edit(int id) {
            var model = new EditCustomerViewModel();
            prepareEditModel(model, id);

            model.Customer = CustomerService.FindCustomerModel(id, CurrentCompany, false);

            // TBD: Need to get country and postcode of customer to be able to create them.
            if (model.Customer == null) model.Customer = CustomerService.CreateCustomer(CurrentCompany, 0, "");

            model.LGS = CustomerService.LockCustomer(model.Customer);

            return View(model);
        }

        void prepareEditModel(EditCustomerViewModel model, int id) {
            PrepareViewModel(model, EvolutionResources.bnrAddEditCustomer + (id > 0 ? " - CustId:" + id.ToString() : ""), id, MakeMenuOptionFlags(id, 0));

            model.CustomerTypeList = LookupService.FindLOVItemsListItemModel(model.CurrentCompany, LOVName.CustomerType);
            model.CurrencyList = LookupService.FindCurrenciesListItemModel();
            model.TaxCodeList = LookupService.FindTaxCodesListItemModel(model.CurrentCompany);
            model.PriceLevelList = LookupService.FindPriceLevelsListItemModel(model.CurrentCompany);
            model.PaymentTermList = LookupService.FindPaymentTermsListItemModel(model.CurrentCompany);
            model.OrderTypeList = LookupService.FindLOVItemsListItemModel(model.CurrentCompany, LOVName.OrderType);
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.Sales)]
        public ActionResult Delete(int index, int id) {
            var model = new CustomerListModel();
            model.GridIndex = index;
            try {
                CustomerService.DeleteCustomer(CurrentCompany.Id, id);
            } catch (Exception e1) {
                model.Error.SetError(e1);
            }
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        [ValidateAntiForgeryToken]
        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.Sales)]
        public ActionResult Save(EditCustomerViewModel model, string command) {
            if (command.ToLower() == "save") {
                var modelError = CustomerService.InsertOrUpdateCustomer(model.Customer, CurrentUser, model.LGS);
                if (modelError.IsError) {
                    prepareEditModel(model, model.Customer.Id);
                    model.SetErrorOnField(ErrorIcon.Error,
                                          modelError.Message,
                                          "Customer_" + modelError.FieldName);
                    return View("Edit", model);

                } else {
                    return RedirectToAction("Customers");
                }

            } else {
                return RedirectToAction("Customers");
            }
        }

        [CustomPrincipalPermission(SecurityAction.Demand, Role = UserRole.Sales)]
        public ActionResult CreateCustomer(NewCustomerViewModel model, string command) {
            if (ModelState.IsValid) {
                var customer = CustomerService.CreateCustomer(CurrentCompany, model.Customer.CountryId, model.Customer.Postcode);
                customer.Name = model.Customer.Name;
                customer.TaxId = model.Customer.TaxId;
                customer.CustomerTypeId = model.Customer.CustomerTypeId;

                // Valid date the customer
                model.Error = CustomerService.ValidateCustomerModel(customer);
                if (!model.Error.IsError) {
                    // Now validate the address
                    var customerAddrs = new CustomerAddressModel {
                        CompanyId = CurrentCompany.Id,
                        AddressTypeId = LookupService.FindLOVItemByValueModel(LOVName.AddressType, ((int)AddressType.Billing).ToString()).Id,
                        Street = model.Customer.Street,
                        City = model.Customer.City,
                        State = model.Customer.Postcode,
                        Postcode = model.Customer.Postcode,
                        CountryId = model.Customer.CountryId
                    };
                    model.Error = CustomerService.ValidateAddressModel(customerAddrs);
                    if (!model.Error.IsError) {
                        model.Error = CustomerService.InsertOrUpdateCustomer(customer, CurrentUser);
                        if(!model.Error.IsError) {
                            customerAddrs.CustomerId = customer.Id;
                            model.Error = CustomerService.InsertOrUpdateCustomerAddress(customerAddrs);

                            if(!model.Error.IsError) {
                                var additionalInfo = new CustomerAdditionalInfoModel {
                                    Id = customer.Id,
                                    CompanyId = CurrentCompany.Id,
                                    RegionId = model.AdditionalInfo.RegionId
                                };
                                model.Error = CustomerService.InsertOrUpdateCustomerAdditionalInfo(additionalInfo, CurrentUser, CustomerService.LockCustomer(customer));
                            }
                        }
                    }
                    if (!model.Error.IsError) model.Error.URL = "/Customers/Customers/Edit?Id=" + customer.Id.ToString();
                }

                if (model.Error.IsError) ModelState.AddModelError("Customer_" + model.Error.FieldName, model.Error.Message);

                if (!model.Error.IsError) {
                    return Json(model, JsonRequestBehavior.AllowGet);
                } else {
                    prepareEditModel(model);
                    return View("NewCustomer", model);
                }
            } else {
                prepareEditModel(model);
                return View("NewCustomer", model);
            }
        }
    }
}
