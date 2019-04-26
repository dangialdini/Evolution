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

        public List<ListItemModel> FindLocationListItemModel(CompanyModel company, bool bInsertNone = false, bool bShowHidden = false) {
            List<ListItemModel> model = db.FindLocations(company.Id, bShowHidden)
                                           .Select(l => new ListItemModel {
                                               Id = l.Id.ToString(),
                                               Text = l.LocationName,
                                               ImageURL = ""
                                           })
                                           .ToList();
            if (bInsertNone) model.Insert(0, new ListItemModel(EvolutionResources.lblNone, "0"));
            return model;
        }

        public LocationListModel FindLocationsListModel(int companyId, int index, int pageNo, int pageSize, string search){
            var model = new LocationListModel();

            // Do a case sensitive search
            model.GridIndex = index;
            var allItems = db.FindLocations(companyId, true)
                             .Where(l => l.CompanyId == companyId &&
                             (string.IsNullOrEmpty(search) ||
                             (l.LocationIdentification != null && l.LocationIdentification.ToLower().Contains(search.ToLower())) ||
                             (l.LocationName != null && l.LocationName.ToLower().Contains(search.ToLower())) ||
                             (l.Street != null && l.Street.ToLower().Contains(search.ToLower())) ||
                             (l.City != null && l.City.ToLower().Contains(search.ToLower())) ||
                             (l.State != null && l.State.ToLower().Contains(search.ToLower())) ||
                             (l.PostCode != null && l.PostCode.ToLower().Contains(search.ToLower())) ||
                             (l.Country != null && l.Country.ToLower().Contains(search.ToLower())) ||
                             (l.Contact != null && l.Contact.ToLower().Contains(search.ToLower())) ||
                             (l.ContactPhone != null && l.ContactPhone.ToLower().Contains(search.ToLower()))));

            model.TotalRecords = allItems.Count();
            foreach(var item in allItems.Skip((pageNo - 1) * pageSize)
                                        .Take(pageSize)) {
                model.Items.Add(MapToModel(item));
            }
            return model;
        }

        public LocationModel FindLocationModel(int id, bool bCreateEmptyIfNotfound = true) {
            LocationModel model = null;

            var location = db.FindLocation(id);
            if (location == null) {
                if (bCreateEmptyIfNotfound) model = new LocationModel();

            } else {
                model = MapToModel(location);
            }

            return model;
        }

        public LocationModel MapToModel(Location item) {
            var model = Mapper.Map<Location, LocationModel>(item);
            return model;
        }

        public LocationModel Clone(LocationModel item) {
            var model = Mapper.Map<LocationModel, LocationModel>(item);
            return model;
        }

        public Error InsertOrUpdateLocation(LocationModel location, UserModel user, string lockGuid) {
            var error = validateModel(location);
            if (!error.IsError) {
                // Check that the lock is still current
                if (!db.IsLockStillValid(typeof(Location).ToString(), location.Id, lockGuid)) {
                    error.SetError(EvolutionResources.errRecordChangedByAnotherUser, "Location");

                } else {
                    Location temp = null;
                    if (location.Id != 0) temp = db.FindLocation(location.Id);
                    if (temp == null) temp = new Location();

                    Mapper.Map<LocationModel, Location>(location, temp);

                    db.InsertOrUpdateLocation(temp);
                    location.Id = temp.Id;
                }
            }
            return error;
        }

        public void DeleteLocation(int id) {
            db.DeleteLocation(id);
        }

        public string LockLocation(LocationModel model) {
            return db.LockRecord(typeof(Location).ToString(), model.Id);
        }

        #endregion

        #region Private methods

        private Error validateModel(LocationModel model) {
            var error = isValidNonRequiredString(getFieldValue(model.CanBeSold), 1, "CanBeSold", EvolutionResources.errTextDataRequiredInField);
            if (!error.IsError) error = isValidNonRequiredString(getFieldValue(model.LocationIdentification), 10, "LocationIdentification", EvolutionResources.errTextDataRequiredInField);
            if (!error.IsError) error = isValidNonRequiredString(getFieldValue(model.LocationName), 30, "LocationName", EvolutionResources.errTextDataRequiredInField);
            if (!error.IsError) error = isValidNonRequiredString(getFieldValue(model.Street), 255, "Street", EvolutionResources.errTextDataRequiredInField);
            if (!error.IsError) error = isValidNonRequiredString(getFieldValue(model.City), 255, "City", EvolutionResources.errTextDataRequiredInField);
            if (!error.IsError) error = isValidNonRequiredString(getFieldValue(model.State), 255, "State", EvolutionResources.errTextDataRequiredInField);
            if (!error.IsError) error = isValidNonRequiredString(getFieldValue(model.PostCode), 11, "PostCode", EvolutionResources.errTextDataRequiredInField);
            if (!error.IsError) error = isValidNonRequiredString(getFieldValue(model.Country), 255, "Country", EvolutionResources.errTextDataRequiredInField);
            if (!error.IsError) error = isValidNonRequiredString(getFieldValue(model.Contact), 255, "Contact", EvolutionResources.errTextDataRequiredInField);
            if (!error.IsError) error = isValidNonRequiredString(getFieldValue(model.ContactPhone), 21, "ContactPhone", EvolutionResources.errTextDataRequiredInField);
            if (!error.IsError) error = isValidNonRequiredString(getFieldValue(model.Notes), 255, "Notes", EvolutionResources.errTextDataRequiredInField);

            if (!error.IsError) {
                // Check if a record with the same name already exists
                var carrier = db.FindLocation(model.CompanyId, model.LocationIdentification);
                if (carrier != null &&                 // Record was found
                    ((carrier.Id == 0 ||               // when creating new or
                      carrier.Id != model.Id))) {      // when updating existing
                    error.SetError(EvolutionResources.errItemAlreadyExists, "LocationIdentification");
                }
            }

            return error;
        }

        #endregion
    }
}
