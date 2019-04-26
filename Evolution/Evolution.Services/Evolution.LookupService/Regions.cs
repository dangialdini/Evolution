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

        public List<RegionModel> FindRegionsModel(int companyId) {
            List<RegionModel> model = new List<RegionModel>();

            foreach(var item in db.FindRegions(companyId)) {
                model.Add(MapToModel(item));
            }
            return model;
        }

        public List<ListItemModel> FindRegionsListItemModel(CompanyModel company, bool bInsertBlank = false) {
            return FindRegionsListItemModel(company.Id, bInsertBlank);
        }

        public List<ListItemModel> FindRegionsListItemModel(int companyId, bool bInsertBlank = false) {
            var list = db.FindRegions(companyId)
                     .Select(r => new ListItemModel {
                         Id = r.Id.ToString(),
                         Text = r.RegionName,
                         ImageURL = ""
                     })
                     .ToList();
            if (bInsertBlank) list.Insert(0, new ListItemModel("", "0"));
            return list;
        }

        public RegionListModel FindRegionsListModel(int companyId, int index, int pageNo, int pageSize, string search) {
            var model = new RegionListModel();

            decimal numValue = 0;
            bool bGotNum = decimal.TryParse(search, out numValue);

            // Do a case-insensitive search
            model.GridIndex = index;
            var allItems = db.FindRegions(companyId, true)
                            .Where(r => r.CompanyId == companyId &&
                                        (string.IsNullOrEmpty(search) ||
                                         (r.RegionName != null && r.RegionName.ToLower().Contains(search.ToLower())) ||
                                         (r.CountryCode != null && r.CountryCode.ToLower().Contains(search.ToLower())) ||
                                         (r.PostCodeFrom != null && r.PostCodeFrom.ToLower().Contains(search.ToLower())) ||
                                         (r.PostCodeTo != null && r.PostCodeTo.ToLower().Contains(search.ToLower())) ||
                                         (bGotNum && r.FreightRate == numValue)));

            model.TotalRecords = allItems.Count();
            foreach(var item in allItems.Skip((pageNo - 1) * pageSize)
                                        .Take(pageSize)) {
                model.Items.Add(MapToModel(item));
            }
            return model;
        }

        public RegionModel FindRegionModel(int id, bool bCreateEmptyIfNotfound = true) {
            RegionModel model = null;

            var r = db.FindRegion(id);
            if (r == null) {
                if(bCreateEmptyIfNotfound) model = new RegionModel();

            } else {
                model = MapToModel(r);
            }

            return model;
        }

        public RegionModel MapToModel(Region item) {
            var newItem = Mapper.Map<Region, RegionModel>(item);
            return newItem;
        }

        public RegionModel Clone(RegionModel item) {
            var newItem = Mapper.Map<RegionModel, RegionModel>(item);
            return newItem;
        }

        public Error InsertOrUpdateRegion(RegionModel region, UserModel user, string lockGuid) {
            var error = validateModel(region);
            if (!error.IsError) {
                // Check that the lock is still current
                if (!db.IsLockStillValid(typeof(Region).ToString(), region.Id, lockGuid)) {
                    error.SetError(EvolutionResources.errRecordChangedByAnotherUser, "RegionName");

                } else {
                    Region temp = null;
                    if (region.Id != 0) temp = db.FindRegion(region.Id);
                    if (temp == null) temp = new Region();

                    Mapper.Map<RegionModel, Region>(region, temp);

                    db.InsertOrUpdateRegion(temp);
                    region.Id = temp.Id;
                }
            }
            return error;
        }

        public void DeleteRegion(int id) {
            db.DeleteRegion(id);
        }

        public string LockRegion(RegionModel model) {
            return db.LockRecord(typeof(Region).ToString(), model.Id);
        }

        public void CopyRegions(CompanyModel source, CompanyModel target, UserModel user) {
            foreach (var region in FindRegionsModel(source.Id)) {
                var newItem = Mapper.Map< RegionModel, RegionModel>(region);
                newItem.Id = 0;
                newItem.CompanyId = target.Id;
                InsertOrUpdateRegion(newItem, user, "");
            }
        }

        #endregion

        #region Private methods

        private Error validateModel(RegionModel model) {
            var error = isValidRequiredString(getFieldValue(model.RegionName), 50, "RegionName", EvolutionResources.errRegionNameRequired);
            if (!error.IsError) error = isValidNonRequiredString(getFieldValue(model.CountryCode), 3, "CountryCode", EvolutionResources.errTextDataRequiredInField);
            if (!error.IsError) error = isValidNonRequiredString(getFieldValue(model.PostCodeFrom), 10, "PostCodeFrom", EvolutionResources.errTextDataRequiredInField);
            if (!error.IsError) error = isValidNonRequiredString(getFieldValue(model.PostCodeTo), 10, "PostCodeTo", EvolutionResources.errTextDataRequiredInField);

            if (!error.IsError) {
                // Check if a record with the same name already exists
                var region = db.FindRegion(model.CompanyId, model.RegionName);
                if (region != null &&                 // Record was found
                    ((region.Id == 0 ||               // when creating new or
                      region.Id != model.Id))) {      // when updating existing
                    error.SetError(EvolutionResources.errItemAlreadyExists, "RegionName");
                }
            }

            return error;
        }

        #endregion
    }
}
