using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Evolution.DAL;
using Evolution.Models.Models;
using Evolution.CommonService;
using Evolution.Resources;
using Evolution.Extensions;
using AutoMapper;

namespace Evolution.CustomerService {
    public partial class CustomerService : CommonService.CommonService {

        public CustomerConflictListModel FindCustomerConflictsListModel(int customerId, int index, int pageNo, int pageSize) {
            var model = new CustomerConflictListModel();

            model.GridIndex = index;
            var allItems = db.FindCustomerConflicts(customerId)
                             .OrderBy(cc => cc.Id);

            model.TotalRecords = allItems.Count();
            foreach (var item in allItems.Skip((pageNo - 1) * pageSize)
                                        .Take(pageSize)) {
                var newItem = MapToModel(item);
                newItem.SensitiveWithCompanyName = item.Customer1.Name;
                model.Items.Add(newItem);
            }
            return model;
        }

        public CustomerConflictModel MapToModel(CustomerConflictSensitivity item) {
            var newItem = Mapper.Map<CustomerConflictSensitivity, CustomerConflictModel>(item);
            return newItem;
        }

        public void MapToEntity(CustomerConflictModel model, CustomerConflictSensitivity entity) {
            Mapper.Map<CustomerConflictModel, CustomerConflictSensitivity>(model, entity);
        }

        public CustomerConflictModel FindCustomerConflictModel(int id, CompanyModel company, CustomerModel customer, bool bCreateEmptyIfNotfound = true) {
            return FindCustomerConflictModel(id, company, customer.Id, bCreateEmptyIfNotfound);
        }

        public CustomerConflictModel FindCustomerConflictModel(int id, CompanyModel company, int customerId, bool bCreateEmptyIfNotfound = true) {
            CustomerConflictModel model = null;

            var a = db.FindCustomerConflict(id);
            if (a == null) {
                if (bCreateEmptyIfNotfound) model = new CustomerConflictModel { CompanyId = company.Id, CustomerId = customerId };
            } else {
                model = MapToModel(a);
            }

            return model;
        }

        public Error InsertOrUpdateCustomerConflict(CustomerConflictModel conflict, UserModel user, string lockGuid) {
            var error = validateConflictModel(conflict);
            if (!error.IsError) {
                // Check that the lock is still current
                if (!db.IsLockStillValid(typeof(CustomerConflictSensitivity).ToString(), conflict.Id, lockGuid)) {
                    error.SetError(EvolutionResources.errRecordChangedByAnotherUser, "ConflictFirstname");

                } else {
                    CustomerConflictSensitivity temp = null;
                    if (conflict.Id != 0) temp = db.FindCustomerConflict(conflict.Id);
                    if (temp == null) temp = new CustomerConflictSensitivity();

                    MapToEntity(conflict, temp);

                    db.InsertOrUpdateCustomerConflict(temp);
                    conflict.Id = temp.Id;
                }
            }
            return error;
        }

        public void DeleteCustomerConflict(int id) {
            db.DeleteCustomerConflict(id);
        }

        public string LockCustomerConflict(CustomerConflictModel model) {
            return db.LockRecord(typeof(CustomerConflictSensitivity).ToString(), model.Id);
        }

        private Error validateConflictModel(CustomerConflictModel model) {
            var error = new Error();
            /*
            string firstName = getFieldValue(model.ConflictFirstname),
                    surname = getFieldValue(model.ConflictSurname),
                    email = getFieldValue(model.ConflictEmail);

            if (string.IsNullOrEmpty(firstName)) {
                error.SetError(EvolutionResources.errFirstNameRequired, "ConflictFirstname");

            } else if (string.IsNullOrEmpty(surname)) {
                error.SetError(EvolutionResources.errSurnameRequired, "ConflictSurname");

            } else if (!string.IsNullOrEmpty(email)) {
                // If email address is entered, make sure it is valid
                if (!email.IsValidEMail()) {
                    error.SetError(EvolutionResources.errInvalidEMailAddress, "ConflictEmail");
                }
            }
            */
            return error;
        }
    }
}
