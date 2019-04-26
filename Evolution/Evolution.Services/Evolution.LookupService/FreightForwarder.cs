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

        public List<ListItemModel> FindFreightForwardersListItemModel(CompanyModel company, bool bInsertNone = false, bool bShowHidden = false) {
            List<ListItemModel> model = db.FindFreightForwarders(company.Id, bShowHidden)
                                          .Select(ff => new ListItemModel {
                                             Id = ff.Id.ToString(),
                                             Text = ff.Name,
                                             ImageURL = ""
                                          })
                                          .ToList();
            if (bInsertNone) model.Insert(0, new ListItemModel(EvolutionResources.lblNone, "0"));
            return model;
        }

        public FreightForwarderListModel FindFreightForwardersListModel(int companyId, int index, int pageNo, int pageSize, string search) {
            var model = new FreightForwarderListModel();

            // Do a case-insensitive search
            model.GridIndex = index;
            var allItems = db.FindFreightForwarders(companyId, true)
                            .Where(ff => string.IsNullOrEmpty(search) ||
                                        (ff.Name != null && ff.Name.ToLower().Contains(search.ToLower())) ||
                                        (ff.Address != null && ff.Address.ToLower().Contains(search.ToLower())) ||
                                        (ff.Phone != null && ff.Phone.ToLower().Contains(search.ToLower())) ||
                                        (ff.Email != null && ff.Email.ToLower().Contains(search.ToLower())));

            model.TotalRecords = allItems.Count();
            foreach (var item in allItems.Skip((pageNo - 1) * pageSize)
                                         .Take(pageSize)) {
                var newItem = MapToModel(item);
                model.Items.Add(newItem);
            }

            return model;
        }

        public FreightForwarderModel FindFreightForwarderModel(int id, bool bCreateEmptyIfNotfound = true) {
            FreightForwarderModel model = null;

            var freightCarrier = db.FindFreightForwarder(id);
            if (freightCarrier == null) {
                if (bCreateEmptyIfNotfound) model = new FreightForwarderModel();

            } else {
                model = MapToModel(freightCarrier);
            }

            return model;
        }

        public FreightForwarderModel MapToModel(FreightForwarder item) {
            var model = Mapper.Map<FreightForwarder, FreightForwarderModel>(item);
            return model;
        }

        public FreightForwarderModel Clone(FreightForwarderModel item) {
            var model = Mapper.Map<FreightForwarderModel, FreightForwarderModel>(item);
            return model;
        }

        public Error InsertOrUpdateFreightForwarder(FreightForwarderModel forwarder, UserModel user, string lockGuid) {
            var error = validateModel(forwarder);
            if (!error.IsError) {
                // Check that the lock is still current
                if (!db.IsLockStillValid(typeof(FreightForwarder).ToString(), forwarder.Id, lockGuid)) {
                    error.SetError(EvolutionResources.errRecordChangedByAnotherUser, "Name");

                } else {
                    FreightForwarder temp = null;
                    if (forwarder.Id != 0) temp = db.FindFreightForwarder(forwarder.Id);
                    if (temp == null) temp = new FreightForwarder();

                    Mapper.Map<FreightForwarderModel, FreightForwarder>(forwarder, temp);

                    db.InsertOrUpdateFreightForwarder(temp);
                    forwarder.Id = temp.Id;
                }
            }
            return error;
        }

        public void DeleteFreightForwarder(int id) {
            db.DeleteFreightForwarder(id);
        }

        public string LockFreightForwarder(FreightForwarderModel model) {
            return db.LockRecord(typeof(FreightForwarder).ToString(), model.Id);
        }

        #endregion

        #region Private methods

        private Error validateModel(FreightForwarderModel model) {
            var error = isValidRequiredInt(model.CompanyId, "CompanyId", EvolutionResources.errModelFieldValueRequired);
            if (!error.IsError) error = isValidRequiredString(getFieldValue(model.Name), 50, "Name", EvolutionResources.errTextDataRequiredInField);
            if (!error.IsError) error = isValidNonRequiredString(getFieldValue(model.Address), 255, "Address", EvolutionResources.errTextDataRequiredInField);
            if (!error.IsError) error = isValidNonRequiredString(getFieldValue(model.Phone), 20, "Phone", EvolutionResources.errTextDataRequiredInField);
            if (!error.IsError) error = isValidNonRequiredString(getFieldValue(model.Email), 128, "Email", EvolutionResources.errTextDataRequiredInField);

            if (!error.IsError) {
                // Check if a record with the same name already exists
                var forwarder = db.FindFreightForwarder(model.CompanyId, model.Name);
                if (forwarder != null &&                 // Record was found
                    ((forwarder.Id == 0 ||               // when creating new or
                      forwarder.Id != model.Id))) {      // when updating existing
                    error.SetError(EvolutionResources.errItemAlreadyExists, "Name");
                }
            }

            return error;
        }
 
        #endregion
    }
}
