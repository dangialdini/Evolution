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

        public List<ListItemModel> FindFreightCarriersListItemModel(CompanyModel company) {
            return FindFreightCarriersListItemModel(company.Id);
        }

        public List<ListItemModel> FindFreightCarriersListItemModel(int companyId) {
            return db.FindFreightCarriers(companyId)
                     .Select(c => new ListItemModel {
                         Id = c.Id.ToString(),
                         Text = c.FreightCarrier1,
                         ImageURL = ""
                     })
                     .ToList();
        }

        public FreightCarrierListModel FindFreightCarriersListModel(int companyId, int index = 0, int pageNo = 1, int pageSize = Int32.MaxValue, string search = "") {
            var model = new FreightCarrierListModel();

            // Do a case-insensitive search
            model.GridIndex = index;
            var allItems = db.FindFreightCarriers(companyId, true)
                            .Where(c => string.IsNullOrEmpty(search) ||
                                        (c.FreightCarrier1 != null && c.FreightCarrier1.ToLower().Contains(search.ToLower())) ||
                                        (c.HTTPPrefix != null && c.HTTPPrefix.ToLower().Contains(search.ToLower())));

            model.TotalRecords = allItems.Count();
            model.Items = allItems.Skip((pageNo - 1) * pageSize)
                                  .Take(pageSize)
                                  .Select(c => new FreightCarrierModel {
                                      Id = c.Id,
                                      CompanyId = companyId,
                                      FreightCarrier = c.FreightCarrier1,
                                      HTTPPrefix = (string.IsNullOrEmpty(c.HTTPPrefix) ? "" : c.HTTPPrefix),
                                      AutoBuildTrackingLink = c.AutoBuildTrackingLink,
                                      Enabled = c.Enabled
                                  }).ToList();
            return model;
        }

        public FreightCarrierModel FindFreightCarrierModel(int id, bool bCreateEmptyIfNotfound = true) {
            FreightCarrierModel model = null;

            var freightCarrier = db.FindFreightCarrier(id);
            if (freightCarrier == null) {
                if(bCreateEmptyIfNotfound) model = new FreightCarrierModel();

            } else {
                model = MapToModel(freightCarrier);
            }

            return model;
        }

        public FreightCarrierModel MapToModel(FreightCarrier item) {
            // The mapping config handles FreightCarrier1 => FreightCarrier
            var model = Mapper.Map<FreightCarrier, FreightCarrierModel>(item);
            return model;
        }

        public FreightCarrierModel Clone(FreightCarrierModel item) {
            var model = Mapper.Map<FreightCarrierModel, FreightCarrierModel>(item);
            return model;
        }

        private void mapToEntity(FreightCarrierModel model, FreightCarrier entity) {
            // The mapping config handles FreightCarrier => FreightCarrier1
            Mapper.Map<FreightCarrierModel, FreightCarrier>(model, entity);
        }

        public Error InsertOrUpdateFreightCarrier(FreightCarrierModel freightCarrier, UserModel user, string lockGuid) {
            var error = validateModel(freightCarrier);
            if (!error.IsError) {
                // Check that the lock is still current
                if (!db.IsLockStillValid(typeof(FreightCarrier).ToString(), freightCarrier.Id, lockGuid)) {
                    error.SetError(EvolutionResources.errRecordChangedByAnotherUser, "FreightCarrier");

                } else {
                    FreightCarrier temp = null;
                    if (freightCarrier.Id != 0) temp = db.FindFreightCarrier(freightCarrier.Id);
                    if (temp == null) temp = new FreightCarrier();

                    mapToEntity(freightCarrier, temp);

                    db.InsertOrUpdateFreightCarrier(temp);
                    freightCarrier.Id = temp.Id;
                }
            }
            return error;
        }

        public void DeleteFreightCarrier(int id) {
            db.DeleteFreightCarrier(id);
        }

        public string LockFreightCarrier(FreightCarrierModel model) {
            return db.LockRecord(typeof(FreightCarrier).ToString(), model.Id);
        }

        #endregion

        #region Private methods

        private Error validateModel(FreightCarrierModel model) {
            var error = isValidRequiredString(getFieldValue(model.FreightCarrier), 50, "FreightCarrier", EvolutionResources.errFreightCarrierNameRequired);
            if (!error.IsError) error = isValidNonRequiredString(getFieldValue(model.HTTPPrefix), 255, "HTTPPrefix", EvolutionResources.errTextDataRequiredInField);

            if (!error.IsError) {
                // Check if a record with the same name already exists
                var carrier = db.FindFreightCarrier(model.CompanyId, model.FreightCarrier);
                if (carrier != null &&                 // Record was found
                    ((carrier.Id == 0 ||               // when creating new or
                      carrier.Id != model.Id))) {      // when updating existing
                    error.SetError(EvolutionResources.errItemAlreadyExists, "FreightCarrier");
                }
            }

            return error;
        }

        #endregion
    }
}
