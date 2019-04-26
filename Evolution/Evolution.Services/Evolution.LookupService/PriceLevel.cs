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

        public List<PriceLevelModel> FindPriceLevelsModel(int companyId) {
            List<PriceLevelModel> model = new List<PriceLevelModel>();

            foreach(var item in db.FindPriceLevels(companyId)) {
                model.Add(MapToModel(item));
            }
            return model;
        }

        public List<ListItemModel> FindPriceLevelsListItemModel(CompanyModel company) {
            return db.FindPriceLevels(company.Id)
                     .Select(c => new ListItemModel {
                         Id = c.Id.ToString(),
                         Text = c.Description,
                         ImageURL = ""
                     })
                     .ToList();
        }

        public PriceLevelListModel FindPriceLevelsListModel(int companyId, int index, int pageNo, int pageSize, string search) {
            var model = new PriceLevelListModel();

            // Do a case-insensitive search
            model.GridIndex = index;
            var allItems = db.FindPriceLevels(companyId, true)
                             .Where(pl => string.IsNullOrEmpty(search) ||
                                          (pl.Mneumonic != null && pl.Mneumonic.ToLower().Contains(search.ToLower())) ||
                                          (pl.Description != null && pl.Description.ToLower().Contains(search.ToLower())));

            model.TotalRecords = allItems.Count();
            foreach(var item in allItems.Skip((pageNo - 1) * pageSize)
                                        .Take(pageSize)) {
                model.Items.Add(MapToModel(item));
            }
            return model;
        }

        public PriceLevelModel FindPriceLevelModel(int id, bool bCreateEmptyIfNotfound = true) {
            PriceLevelModel model = null;

            var pl = db.FindPriceLevel(id);
            if (pl == null) {
                if(bCreateEmptyIfNotfound) model = new PriceLevelModel();

            } else {
                model = MapToModel(pl);
            }

            return model;
        }

        public PriceLevelModel MapToModel(PriceLevel item) {
            var newItem = Mapper.Map<PriceLevel, PriceLevelModel>(item);
            return newItem;
        }

        public PriceLevelModel Clone(PriceLevelModel item) {
            var newItem = Mapper.Map<PriceLevelModel, PriceLevelModel>(item);
            return newItem;
        }

        public Error InsertOrUpdatePriceLevel(PriceLevelModel priceLevel, UserModel user, string lockGuid) {
            var error = validateModel(priceLevel);
            if (!error.IsError) {
                // Check that the lock is still current
                if (!db.IsLockStillValid(typeof(PriceLevel).ToString(), priceLevel.Id, lockGuid)) {
                    error.SetError(EvolutionResources.errRecordChangedByAnotherUser, "Mneumonic");

                } else {
                    PriceLevel temp = null;
                    if (priceLevel.Id != 0) temp = db.FindPriceLevel(priceLevel.Id);
                    if (temp == null) temp = new PriceLevel();

                    Mapper.Map<PriceLevelModel, PriceLevel>(priceLevel, temp);

                    db.InsertOrUpdatePriceLevel(temp);
                    priceLevel.Id = temp.Id;
                }
            }
            return error;
        }

        public void DeletePriceLevel(int id) {
            db.DeletePriceLevel(id);
        }

        public string LockPriceLevel(PriceLevelModel model) {
            return db.LockRecord(typeof(PriceLevel).ToString(), model.Id);
        }

        public void CopyPriceLevels(CompanyModel source, CompanyModel target, UserModel user) {
            foreach (var priceLevel in FindPriceLevelsModel(source.Id)) {
                var newItem = Mapper.Map<PriceLevelModel, PriceLevelModel>(priceLevel);
                newItem.Id = 0;
                newItem.CompanyId = target.Id;
                InsertOrUpdatePriceLevel(newItem, user, "");
            }
        }

        #endregion

        #region Private methods

        private Error validateModel(PriceLevelModel model) {
            var error = isValidRequiredString(getFieldValue(model.Mneumonic), 3, "Mneumonic", EvolutionResources.errMneumonicRequired);
            if (!error.IsError) error = isValidNonRequiredString(getFieldValue(model.Description), 30, "Description", EvolutionResources.errTextDataRequiredInField);
            if (!error.IsError) error = isValidNonRequiredString(getFieldValue(model.ImportPriceLevel), 1, "ImportPriceLevel", EvolutionResources.errTextDataRequiredInField);
            if (!error.IsError) error = isValidNonRequiredString(getFieldValue(model.ImportSalesTaxCalcMethod), 1, "ImportSaleTaxCalcMethod", EvolutionResources.errTextDataRequiredInField);

            if (!error.IsError) {
                // Check if a record with the same name already exists
                var priceLevel = db.FindPriceLevel(model.CompanyId, model.Mneumonic);
                if (priceLevel != null &&                 // Record was found
                    ((priceLevel.Id == 0 ||               // when creating new or
                      priceLevel.Id != model.Id))) {      // when updating existing
                    error.SetError(EvolutionResources.errItemAlreadyExists, "Mneumonic");
                }
            }

            return error;
        }

        #endregion
    }
}
