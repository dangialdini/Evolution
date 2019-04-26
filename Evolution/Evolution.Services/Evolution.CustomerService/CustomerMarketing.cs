using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using Evolution.DAL;
using Evolution.Models.Models;
using Evolution.CommonService;
using Evolution.Resources;
using Evolution.Extensions;
using AutoMapper;

namespace Evolution.CustomerService {
    public partial class CustomerService : CommonService.CommonService {

        public CustomerMarketingListModel FindCustomerMarketingListModel(int customerId, int index, int pageNo, int pageSize) {
            var model = new CustomerMarketingListModel();

            model.GridIndex = index;
            var allItems = db.FindMarketingGroupSubscriptions(customerId);

            model.TotalRecords = allItems.Count();
            foreach (var item in allItems.Skip((pageNo - 1) * pageSize)
                                        .Take(pageSize)) {
                var newItem = MapToModel(item);
                model.Items.Add(newItem);
            }
            return model;
        }

        public CustomerMarketingModel MapToModel(MarketingGroupSubscription item) {
            var newItem = Mapper.Map<MarketingGroupSubscription, CustomerMarketingModel>(item);

            newItem.ContactName = (item.CustomerContact.ContactFirstname + " " + item.CustomerContact.ContactSurname).Trim();
            newItem.GroupName = item.MarketingGroup.MarketingGroupName;
            return newItem;
        }

        public void MapToEntity(CustomerMarketingModel model, MarketingGroupSubscription entity) {
            Mapper.Map<CustomerMarketingModel, MarketingGroupSubscription>(model, entity);
        }

        public CustomerMarketingModel FindCustomerMarketingModel(int id, CompanyModel company, CustomerModel customer, bool bCreateEmptyIfNotfound = true) {
            return FindCustomerMarketingModel(id, company, customer.Id, bCreateEmptyIfNotfound);
        }

        public CustomerMarketingModel FindCustomerMarketingModel(int id, CompanyModel company, int customerId, bool bCreateEmptyIfNotfound = true) {
            CustomerMarketingModel model = null;

            var a = db.FindMarketingGroupSubscription(id);
            if (a == null) {
                if (bCreateEmptyIfNotfound) model = new CustomerMarketingModel { CompanyId = company.Id, CustomerId = customerId };
            } else {
                model = MapToModel(a);
            }

            return model;
        }

        public Error InsertOrUpdateCustomerMarketing(CustomerMarketingModel marketing, UserModel user, string lockGuid) {
            var error = validateMarketingModel(marketing);
            if (!error.IsError) {
                // Check that the lock is still current
                if (!db.IsLockStillValid(typeof(MarketingGroupSubscription).ToString(), marketing.Id, lockGuid)) {
                    error.SetError(EvolutionResources.errRecordChangedByAnotherUser, "MarketingGroupId");

                } else {
                    MarketingGroupSubscription temp = null;
                    if (marketing.Id != 0) temp = db.FindMarketingGroupSubscription(marketing.Id);
                    if (temp == null) temp = new MarketingGroupSubscription();

                    MapToEntity(marketing, temp);

                    db.InsertOrUpdateMarketingGroupSubscription(temp);
                    marketing.Id = temp.Id;

                    db.Entry(temp).State = EntityState.Detached;     // Force EF to update FK's
                }
            }
            return error;
        }

        public void DeleteCustomerMarketing(int id) {
            db.DeleteMarketingGroupSubscription(id);
        }

        public string LockCustomerMarketing(CustomerMarketingModel model) {
            return db.LockRecord(typeof(MarketingGroupSubscription).ToString(), model.Id);
        }

        private Error validateMarketingModel(CustomerMarketingModel model) {
            var error = new Error();
            return error;
        }
    }
}
