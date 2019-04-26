using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Evolution.CommonService;
using Evolution.DAL;
using Evolution.Extensions;
using Evolution.Enumerations;
using Evolution.Models.Models;
using Evolution.Resources;
using AutoMapper;

namespace Evolution.CustomerService {
    public partial class CustomerService : CommonService.CommonService {

        #region Public members    

        public List<ListItemModel> FindCustomersListItemModel(CompanyModel company, string search, int maxRows = 0) {
            List<ListItemModel> model = new List<ListItemModel>();

            if (maxRows > 0) {
                foreach (var customer in db.FindCustomers(company.Id)
                                           .Where(c => string.IsNullOrEmpty(search) ||
                                                       c.Name.Contains(search))
                                           .Take(maxRows)) {
                    model.Add(new ListItemModel(customer.Name, customer.Id.ToString()));
                }
            } else {
                foreach (var customer in db.FindCustomers(company.Id)
                                          .Where(c => string.IsNullOrEmpty(search) ||
                                                      c.Name.Contains(search))) {
                    model.Add(new ListItemModel(customer.Name, customer.Id.ToString()));
                }
            }
            return model;
        }

        public CustomerListModel FindCustomersListModel(int companyId,
                                                        int index = 0, 
                                                        int pageNo = 1, 
                                                        int pageSize = Int32.MaxValue, 
                                                        string search = "",
                                                        int cardRecordId = 0,
                                                        int acctMgrId = 0,
                                                        int countryId = 0,
                                                        int regionId = 0,
                                                        string sortColumn = "", SortOrder sortOrder = SortOrder.Asc) {
            var model = new CustomerListModel();
            string srchStr = search.ToLower();

            // Do a case-insensitive search
            model.GridIndex = index;
            var allItems = db.FindCustomers(companyId, sortColumn, sortOrder, true)
                             .Where(c => ((string.IsNullOrEmpty(search) ||
                                           (c.Name != null && c.Name.ToLower().Contains(srchStr)) ||
                                           c.CustomerAddresses.Where(ca => ca.Street.ToLower().Contains(srchStr) ||
                                                                           ca.StreetLine1.ToLower().Contains(srchStr) ||
                                                                           ca.City.ToLower().Contains(srchStr) ||
                                                                           ca.State.ToLower().Contains(srchStr) ||
                                                                           ca.Postcode.ToLower().Contains(srchStr))
                                                              .Count() > 0) &&
                                          (acctMgrId == 0 || c.BrandCategorySalesPersons.Where(bcsp => bcsp.UserId == acctMgrId).Count() > 0) &&
                                          (countryId == 0 || c.CustomerAddresses.Where(ca => ca.CountryId == countryId).Count() > 0) &&
                                          (regionId == 0 || c.RegionId == regionId) &&
                                          (cardRecordId == 0 || c.CardRecordId == cardRecordId)));

            model.TotalRecords = allItems.Count();
            foreach (var item in allItems.Skip((pageNo - 1) * pageSize)
                                        .Take(pageSize)) {
                model.Items.Add(MapToModel(item));
            }
            return model;
        }

        public CustomerModel MapToModel(Customer item) {
            var newItem = Mapper.Map<Customer, CustomerModel>(item);

            if (newItem.FreightRate != null) newItem.FreightRate *= 100;
            if (item.Region != null) newItem.RegionText = item.Region.RegionName;

            return newItem;
        }

        public CustomerModel MapToModel(CustomerModel model) {
            var newItem = Mapper.Map<CustomerModel, CustomerModel>(model);
            return newItem;
        }

        public Customer MapToEntity(Customer entity) {
            var newItem = Mapper.Map<Customer, Customer>(entity);
            return newItem;
        }

        private void mapToEntity(CustomerModel model, Customer entity) {
            Mapper.Map<CustomerModel, Customer>(model, entity);
            if (entity.FreightRate != null) entity.FreightRate /= 100;
        }

        public CustomerModel FindCustomerModel(int id, CompanyModel company, bool bCreateEmptyIfNotfound = true) {
            CustomerModel model = null;

            var c = db.FindCustomer(id);
            if (c == null) {
                if (bCreateEmptyIfNotfound) model = new CustomerModel { CompanyId = company.Id };
            } else {
                model = MapToModel(c);
            }

            return model;
        }

        public CustomerModel FindCustomerModel(int companyId, string customerName) {
            CustomerModel model = null;

            var c = db.FindCustomer(companyId, customerName);
            if (c != null)
                model = MapToModel(c);

            return model;
        }

        public Error InsertOrUpdateCustomer(CustomerModel customer, UserModel user = null, string lockGuid = "") {
            var error = ValidateCustomerModel(customer);
            if (!error.IsError) {
                // Check that the lock is still current
                if (!db.IsLockStillValid(typeof(Customer).ToString(), customer.Id, lockGuid)) {
                    error.SetError(EvolutionResources.errRecordChangedByAnotherUser, "Name");

                } else {
                    Customer temp = null;
                    if (customer.Id != 0) temp = db.FindCustomer(customer.Id);
                    if (temp == null) temp = new Customer();

                    var before = MapToEntity(temp);

                    mapToEntity(customer, temp);

                    db.InsertOrUpdateCustomer(temp);
                    customer.Id = temp.Id;

                    logChanges(before, temp, user);
                }
            }
            return error;
        }

        public void DeleteCustomer(int companyId, int customerId) {

            string siteFolder = GetConfigurationSetting("SiteFolder", "");

            var fileList = db.DeleteCustomer(companyId, customerId, (int)NoteType.Customer);

            foreach (var media in fileList) {
                try {
                    string fileName = siteFolder + media.FileName.Replace("/", "\\");
                    File.Delete(fileName);
                } catch { }
            }
        }

        public string LockCustomer(CustomerModel model) {
            return db.LockRecord(typeof(Customer).ToString(), model.Id);
        }

        public int GetCustomerCount(CompanyModel company) {
            return db.FindCustomers(company.Id, "", SortOrder.Asc, true)
                     .Count();
        }

        public Error ValidateCustomerModel(CustomerModel model) {
            if (model.CreatedDate == null) model.CreatedDate = DateTime.Now;

            var error = isValidRequiredString(getFieldValue(model.Name), 52, "Name", EvolutionResources.errCustomerNameMustBeEntered);

            if (!error.IsError) {
                // Check if a customer with the same name already exists
                var customer = db.FindCustomer(model.CompanyId, model.Name);
                if (customer != null &&                 // Record was found
                    ((customer.Id == 0 ||               // when creating new or
                      customer.Id != model.Id))) {      // when updating existing
                    error.SetError(EvolutionResources.errCustomerAlreadyExists, "Name");
                }
            }

            return error;
        }

        #endregion

        #region Private methods

        private void logChanges(Customer before, Customer after, UserModel user) {
            AuditService.LogChanges(typeof(Customer).ToString(), BusinessArea.CustomerDetails, user, before, after);
        }

        #endregion
    }
}
