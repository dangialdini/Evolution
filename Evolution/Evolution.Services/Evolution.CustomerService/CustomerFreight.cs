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

        public CustomerFreightModel FindCustomerFreightModel(CustomerModel customer, CompanyModel company) {
            return FindCustomerFreightModel(customer.Id, company);
        }

        public CustomerFreightModel FindCustomerFreightModel(int customerId, CompanyModel company) {
            CustomerFreightModel model = new CustomerFreightModel { CompanyId = company.Id, CustomerId = customerId };

            var c = db.FindCustomer(customerId);
            if (c != null) {
                model = MapToCustomerFreightModel(c);
                model.CustomerId = c.Id;
            }

            return model;
        }

        public CustomerFreightModel MapToCustomerFreightModel(Customer item) {
            var newItem = Mapper.Map<Customer, CustomerFreightModel>(item);
            return newItem;
        }

        public void MapToEntity(CustomerFreightModel model, Customer entity) {
            Mapper.Map<CustomerFreightModel, Customer>(model, entity);
        }

        public Error InsertOrUpdateCustomerFreight(CustomerFreightModel freight, UserModel user, string lockGuid) {
            var error = validateCustomerFreightModel(freight);
            if (!error.IsError) {
                // Check that the lock is still current
                if (!db.IsLockStillValid(typeof(Customer).ToString(), freight.CustomerId, lockGuid)) {
                    error.SetError(EvolutionResources.errRecordChangedByAnotherUser, "ContactFirstname");

                } else {
                    Customer temp = null;
                    if (freight.CustomerId != 0) {
                        temp = db.FindCustomer(freight.CustomerId);
                        if (temp != null) {
                            MapToEntity(freight, temp);

                            db.InsertOrUpdateCustomer(temp);
                        }
                    }
                }
            }
            return error;
        }

        public string LockCustomerFreight(CustomerFreightModel model) {
            return db.LockRecord(typeof(Customer).ToString(), model.CustomerId);
        }

        private Error validateCustomerFreightModel(CustomerFreightModel model) {
            var error = isValidNonRequiredString(getFieldValue(model.DeliveryInstructions), 255, "DeliveryInstructions", EvolutionResources.errTextDataRequiredInField);
            if (!error.IsError) error = isValidNonRequiredString(getFieldValue(model.DeliveryContact), 30, "DeliveryInstructions", EvolutionResources.errTextDataRequiredInField);
            if (!error.IsError) error = isValidNonRequiredString(getFieldValue(model.ShipMethodAccount), 20, "ShipMethodAccount", EvolutionResources.errTextDataRequiredInField);
            if (!error.IsError) error = isValidNonRequiredString(getFieldValue(model.WarehouseInstructions), 100, "WarehouseInstructions", EvolutionResources.errTextDataRequiredInField);

            return error;
        }
    }
}
