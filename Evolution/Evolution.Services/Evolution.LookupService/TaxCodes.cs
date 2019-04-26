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

        public List<TaxCodeModel> FindTaxCodesModel(int companyId) {
            List<TaxCodeModel> model = new List<TaxCodeModel>();

            foreach(var item in db.FindTaxCodes(companyId)) {
                model.Add(MapToModel(item));
            }
            return model;
        }

        public List<ListItemModel> FindTaxCodesListItemModel(CompanyModel company) {
            return db.FindTaxCodes(company.Id)
                     .Select(c => new ListItemModel {
                         Id = c.Id.ToString(),
                         Text = c.TaxCode1,
                         ImageURL = ""
                     })
                     .ToList();
        }

        public TaxCodeListModel FindTaxCodesListModel(int companyId, int index, int pageNo, int pageSize, string search) {
            var model = new TaxCodeListModel();

            // Do a case-insensitive search
            model.GridIndex = index;
            var allItems = db.FindTaxCodes(companyId, true)
                            .Where(tc => string.IsNullOrEmpty(search) ||
                                         (tc.TaxCode1 != null && tc.TaxCode1.ToLower().Contains(search.ToLower())) ||
                                         (tc.TaxCodeDescription != null && tc.TaxCodeDescription.ToLower().Contains(search.ToLower())) ||
                                         (tc.TaxCodeTypeId != null && tc.TaxCodeTypeId.ToLower().Contains(search.ToLower())));

            model.TotalRecords = allItems.Count();
            foreach(var item in allItems.Skip((pageNo - 1) * pageSize)
                                        .Take(pageSize)) {
                model.Items.Add(MapToModel(item));
            }
            return model;
        }

        public TaxCodeModel FindTaxCodeModel(int id, bool bCreateEmptyIfNotfound = true) {
            TaxCodeModel model = null;

            var taxCode = db.FindTaxCode(id);
            if (taxCode == null) {
                if(bCreateEmptyIfNotfound) model = new TaxCodeModel();

            } else {
                model = MapToModel(taxCode);
            }

            return model;
        }

        public TaxCodeModel MapToModel(TaxCode item) {
            // The mapping handles TaxCode1 => TaxCode
            var model = Mapper.Map<TaxCode, TaxCodeModel>(item);
            return model;
        }

        public TaxCodeModel Clone(TaxCodeModel item) {
            var model = Mapper.Map<TaxCodeModel, TaxCodeModel>(item);
            return model;
        }

        private void mapToEntity(TaxCodeModel model, TaxCode item) {
            // The mapping handles TaxCode => TaxCode1
            Mapper.Map<TaxCodeModel, TaxCode>(model, item);
        }

        public Error InsertOrUpdateTaxCode(TaxCodeModel taxCode, UserModel user, string lockGuid) {
            var error = validateModel(taxCode);
            if (!error.IsError) {
                // Check that the lock is still current
                if (!db.IsLockStillValid(typeof(TaxCode).ToString(), taxCode.Id, lockGuid)) {
                    error.SetError(EvolutionResources.errRecordChangedByAnotherUser, "TaxCode");

                } else {
                    TaxCode temp = null;
                    if (taxCode.Id != 0) temp = db.FindTaxCode(taxCode.Id);
                    if (temp == null) temp = new TaxCode();

                    mapToEntity(taxCode, temp);

                    db.InsertOrUpdateTaxCode(temp);
                    taxCode.Id = temp.Id;
                }
            }
            return error;
        }

        public void DeleteTaxCode(int id) {
            db.DeleteTaxCode(id);
        }

        public string LockTaxCode(TaxCodeModel model) {
            return db.LockRecord(typeof(TaxCode).ToString(), model.Id);
        }

        public void CopyTaxCodes(CompanyModel source, CompanyModel target, UserModel user) {
            foreach (var taxCode in FindTaxCodesModel(source.Id)) {
                var newItem = Mapper.Map<TaxCodeModel, TaxCodeModel>(taxCode);
                newItem.Id = 0;
                newItem.CompanyId = target.Id;
                InsertOrUpdateTaxCode(newItem, user, "");
            }
        }

        #endregion

        #region Private methods

        private Error validateModel(TaxCodeModel model) {
            var error = isValidRequiredString(getFieldValue(model.TaxCode), 3, "TaxCode", EvolutionResources.errTaxCodeRequired);
            if (!error.IsError) error = isValidRequiredString(getFieldValue(model.TaxCodeDescription), 30, "TaxCodeDescription", EvolutionResources.errTaxCodeDescriptionRequired);
            if (!error.IsError) error = isValidRequiredString(getFieldValue(model.TaxCodeTypeId), 3, "TaxCodeTypeId", EvolutionResources.errTextDataRequiredInField);

            if (!error.IsError) {
                // Check if a record with the same name already exists
                var taxCode = db.FindTaxCode(model.CompanyId, model.TaxCode);
                if (taxCode != null &&                 // Record was found
                    ((taxCode.Id == 0 ||               // when creating new or
                      taxCode.Id != model.Id))) {      // when updating existing
                    error.SetError(EvolutionResources.errItemAlreadyExists, "TaxCode");
                }
            }

            return error;
        }

        #endregion
    }
}
