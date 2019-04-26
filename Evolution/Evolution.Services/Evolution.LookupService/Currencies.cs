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

        public string FindCurrencySymbol(int? id) {
            string rc = "";
            if (id != null) {
                var currency = db.FindCurrency(id.Value);
                if (currency != null) rc = currency.CurrencySymbol;
            }
            return rc;
        }

        public List<CurrencyModel> FindCurrenciesModel() {
            return db.FindCurrencies()
                     .Select(c => new CurrencyModel {
                         Id = c.Id,
                         CurrencyCode = c.CurrencyCode,
                         CurrencyName = c.CurrencyName,
                         ExchangeRate = c.ExchangeRate,
                         CurrencySymbol = (string.IsNullOrEmpty(c.CurrencySymbol) ? "" : c.CurrencySymbol),
                         FormatTemplate = (string.IsNullOrEmpty(c.FormatTemplate) ? "" : c.FormatTemplate),
                         Enabled = c.Enabled
                     })
                     .ToList();
        }

        public List<ListItemModel> FindCurrenciesListItemModel() {
            return db.FindCurrencies()
                     .Select(c => new ListItemModel {
                         Id = c.Id.ToString(),
                         Text = c.CurrencyName,
                         ImageURL = ""
                     })
                     .ToList();
        }

        public CurrencyListModel FindCurrenciesListModel(int index, int pageNo, int pageSize, string search) {
            var model = new CurrencyListModel();

            // Do a case-insensitive search
            model.GridIndex = index;
            var allItems = db.FindCurrencies(true)
                            .Where(c => string.IsNullOrEmpty(search) ||
                                        (c.CurrencyCode != null && c.CurrencyCode.ToLower().Contains(search.ToLower())) ||
                                        (c.CurrencyName != null && c.CurrencyName.ToLower().Contains(search.ToLower())) ||
                                        (c.CurrencySymbol != null && c.CurrencySymbol.ToLower().Contains(search.ToLower())));

            model.TotalRecords = allItems.Count();
            model.Items = allItems.Skip((pageNo - 1) * pageSize)
                                  .Take(pageSize)
                                  .Select(c => new CurrencyModel {
                                      Id = c.Id,
                                      CurrencyCode = c.CurrencyCode,
                                      CurrencyName = c.CurrencyName,
                                      ExchangeRate = c.ExchangeRate,
                                      CurrencySymbol = (string.IsNullOrEmpty(c.CurrencySymbol) ? "" : c.CurrencySymbol),
                                      FormatTemplate = (string.IsNullOrEmpty(c.FormatTemplate) ? "" : c.FormatTemplate),
                                      Enabled = c.Enabled
                                  }).ToList();
            return model;
        }

        public CurrencyModel FindCurrencyModel(int id, bool bCreateEmptyIfNotfound = true) {
            CurrencyModel model = null;

            var currency = db.FindCurrency(id);
            if (currency == null) {
                if(bCreateEmptyIfNotfound) model = new CurrencyModel();

            } else {
                model = MapToModel(currency);
            }

            return model;
        }

        public CurrencyModel MapToModel(Currency item) {
            var newItem = Mapper.Map<Currency, CurrencyModel>(item);
            return newItem;
        }

        public CurrencyModel Clone(CurrencyModel item) {
            var newItem = Mapper.Map<CurrencyModel, CurrencyModel>(item);
            return newItem;
        }

        public Error InsertOrUpdateCurrency(CurrencyModel currency, UserModel user, string lockGuid) {
            var error = validateModel(currency);
            if (!error.IsError) {
                // Check that the lock is still current
                if (!db.IsLockStillValid(typeof(Currency).ToString(), currency.Id, lockGuid)) {
                    error.SetError(EvolutionResources.errRecordChangedByAnotherUser, "CurrencyCode");

                } else {
                    Currency temp = null;
                    if (currency.Id != 0) temp = db.FindCurrency(currency.Id);
                    if (temp == null) temp = new Currency();

                    Mapper.Map<CurrencyModel, Currency>(currency, temp);

                    db.InsertOrUpdateCurrency(temp);
                    currency.Id = temp.Id;
                }
            }
            return error;
        }

        public void DeleteCurrency(int id) {
            db.DeleteCurrency(id);
        }

        public string LockCurrency(CurrencyModel model) {
            return db.LockRecord(typeof(Currency).ToString(), model.Id);
        }

        #endregion

        #region Private methods

        private Error validateModel(CurrencyModel model) {
            var error = isValidRequiredString(getFieldValue(model.CurrencyCode), 3, "CurrencyCode", EvolutionResources.errCurrencyCodeRequired);
            if (!error.IsError) error = isValidRequiredString(getFieldValue(model.CurrencyName), 30, "Currencyname", EvolutionResources.errCurrencyNameRequired);
            if (!error.IsError) error = isValidRequiredString(getFieldValue(model.CurrencySymbol), 4, "CurrencySymbol", EvolutionResources.errCurrencySymbolRequired);

            if (!error.IsError) {
                // Check if a record with the same code already exists
                var currency = db.FindCurrency(model.CurrencyCode);
                if (currency != null &&                 // Record was found
                    ((currency.Id == 0 ||               // when creating new or
                      currency.Id != model.Id))) {      // when updating existing
                    error.SetError(EvolutionResources.errItemAlreadyExists, "CurrencyCode");
                }
            }

            return error;
        }

        #endregion
    }
}
