using Evolution.CommonService;
using Evolution.DAL;
using Evolution.Enumerations;
using Evolution.Models.Models;
using Evolution.Resources;
using AutoMapper;

namespace Evolution.CustomerService {
    public partial class CustomerService : CommonService.CommonService {

        #region Public members

        public CustomerAdditionalInfoModel FindCustomerAdditionalInfoModel(int customerId, CompanyModel company) {
            CustomerAdditionalInfoModel model = new CustomerAdditionalInfoModel { Id = customerId, CompanyId = company.Id };

            var c = db.FindCustomer(customerId);
            if (c != null) model = MapToAdditionalInfoModel(c);

            return model;
        }

        public CustomerAdditionalInfoModel MapToAdditionalInfoModel(Customer item) {
            var newItem = Mapper.Map<Customer, CustomerAdditionalInfoModel>(item);
            return newItem;
        }

        public void MapToEntity(CustomerAdditionalInfoModel model, Customer entity) {
            Mapper.Map<CustomerAdditionalInfoModel, Customer>(model, entity);
        }

        public Error InsertOrUpdateCustomerAdditionalInfo(CustomerAdditionalInfoModel info, UserModel user, string lockGuid) {
            var error = validateCustomerAdditionalInfoModel(info);
            if (!error.IsError) {
                // Check that the lock is still current
                if (!db.IsLockStillValid(typeof(Customer).ToString(), info.Id, lockGuid)) {
                    error.SetError(EvolutionResources.errRecordChangedByAnotherUser, "DeliveryInstructions");

                } else {
                    Customer temp = null;
                    if (info.Id != 0) {
                        temp = db.FindCustomer(info.Id);
                        if (temp != null) {
                            logChanges(temp, info, user);

                            MapToEntity(info, temp);

                            db.InsertOrUpdateCustomer(temp);
                            info.Id = temp.Id;
                        }
                    }
                }
            }
            return error;
        }

        public string LockCustomerAdditionalInfo(CustomerAdditionalInfoModel model) {
            return db.LockRecord(typeof(Customer).ToString(), model.Id);
        }

        #endregion

        #region Private members

        private Error validateCustomerAdditionalInfoModel(CustomerAdditionalInfoModel model) {
            var error = isValidNonRequiredString(getFieldValue(model.EDI_VendorNo), 30, "EDI_VendorNo", EvolutionResources.errTextDataRequiredInField);
            if (!error.IsError) error = isValidRequiredInt(model.SourceId, "SourceId", EvolutionResources.errSelectionRequiredInField);
            return error;
        }
        private void logChanges(Customer dbRecord, CustomerAdditionalInfoModel after, UserModel user) {
            CustomerAdditionalInfoModel before = new CustomerAdditionalInfoModel();
            Mapper.Map(dbRecord, before);

            AuditService.LogChanges(typeof(Customer).ToString(), BusinessArea.CustomerAdditionalInfo, user, before, after);
        }

        #endregion
    }
}
