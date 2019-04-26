using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Evolution.DAL;
using Evolution.Models.Models;
using Evolution.CommonService;
using Evolution.Enumerations;
using Evolution.Extensions;
using Evolution.Resources;
using AutoMapper;

namespace Evolution.CustomerService {
    public partial class CustomerService : CommonService.CommonService {

        public CustomerDefaultListModel FindCustomerDefaultsListModel(int companyId,
                                                                      int index = 0,
                                                                      int pageNo = 1,
                                                                      int pageSize = Int32.MaxValue,
                                                                      string search = "") {
            var model = new CustomerDefaultListModel();

            // Do a case-insensitive search
            model.GridIndex = index;
            var allItems = db.FindCustomerDefaults(companyId)
                             .Where(cd => string.IsNullOrEmpty(search) ||
                                          (cd.Country.CountryName.ToLower().Contains(search.ToLower()) ||
                                           cd.Postcode != null && cd.Postcode.ToLower().Contains(search.ToLower())));

            model.TotalRecords = allItems.Count();
            foreach (var item in allItems.Skip((pageNo - 1) * pageSize)
                                        .Take(pageSize)) {
                model.Items.Add(mapToModel(item));
            }
            return model;
        }

        private CustomerDefaultModel mapToModel(CustomerDefault item) {
            var newItem = Mapper.Map<CustomerDefault, CustomerDefaultModel>(item);

            if (item.Country != null) newItem.CountryNameText = item.Country.CountryName;
            if (item.Currency != null) newItem.CurrencyCodeText = item.Currency.CurrencyCode;
            if (item.LOVItem_CustomerType != null) newItem.CustomerTypeText = item.LOVItem_CustomerType.ItemText;
            if (item.User_SalesPerson != null) newItem.SalesPersonText = (item.User_SalesPerson.FirstName + " " + item.User_SalesPerson.LastName).Trim();
            if (item.PaymentTerm != null) newItem.PaymentTermsText = item.PaymentTerm.TermsOfPaymentId;
            if (item.TaxCode != null) newItem.TaxCodeText = item.TaxCode.TaxCode1;
            if (item.TaxCode_IfNoTaxId != null) newItem.TaxCodeWithoutTaxIdText = item.TaxCode_IfNoTaxId.TaxCode1;

            return newItem;
        }

        private void mapToEntity(CustomerDefaultModel model, CustomerDefault entity) {
            Mapper.Map<CustomerDefaultModel, CustomerDefault>(model, entity);
        }

        public CustomerDefaultModel FindCustomerDefaultModel(int id, CompanyModel company, bool bCreateEmptyIfNotfound = true) {
            CustomerDefaultModel model = null;

            var c = db.FindCustomerDefault(id);
            if (c == null) {
                if (bCreateEmptyIfNotfound) model = new CustomerDefaultModel { CompanyId = company.Id };
            } else {
                model = mapToModel(c);
            }

            return model;
        }

        public Error InsertOrUpdateCustomerDefault(CustomerDefaultModel defaults, string lockGuid = "") {
            var error = validateCustomerDefaultModel(defaults);
            if (!error.IsError) {
                // Check that the lock is still current
                if (!db.IsLockStillValid(typeof(CustomerDefault).ToString(), defaults.Id, lockGuid)) {
                    error.SetError(EvolutionResources.errRecordChangedByAnotherUser, "Name");

                } else {
                    CustomerDefault temp = null;
                    if (defaults.Id != 0) temp = db.FindCustomerDefault(defaults.Id);
                    if (temp == null) temp = new CustomerDefault();

                    mapToEntity(defaults, temp);
                    if (temp.CountryId == 0) temp.CountryId = null;
                    if (temp.PriceLevelId == 0) temp.PriceLevelId = null;
                    if (temp.SalespersonId == 0) temp.SalespersonId = null;
                    if (temp.CustomerTypeId == 0) temp.CustomerTypeId = null;
                    if (temp.TaxCodeId == 0) temp.TaxCodeId = null;
                    if (temp.TaxCodeWithoutTaxId == 0) temp.TaxCodeWithoutTaxId = null;

                    db.InsertOrUpdateCustomerDefault(temp);
                    defaults.Id = temp.Id;
                }
            }
            return error;
        }

        public void DeleteCustomerDefault(int id) {
            db.DeleteCustomerDefault(id);
        }

        public string LockCustomerDefault(CustomerDefaultModel model) {
            return db.LockRecord(typeof(CustomerDefault).ToString(), model.Id);
        }

        private Error validateCustomerDefaultModel(CustomerDefaultModel model) {
            var error = isValidNonRequiredString(getFieldValue(model.Postcode), 20, "Postcode", EvolutionResources.errTextDataRequiredInField);
            return error;
        }

        public CustomerModel CreateCustomer(CompanyModel company, int? countryId, string postCode = "") {
            var model = new CustomerModel { CompanyId = company.Id };
            SetCustomerDefaults(company, model, countryId, postCode);
            return model;
        }

        public void SetCustomerDefaults(CompanyModel company, CustomerModel customer,
                                        string countryName, string postCode) {
            var country = db.FindCountry(countryName);
            if(country != null) {
                SetCustomerDefaults(company, customer, country.Id, postCode);
            } else {
                int? temp = null;
                SetCustomerDefaults(company, customer, temp, postCode);
            }
        }

        public void SetCustomerDefaults(CompanyModel company, CustomerModel customer, 
                                        int? countryId, string postCode) {
            customer.CompanyId = company.Id;
            customer.CreatedDate = DateTimeOffset.Now;

            var defaults = db.FindCustomerDefaults(company.Id, countryId, postCode);
            if(defaults != null) {
                // Populate the defaults
                if(defaults.CurrencyId != null) customer.CurrencyId = defaults.CurrencyId.Value;
                if(defaults.TermId != null) customer.PaymentTermId = defaults.TermId.Value;
                if (defaults.PriceLevel != null) customer.PriceLevelId = defaults.PriceLevelId.Value;
                
                if(defaults.TaxCodeId != null) customer.TaxCodeId = defaults.TaxCodeId.Value;
                //customer.TaxCodeWithoutTaxId = defaults.;
                customer.CreditLimit = defaults.CreditLimit;
                customer.VolumeDiscount = defaults.VolumeDiscount;
                //customer.PrintedForm = defaults.;
                customer.ShippingMethodId = defaults.ShippingMethodId;
                //customer.SalespersonId = defaults.;
                customer.FreightCarrierId = defaults.FreightCarrierId;
                //customer.IsManualFreight = defaults.;
                customer.OnHold = defaults.OnHold;
                customer.Enabled = defaults.Enabled;
                customer.SendInvoices = defaults.SendInvoices;
                customer.EmailAcctMgrOnSaleChange = defaults.EmailAcctMgrOnSaleChange;
                customer.EmailAcctMgrOnLinkedPurchaseChange = defaults.EmailAcctMgrOnLinkedPurchaseChange;
                customer.RequireAuthorisation4OrderQtyChange = defaults.RequireAuthorisation4OrderQtyChange;
                customer.AllowSalesInNonMSQMultiples = defaults.AllowSalesInNonMSQMultiples;
                //customer.ProductLabelName = defaults.ProductLabelName;
                customer.SendPOSFile = defaults.SendPOSFile;
                customer.RemoveCustNameFromAddressWhenDrop = defaults.RemoveCustNameFromAddressWhenDrop;
                customer.ExcludeFromSalesGraphs = defaults.ExcludeFromSalesGraphs;
                customer.CustomerTypeId = defaults.CustomerTypeId;
                customer.FreightRate = defaults.FreightRate;
                customer.MinFreightPerOrder = defaults.MinFreightPerOrder;
                customer.MinFreightThreshold = defaults.MinFreightThreshold;
                customer.FreightWhenBelowThreshold = defaults.FreightWhenBelowThreshold;
                //customer.DefaultTemplateType = defaults.DefaultTemplateType;
            }
        }
    }
}
