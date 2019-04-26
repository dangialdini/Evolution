using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using Evolution.DAL;
using Evolution.Models.Models;
using Evolution.CommonService;
using Evolution.Extensions;
using Evolution.Resources;
using Evolution.EncryptionService;

namespace Evolution.CustomerService {
    public partial class CustomerService {

        #region Public members    

        public List<ListItemModel> FindCreditCardsListItemModel(CompanyModel company, int customerId, bool bObscure = true) {
            var model = new List<ListItemModel>();

            foreach (var item in db.FindCreditCards(company.Id, customerId)) {
                string cardNo = RFC2898.Decrypt(item.CreditCardNo, Password);

                var newItem = new ListItemModel {
                    Id = item.Id.ToString(),
                    Text = cardNo.ObscureString(bObscure),
                    ImageURL = ""
                };
                model.Add(newItem);
            }
            return model;
        }

        public CreditCardListModel FindCreditCardsListModel(int companyId, int customerId, int index, int pageNo, int pageSize) {
            var model = new CreditCardListModel();

            // Do a case-insensitive search
            model.GridIndex = index;
            var allItems = db.FindCreditCards(companyId, customerId);

            model.TotalRecords = allItems.Count();
            foreach (var item in allItems.Skip((pageNo - 1) * pageSize)
                                        .Take(pageSize)) {
                model.Items.Add(MapToModel(item, true));
            }
            return model;
        }

        public CreditCardModel MapToModel(CreditCard item, bool bObscure) {
            var newItem = Mapper.Map<CreditCard, CreditCardModel>(item);

            if(item.CreditCardProvider != null) {
                newItem.CardProviderName = item.CreditCardProvider.ProviderName;
                newItem.CardProviderLogo = "/Content/CreditCards/" + item.CreditCardProvider.IconFile;

            } else {
                newItem.CardProviderName = "";
                newItem.CardProviderLogo = "";
            }

            newItem.CreditCardNo = RFC2898.Decrypt(newItem.CreditCardNo, Password).ObscureString(bObscure);
            newItem.NameOnCard = RFC2898.Decrypt(newItem.NameOnCard, Password);
            newItem.Expiry = RFC2898.Decrypt(newItem.Expiry, Password);
            newItem.CCV = RFC2898.Decrypt(newItem.CCV, Password);
            return newItem;
        }

        public void MapToEntity(CreditCardModel model, CreditCard entity) {
            Mapper.Map<CreditCardModel, CreditCard>(model, entity);
            entity.CreditCardNo = RFC2898.Encrypt(model.CreditCardNo, Password);
            entity.NameOnCard = RFC2898.Encrypt(model.NameOnCard, Password);
            entity.Expiry = RFC2898.Encrypt(model.Expiry, Password);
            entity.CCV = RFC2898.Encrypt(model.CCV, Password);
        }

        public CreditCardModel FindCreditCardModel(int id, CompanyModel company, CustomerModel customer, bool bCreateEmptyIfNotfound = true) {
            return FindCreditCardModel(id, company, customer.Id, bCreateEmptyIfNotfound);
        }

        public CreditCardModel FindCreditCardModel(int id, CompanyModel company, int customerId, bool bCreateEmptyIfNotfound = true) {
            CreditCardModel model = null;

            var a = db.FindCreditCard(id);
            if (a == null) {
                if (bCreateEmptyIfNotfound) model = new CreditCardModel { CompanyId = company.Id, CustomerId = customerId };
            } else {
                model = MapToModel(a, false);
            }

            return model;
        }

        public Error InsertOrUpdateCreditCard(CreditCardModel card, string lockGuid) {
            var error = validateCreditCardModel(card);
            if (!error.IsError) {
                // Check that the lock is still current
                if (!db.IsLockStillValid(typeof(CreditCard).ToString(), card.Id, lockGuid)) {
                    error.SetError(EvolutionResources.errRecordChangedByAnotherUser, "CreditCardNo");

                } else {
                    CreditCard temp = null;
                    if (card.Id != 0) temp = db.FindCreditCard(card.Id);
                    if (temp == null) temp = new CreditCard();

                    MapToEntity(card, temp);

                    db.InsertOrUpdateCreditCard(temp);
                    card.Id = temp.Id;
                }
            }
            return error;
        }

        public void DeleteCreditCard(int id) {
            db.DeleteCreditCard(id);
        }

        public string LockCreditCard(CreditCardModel model) {
            return db.LockRecord(typeof(CreditCard).ToString(), model.Id);
        }

        private Error validateCreditCardModel(CreditCardModel model) {
            var error = isValidRequiredString(getFieldValue(model.CreditCardNo), 16, "CreditCardNo", EvolutionResources.errCreditCardNoRequiredInField);
            if (!error.IsError) error = isValidRequiredString(getFieldValue(model.NameOnCard), 42, "NameOnCard", EvolutionResources.errTextDataRequiredInField);
            if (!error.IsError) error = isValidRequiredString(getFieldValue(model.Expiry), 5, "Expiry", EvolutionResources.errInvalidExpiryInField);
            if (!error.IsError) error = isValidRequiredInt(getFieldValue(model.CCV).ParseInt(), "CCV", EvolutionResources.errNumericDataRequiredInField);
            if (!error.IsError) error = isValidNonRequiredString(getFieldValue(model.Notes), 100, "Notes", EvolutionResources.errTextDataRequiredInField);

            return error;
        }

        #endregion

        #region Private methods

        private string _password = null;

        private string Password {
            get {
                if (_password == null) {
                    string encryptedPw = GetConfigurationSetting("EncryptionPW", "");
                    _password = RFC2898.Decrypt(encryptedPw, "Evolution");
                }
                return _password;
            }
        }

        #endregion
    }
}
