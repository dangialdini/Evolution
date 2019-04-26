using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using Evolution.DAL;
using Evolution.Models.Models;
using Evolution.CommonService;
using Evolution.Enumerations;
using Evolution.Resources;
using AutoMapper;

namespace Evolution.CustomerService {
    public partial class CustomerService : CommonService.CommonService {

        #region Public members

        public CustomerAddressListModel FindCustomerAddressesListModel(int customerId) {
            var model = new CustomerAddressListModel();
            foreach(var item in db.FindCustomerAddresses(customerId)) {
                model.Items.Add(MapToModel(item));
            }
            return model;
        }

        public CustomerAddressListModel FindCustomerAddressesListModel(int customerId, int index, int pageNo, int pageSize, string search) {
            var model = new CustomerAddressListModel();

            // Do a case-insensitive search
            model.GridIndex = index;
            var allItems = db.FindCustomerAddresses(customerId)
                             .Where(ca => string.IsNullOrEmpty(search) ||
                                          (ca.Street != null && ca.Street.ToLower().Contains(search.ToLower())) ||
                                          (ca.City != null && ca.City.ToLower().Contains(search.ToLower())) ||
                                          (ca.State != null && ca.State.ToLower().Contains(search.ToLower())) ||
                                          (ca.Postcode != null && ca.Postcode.ToLower().Contains(search.ToLower())) ||
                                          (ca.StreetLine1 != null && ca.StreetLine1.ToLower().Contains(search.ToLower())) ||
                                          (ca.StreetLine2 != null && ca.StreetLine2.ToLower().Contains(search.ToLower())) ||
                                          (ca.StreetLine3 != null && ca.StreetLine3.ToLower().Contains(search.ToLower())) ||
                                          (ca.StreetLine4 != null && ca.StreetLine4.ToLower().Contains(search.ToLower())))
                             .OrderByDescending(ca => ca.DateStart)
                             .ThenByDescending(ca => ca.DateEnd)
                             .ThenBy(ca => ca.Street);

            model.TotalRecords = allItems.Count();
            foreach (var item in allItems.Skip((pageNo - 1) * pageSize)
                                        .Take(pageSize)) {
                var newItem = MapToModel(item);
                newItem.AddressTypeText = (item.LOVItem_AddressType == null ? "" : item.LOVItem_AddressType.ItemText);
                model.Items.Add(newItem);
            }
            return model;
        }

        public List<CustomerAddressModel> FindCurrentCustomerAddresses(CustomerModel customer, AddressType addrsType) {
            List<CustomerAddressModel> model = new List<CustomerAddressModel>();

            var now = DateTimeOffset.Now;
            foreach(var item in db.FindCustomerAddresses(customer.Id)
                                  .Where(ca => ca.LOVItem_AddressType.ItemValue1 == ((int)addrsType).ToString() &&
                                               ca.DateStart <= now &&
                                               (ca.DateEnd >= now || ca.DateEnd == null))
                                  .OrderBy(ca => ca.DateStart)
                                  .ThenBy(ca => ca.DateEnd)) {

                model.Add(MapToModel(item));
            }
            return model;
        }

        public CustomerAddressModel MapToModel(CustomerAddress item) {
            var newItem = Mapper.Map<CustomerAddress, CustomerAddressModel>(item);

            var addrsType = (item.LOVItem_AddressType == null ? new LOVItem() : item.LOVItem_AddressType);
            newItem.AddressType = (AddressType)Convert.ToInt32(addrsType.ItemValue1);
            newItem.AddressTypeText = addrsType.ItemText;
            if (item.Country != null) newItem.CountryName = item.Country.CountryName;

            return newItem;
        }

        public void MapToEntity(CustomerAddressModel model, CustomerAddress entity) {
            Mapper.Map<CustomerAddressModel, CustomerAddress>(model, entity);
        }

        public CustomerAddressModel FindCustomerAddressModel(int id, CompanyModel company, CustomerModel customer, bool bCreateEmptyIfNotfound = true) {
            return FindCustomerAddressModel(id, company, customer.Id, bCreateEmptyIfNotfound);
        }

        public CustomerAddressModel FindCustomerAddressModel(int id, CompanyModel company, int customerId, bool bCreateEmptyIfNotfound = true) {
            CustomerAddressModel model = null;

            var a = db.FindCustomerAddress(id);
            if (a == null) {
                if (bCreateEmptyIfNotfound) {
                    model = new CustomerAddressModel { CompanyId = company.Id, CustomerId = customerId };

                    // Default the country according to the company default location
                    if(company.DefaultLocationID != null) {
                        var location = db.FindLocation(company.DefaultLocationID.Value);
                        if (location != null) {
                            string countryName = location.Country;
                            if (countryName.ToLower() == "england") countryName = "United Kingdom"; // Handles existing data

                            var country = db.FindCountry(countryName);
                            if (country != null) model.CountryId = country.Id;
                        }
                    }
                }
            } else {
                model = MapToModel(a);
            }

            return model;
        }

        public Error InsertOrUpdateCustomerAddress(CustomerAddressModel address, string lockGuid = "") {
            var error = ValidateAddressModel(address);
            if (!error.IsError) {
                // Check that the lock is still current
                if (!db.IsLockStillValid(typeof(CustomerAddress).ToString(), address.Id, lockGuid)) {
                    error.SetError(EvolutionResources.errRecordChangedByAnotherUser, "Street");

                } else {
                    CustomerAddress temp = null;
                    if (address.Id != 0) temp = db.FindCustomerAddress(address.Id);
                    if (temp == null) temp = new CustomerAddress();

                    MapToEntity(address, temp);

                    db.InsertOrUpdateCustomerAddress(temp);
                    address.Id = temp.Id;

                    db.Entry(temp).State = EntityState.Detached;
                }
            }
            return error;
        }

        public void DeleteCustomerAddress(int id) {
            db.DeleteCustomerAddress(id);
        }

        public string LockCustomerAddress(CustomerAddressModel model) {
            return db.LockRecord(typeof(CustomerAddress).ToString(), model.Id);
        }

        public Error ValidateAddressModel(CustomerAddressModel model) {
            var error = isValidRequiredString(getFieldValue(model.Street), 255, "Street", EvolutionResources.errStreetRequired);
            if (!error.IsError) error = isValidRequiredString(getFieldValue(model.City), 50, "City", EvolutionResources.errCityRequired);
            if (!error.IsError) error = isValidRequiredString(getFieldValue(model.State), 20, "State", EvolutionResources.errStateRequired);
            if (!error.IsError) error = isValidRequiredString(getFieldValue(model.Postcode), 10, "Postcode", EvolutionResources.errPostCodeRequired);

            return error;
        }

        #endregion
    }
}
