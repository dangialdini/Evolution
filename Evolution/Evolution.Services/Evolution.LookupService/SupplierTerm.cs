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

        public List<ListItemModel> FindSupplierTermsListItemModel(CompanyModel company) {
            return db.FindSupplierTerms(company.Id)
                     .Select(c => new ListItemModel {
                         Id = c.Id.ToString(),
                         Text = c.SupplierTermName,
                         ImageURL = ""
                     })
                     .ToList();
        }

        public SupplierTermModel FindSupplierTermModel(int id, bool bCreateEmptyIfNotfound = true) {
            SupplierTermModel model = null;

            var term = db.FindSupplierTerm(id);
            if (term == null) {
                if (bCreateEmptyIfNotfound) model = new SupplierTermModel();

            } else {
                model = MapToModel(term);
            }

            return model;
        }

        public SupplierTermModel MapToModel(SupplierTerm term) {
            var model = Mapper.Map<SupplierTerm, SupplierTermModel>(term);
            return model;
        }

        public SupplierTermModel Clone(SupplierTermModel term) {
            var model = Mapper.Map<SupplierTermModel, SupplierTermModel>(term);
            return model;
        }

        public Error InsertOrUpdateSupplierTerm(SupplierTermModel term, UserModel user, string lockGuid) {
            var error = validateModel(term);
            if (!error.IsError) {
                // Check that the lock is still current
                if (!db.IsLockStillValid(typeof(SupplierTerm).ToString(), term.Id, lockGuid)) {
                    error.SetError(EvolutionResources.errRecordChangedByAnotherUser, "SupplierTermName");

                } else {
                    SupplierTerm temp = null;
                    if (term.Id != 0) temp = db.FindSupplierTerm(term.Id);
                    if (temp == null) temp = new SupplierTerm();

                    Mapper.Map<SupplierTermModel, SupplierTerm>(term, temp);

                    db.InsertOrUpdateSupplierTerm(temp);
                    term.Id = temp.Id;
                }
            }
            return error;
        }

        public void DeleteSupplierTerm(int id) {
            db.DeleteSupplierTerm(id);
        }

        public string LockSupplierTerm(SupplierTermModel model) {
            return db.LockRecord(typeof(SupplierTerm).ToString(), model.Id);
        }

        #endregion

        #region Private methods

        private Error validateModel(SupplierTermModel model) {
            var error = isValidRequiredInt(model.CompanyId, "CompanyId", EvolutionResources.errModelFieldValueRequired);
            if (!error.IsError) error = isValidRequiredString(getFieldValue(model.SupplierTermName), 50, "SupplierTermName", EvolutionResources.errTextDataRequiredInField);

            if (!error.IsError) {
                // Check if a record with the same name already exists
                var term = db.FindSupplierTerm(model.CompanyId, model.SupplierTermName);
                if (term != null &&                 // Record was found
                    ((term.Id == 0 ||               // when creating new or
                      term.Id != model.Id))) {      // when updating existing
                    error.SetError(EvolutionResources.errItemAlreadyExists, "SupplierTermName");
                }
            }

            return error;
        }

        #endregion
    }
}
