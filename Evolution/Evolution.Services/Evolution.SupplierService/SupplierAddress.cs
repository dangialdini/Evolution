using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Evolution.DAL;
using Evolution.Models.Models;
using Evolution.CommonService;
using Evolution.Resources;
using Evolution.Enumerations;
using AutoMapper;

namespace Evolution.SupplierService {
    public partial class SupplierService : CommonService.CommonService {

        #region Public members

        public SupplierAddressListModel FindSupplierAddressesListModel(int supplierId, int index, int pageNo, int pageSize, string search) {
            var model = new SupplierAddressListModel();

            model.GridIndex = index;
            var allItems = db.FindSupplierAddresses(supplierId)
                             .Where(sa => string.IsNullOrEmpty(search) ||
                                          (sa.Street != null && sa.Street.ToLower().Contains(search.ToLower())) ||
                                          (sa.City != null && sa.City.ToLower().Contains(search.ToLower())) ||
                                          (sa.State != null && sa.State.ToLower().Contains(search.ToLower())) ||
                                          (sa.Postcode != null && sa.Postcode.ToLower().Contains(search.ToLower())) ||
                                          (sa.StreetLine1 != null && sa.StreetLine1.ToLower().Contains(search.ToLower())) ||
                                          (sa.StreetLine2 != null && sa.StreetLine2.ToLower().Contains(search.ToLower())) ||
                                          (sa.StreetLine3 != null && sa.StreetLine3.ToLower().Contains(search.ToLower())) ||
                                          (sa.StreetLine4 != null && sa.StreetLine4.ToLower().Contains(search.ToLower())))
                              .OrderByDescending(sa => sa.Street);

            model.TotalRecords = allItems.Count();
            foreach (var item in allItems.Skip((pageNo - 1) * pageSize)
                                        .Take(pageSize)) {
                var newItem = mapToModel(item);
                newItem.AddressTypeText = (item.LOVItem_AddressType == null ? "" : item.LOVItem_AddressType.ItemText);
                model.Items.Add(newItem);
            }
            return model;
        }

        SupplierAddressModel mapToModel(SupplierAddress item) {
            var newItem = Mapper.Map<SupplierAddress, SupplierAddressModel>(item);

            if (item.Country != null) newItem.CountryName = item.Country.CountryName;

            return newItem;
        }

        public SupplierAddressModel FindSupplierAddressModel(int supplierId, bool bCreateEmptyIfNotfound = true) {
            SupplierAddressModel model = null;

            var a = db.FindSupplierAddress(supplierId);
            if (a == null) {
                if (bCreateEmptyIfNotfound) model = new SupplierAddressModel { SupplierId = supplierId };
            } else {
                model = mapToModel(a);
            }

            return model;
        }

        public SupplierAddressModel FindSupplierAddressModel(int id, int supplierId, CompanyModel company, bool bCreateEmptyIfNotfound = true) {
            SupplierAddressModel model = null;

            var a = db.FindSupplierAddressById(id);
            if (a == null) {
                if (bCreateEmptyIfNotfound) {
                    model = new SupplierAddressModel { CompanyId = company.Id, SupplierId = supplierId };
                }
            } else {
                model = mapToModel(a);
            }

            return model;
        }

        public Error InsertOrUpdateSupplierAddress(SupplierAddressModel address, UserModel user) {
            // Supplier addresses are not locked as they are updated at the same time as a supplier 1:1
            Error error = ValidateAddressModel(address);
            if (!error.IsError) {
                SupplierAddress temp = null;
                if (address.Id != 0) temp = db.FindSupplierAddressById(address.Id);
                if (temp == null) temp = new SupplierAddress();

                var before = Mapper.Map<SupplierAddress, SupplierAddress>(temp);

                Mapper.Map<SupplierAddressModel, SupplierAddress>(address, temp);

                db.InsertOrUpdateSupplierAddress(temp);
                address.Id = temp.Id;

                logChanges(before, temp, user);
            }
            return error;
        }

        public Error InsertOrUpdateSupplierAddress(SupplierAddressModel address, string lockGuid = "") {
            var error = ValidateAddressModel(address);
            if(!error.IsError) {
                // Check that the lock is still current
                if (!db.IsLockStillValid(typeof(SupplierAddress).ToString(), address.Id, lockGuid)) {
                    error.SetError(EvolutionResources.errRecordChangedByAnotherUser, "Street");
                } else {
                    SupplierAddress temp = null;
                    if (address.Id != 0) temp = db.FindSupplierAddressById(address.Id);
                    if (temp == null) temp = new SupplierAddress();

                    mapToEntity(address, temp);

                    db.InsertOrUpdateSupplierAddress(temp);
                    address.Id = temp.Id;

                    db.Entry(temp).State = System.Data.Entity.EntityState.Detached;
                }
            }
            return error;
        }

        public void DeleteSupplierAddress(int id) {
            db.DeleteSupplierAddress(id);
        }

        public string LockSupplierAddress(SupplierAddressModel model) {
            return db.LockRecord(typeof(SupplierAddress).ToString(), model.Id);
        }

        public Error ValidateAddressModel(SupplierAddressModel model) {
            var error = isValidRequiredString(getFieldValue(model.Street), 255, "Street", EvolutionResources.errStreetRequired);
            if (!error.IsError) error = isValidRequiredString(getFieldValue(model.City), 50, "City", EvolutionResources.errCityRequired);
            if (!error.IsError) error = isValidRequiredString(getFieldValue(model.State), 20, "State", EvolutionResources.errStateRequired);
            if (!error.IsError) error = isValidRequiredString(getFieldValue(model.Postcode), 10, "Postcode", EvolutionResources.errPostCodeRequired);

            return error;
        }

        #endregion

        #region Private methods

        private void mapToEntity(SupplierAddressModel model, SupplierAddress entity) {
            Mapper.Map<SupplierAddressModel, SupplierAddress>(model, entity);
        }

        private void logChanges(SupplierAddress before, SupplierAddress after, UserModel user) {
            AuditService.LogChanges(typeof(SupplierAddress).ToString(), BusinessArea.SupplierAddress, user, before, after);
        }

        #endregion
    }
}
