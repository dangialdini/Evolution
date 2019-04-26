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

        public List<CountryModel> FindCountriesModel() {
            return db.FindCountries()
                     .Select(c => new CountryModel {
                         Id = c.Id,
                         CountryName = c.CountryName,
                         ISO2Code = c.ISO2Code,
                         ISO3Code = c.ISO3Code,
                         UNCode = c.UNCode,
                         Enabled = c.Enabled
                     })
                     .ToList();
        }

        public List<ListItemModel> FindCountriesListItemModel() {
            return db.FindCountries()
                     .Select(c => new ListItemModel {
                         Id = c.Id.ToString(),
                         Text = c.CountryName,
                         ImageURL = ""
                     })
                     .ToList();
        }

        public CountryListModel FindCountriesListModel(int index = 0, int pageNo = 1, int pageSize = Int32.MaxValue, string search = "") {
            var model = new CountryListModel();

            int numValue = 0;
            bool bGotNum = int.TryParse(search, out numValue);

            // Do a case-insensitive search
            model.GridIndex = index;
            var allItems = db.FindCountries(true)
                            .Where(c => string.IsNullOrEmpty(search) ||
                                        (c.CountryName != null && c.CountryName.ToLower().Contains(search.ToLower())) ||
                                        (c.ISO2Code != null && c.ISO2Code.ToLower().Contains(search.ToLower())) ||
                                        (c.ISO3Code != null && c.ISO3Code.ToLower().Contains(search.ToLower())) ||
                                        (bGotNum && c.UNCode == numValue));

            model.TotalRecords = allItems.Count();
            model.Items = allItems.Skip((pageNo - 1) * pageSize)
                                  .Take(pageSize)
                                  .Select(c => new CountryModel {
                                      Id = c.Id,
                                      CountryName = c.CountryName,
                                      ISO2Code = c.ISO2Code,
                                      ISO3Code = c.ISO3Code,
                                      UNCode = c.UNCode,
                                      Enabled = c.Enabled
                                  }).ToList();
            return model;
        }

        public Country FindCountry(int id) {
            return db.FindCountry(id);
        }

        public CountryModel FindCountryModel(int id, bool bCreateEmptyIfNotfound = true) {
            CountryModel model = null;

            var country = db.FindCountry(id);
            if (country == null) {
                if(bCreateEmptyIfNotfound) model = new CountryModel();

            } else {
                model = MapToModel(country);
            }

            return model;
        }

        public CountryModel FindCountryModel(string countryName) {
            CountryModel model = null;

            var country = db.FindCountries(true)
                                .Where(c => (c.CountryName != null && c.CountryName.ToLower().Contains(countryName.ToLower())) ||
                                            (c.ISO2Code != null && c.ISO2Code.ToLower() == countryName.ToLower()) ||
                                            (c.ISO3Code != null && c.ISO3Code.ToLower() == countryName.ToLower()))
                                .FirstOrDefault();
            if(country != null)
                model = Mapper.Map<Country, CountryModel>(country);
            return model;
        }

        public CountryModel MapToModel(Country item) {
            var newItem = Mapper.Map<Country, CountryModel>(item);
            return newItem;
        }

        public CountryModel Clone(CountryModel item) {
            var newItem = Mapper.Map<CountryModel, CountryModel>(item);
            return newItem;
        }

        public Error InsertOrUpdateCountry(CountryModel country, UserModel user, string lockGuid) {
            var error = validateModel(country);
            if (!error.IsError) {
                // Check that the lock is still current
                if (!db.IsLockStillValid(typeof(Country).ToString(), country.Id, lockGuid)) {
                    error.SetError(EvolutionResources.errRecordChangedByAnotherUser, "CountryName");

                } else {
                    Country temp = null;
                    if (country.Id != 0) temp = db.FindCountry(country.Id);
                    if (temp == null) temp = new Country();

                    Mapper.Map<CountryModel, Country>(country, temp);

                    db.InsertOrUpdateCountry(temp);
                    country.Id = temp.Id;
                }
            }
            return error;
        }

        public void DeleteCountry(int id) {
            db.DeleteCountry(id);
        }

        public string LockCountry(CountryModel model) {
            return db.LockRecord(typeof(Country).ToString(), model.Id);
        }

        #endregion

        #region Private methods

        private Error validateModel(CountryModel model) {
            string countryName = getFieldValue(model.CountryName);

            var error = isValidRequiredString(countryName, 50, "CountryName", EvolutionResources.errCountryNameRequired);

            if (!error.IsError) {
                // Check if a record with the same name already exists
                var country = db.FindCountry(model.CountryName);
                if (country != null &&                 // Record was found
                    ((country.Id == 0 ||               // when creating new or
                      country.Id != model.Id))) {      // when updating existing
                    error.SetError(EvolutionResources.errItemAlreadyExists, "CountryName");
                }
            }

            return error;
        }

        #endregion
    }
}
