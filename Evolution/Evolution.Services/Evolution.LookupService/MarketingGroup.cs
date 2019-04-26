using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Evolution.DAL;
using Evolution.Models.Models;
using Evolution.CommonService;
using Evolution.Resources;
using AutoMapper;

namespace Evolution.LookupService {
    public partial class LookupService {

        #region Public members    

        public List<MarketingGroupModel> FindMarketingGroupsModel(int companyId) {
            List<MarketingGroupModel> model = new List<MarketingGroupModel>();

            foreach(var item in db.FindMarketingGroups(companyId)) {
                model.Add(MapToModel(item));
            }
            return model;
        }

        public List<ListItemModel> FindMarketingGroupsListItemModel(CompanyModel company) {
            return db.FindMarketingGroups(company.Id)
                     .Select(mg => new ListItemModel {
                         Id = mg.Id.ToString(),
                         Text = mg.MarketingGroupName,
                         ImageURL = ""
                     })
                     .ToList();
        }

        public MarketingGroupListModel FindMarketingGroupsListModel(int companyId, int index, int pageNo, int pageSize, string search) {
            var model = new MarketingGroupListModel();

            int numValue = 0;
            bool bGotNum = int.TryParse(search, out numValue);

            // Do a case-insensitive search
            model.GridIndex = index;
            var allItems = db.FindMarketingGroups(companyId, true)
                            .Where(mg => string.IsNullOrEmpty(search) ||
                                         (mg.MarketingGroupName != null && mg.MarketingGroupName.ToLower().Contains(search.ToLower())));

            model.TotalRecords = allItems.Count();
            foreach(var item in allItems.Skip((pageNo - 1) * pageSize)
                                        .Take(pageSize)) {
                model.Items.Add(MapToModel(item));
            }
            return model;
        }

        public MarketingGroupModel FindMarketingGroupModel(int id, bool bCreateEmptyIfNotfound = true) {
            MarketingGroupModel model = null;

            var mg = db.FindMarketingGroup(id);
            if (mg == null) {
                if(bCreateEmptyIfNotfound) model = new MarketingGroupModel();

            } else {
                model = MapToModel(mg);
            }

            return model;
        }

        public MarketingGroupModel MapToModel(MarketingGroup item) {
            var newItem = Mapper.Map<MarketingGroup, MarketingGroupModel>(item);
            return newItem;
        }

        public MarketingGroupModel Clone(MarketingGroupModel item) {
            var newItem = Mapper.Map<MarketingGroupModel, MarketingGroupModel>(item);
            return newItem;
        }

        public Error InsertOrUpdateMarketingGroup(MarketingGroupModel marketingGroup, UserModel user, string lockGuid) {
            var error = validateModel(marketingGroup);
            if (!error.IsError) {
                // Check that the lock is still current
                if (!db.IsLockStillValid(typeof(MarketingGroup).ToString(), marketingGroup.Id, lockGuid)) {
                    error.SetError(EvolutionResources.errRecordChangedByAnotherUser, "MarketingGroupName");

                } else {
                    MarketingGroup temp = null;
                    if (marketingGroup.Id != 0) temp = db.FindMarketingGroup(marketingGroup.Id);
                    if (temp == null) temp = new MarketingGroup();

                    Mapper.Map<MarketingGroupModel, MarketingGroup>(marketingGroup, temp);

                    db.InsertOrUpdateMarketingGroup(temp);
                    marketingGroup.Id = temp.Id;
                }
            }
            return error;
        }

        public void DeleteMarketingGroup(int id) {
            db.DeleteMarketingGroup(id);
        }

        public string LockMarketingGroup(MarketingGroupModel model) {
            return db.LockRecord(typeof(MarketingGroup).ToString(), model.Id);
        }

        public void CopyMarketingGroups(CompanyModel source, CompanyModel target, UserModel user) {
            foreach (var marketingGroup in FindMarketingGroupsModel(source.Id)) {
                var newItem = Mapper.Map<MarketingGroupModel, MarketingGroupModel>(marketingGroup);
                newItem.Id = 0;
                newItem.CompanyId = target.Id;
                InsertOrUpdateMarketingGroup(newItem, user, "");
            }
        }

        #endregion

        #region Private methods

        private Error validateModel(MarketingGroupModel model) {
            string marketingGroupName = getFieldValue(model.MarketingGroupName);

            var error = isValidRequiredString(marketingGroupName, 30, "MarketingGroupName", EvolutionResources.errMarketingGroupNameRequired);

            if (!error.IsError) {
                // Check if a record with the same name already exists
                var forwarder = db.FindMarketingGroup(model.CompanyId, model.MarketingGroupName);
                if (forwarder != null &&                 // Record was found
                    ((forwarder.Id == 0 ||               // when creating new or
                      forwarder.Id != model.Id))) {      // when updating existing
                    error.SetError(EvolutionResources.errItemAlreadyExists, "MarketingGroupName");
                }
            }

            return error;
        }

        #endregion
    }
}
