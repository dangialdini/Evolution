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

        public List<ListItemModel> FindSupplierListItemModel(CompanyModel company, bool bShowHidden = false) {
            return db.FindSuppliers(company.Id, bShowHidden)
                     .Select(c => new ListItemModel {
                         Id = c.Id.ToString(),
                         Text = c.Name,
                         ImageURL = ""
                     })
                     .ToList();
        }

        public SupplierListModel FindSuppliersListModel(CompanyModel company,
                                                        int index = 0, 
                                                        int pageNo = 1, 
                                                        int pageSize = Int32.MaxValue, 
                                                        string search = "",
                                                        int countryId = 0) {
            var model = new SupplierListModel();

            // Do a case-insensitive search
            model.GridIndex = index;
            var allItems = db.FindSuppliers(company.Id, true)
                             .Where(s => (string.IsNullOrEmpty(search) ||
                                          (s.Name != null && s.Name.ToLower().Contains(search.ToLower())) ||
                                           s.ContactName.ToLower().Contains(search.ToLower()) ||
                                           s.Email.ToLower().Contains(search.ToLower())) &&
                                          (countryId == 0 || s.SupplierAddresses.Where(sa => sa.CountryId == countryId).Count() > 0));

            model.TotalRecords = allItems.Count();
            foreach (var item in allItems.Skip((pageNo - 1) * pageSize)
                                        .Take(pageSize)) {
                var supplier = MapToModel(item);
                var address = FindSupplierAddressModel(supplier.Id);
                if (address != null) {
                    supplier.Street = address.Street;
                    supplier.City = address.City;
                    supplier.State = address.State;
                    supplier.Postcode = address.Postcode;
                    supplier.CountryName = address.CountryName;
                }
                model.Items.Add(supplier);
            }
            return model;
        }

        public SupplierModel MapToModel(Supplier item) {
            var newItem = Mapper.Map<Supplier, SupplierModel>(item);
            return newItem;
        }

        public SupplierModel MapToModel(SupplierModel item) {
            var newItem = Mapper.Map<SupplierModel, SupplierModel>(item);
            return newItem;
        }

        public SupplierModel FindSupplierModel(int id, bool bCreateEmptyIfNotfound = true) {
            SupplierModel model = null;

            var c = db.FindSupplier(id);
            if (c == null) {
                if (bCreateEmptyIfNotfound) model = new SupplierModel();
            } else {
                model = MapToModel(c);
            }

            return model;
        }

        public SupplierModel FindSupplierModel(int id, CompanyModel company, bool bCreateEmptyIfNotfound = true) {
            SupplierModel model = null;

            var c = db.FindSupplier(id);
            if(c == null) {
                if (bCreateEmptyIfNotfound) model = new SupplierModel { CompanyId = company.Id };
            } else {
                model = MapToModel(c);
            }

            return model;
        }

        public SupplierModel FindSupplierModel(string supplierName, bool bCreateEmptyIfNotfound = true) {
            SupplierModel model = null;

            var c = db.FindSupplier(supplierName);
            if (c == null) {
                if (bCreateEmptyIfNotfound) model = new SupplierModel();
            } else {
                model = MapToModel(c);
            }

            return model;
        }

        public Error InsertOrUpdateSupplier(SupplierModel supplier, UserModel user, string lockGuid) {
            var error = ValidateSupplierModel(supplier);
            if (!error.IsError) {
                // Check that the lock is still current
                if (!db.IsLockStillValid(typeof(Supplier).ToString(), supplier.Id, lockGuid)) {
                    error.SetError(EvolutionResources.errRecordChangedByAnotherUser, "Name");

                } else {
                    Supplier temp = null;
                    if (supplier.Id != 0) temp = db.FindSupplier(supplier.Id);
                    if (temp == null) temp = new Supplier();

                    var before = Mapper.Map<Supplier, Supplier>(temp);

                    if (temp.Id == 0) {
                        supplier.CreatedById = user.Id;
                        supplier.CreatedDate = DateTimeOffset.Now;
                    }
                    Mapper.Map<SupplierModel, Supplier>(supplier, temp);

                    db.InsertOrUpdateSupplier(temp);
                    supplier.Id = temp.Id;

                    logChanges(before, temp, user);
                }
            }
            return error;
        }

        public Error DeleteSupplier(int id) {
            var error = new Error();

            int prodCount = db.FindProductsForSupplier(id, true)
                              .Count();
            if (prodCount > 0) {
                error.SetError(EvolutionResources.errCantDeleteSupplier, "", prodCount.ToString());
            } else {
                db.DeleteSupplier(id);
            }
            return error;
        }

        public string LockSupplier(SupplierModel model) {
            return db.LockRecord(typeof(Supplier).ToString(), model.Id);
        }

        public Error ValidateSupplierModel(SupplierModel model) {
            if (model.CreatedDate == null) model.CreatedDate = DateTime.Now;

            Error error = isValidRequiredString(getFieldValue(model.Name), 52, "Name", EvolutionResources.errSupplierNameMustBeEntered);
            if (!error.IsError) error = isValidNonRequiredString(getFieldValue(model.CancelMessage), 255, "CancelMessage", EvolutionResources.errTextDataRequiredInField);
            if (!error.IsError) error = isValidNonRequiredString(getFieldValue(model.Notes), 255, "Notes", EvolutionResources.errTextDataRequiredInField);
            if (!error.IsError) error = isValidNonRequiredString(getFieldValue(model.ContactName), 50, "ContactName", EvolutionResources.errTextDataRequiredInField);
            if (!error.IsError) error = isValidNonRequiredString(getFieldValue(model.Phone1), 25, "Phone1", EvolutionResources.errTextDataRequiredInField);
            if (!error.IsError) error = isValidNonRequiredString(getFieldValue(model.Phone2), 25, "Phone2", EvolutionResources.errTextDataRequiredInField);
            if (!error.IsError) error = isValidNonRequiredEMail(getFieldValue(model.Email), 255, "Email", EvolutionResources.errTextDataRequiredInField);
            if (!error.IsError) error = isValidNonRequiredString(getFieldValue(model.URL), 255, "URL", EvolutionResources.errTextDataRequiredInField);

            if(!error.IsError) {
                // Check if a supplier with the same name already exists
                var supplier = db.FindSupplier(model.Name);
                if (supplier != null &&                 // Supplier was found
                    ((model.Id == 0 ||                  // when creating a new supplier or
                      supplier.Id != model.Id))) {      // when updating an existing supplier
                    error.SetError(EvolutionResources.errSupplierAlreadyExists, "Name");
                }
            }

            return error;
        }

        #endregion

        #region Private members

        private void logChanges(Supplier before, Supplier after, UserModel user) {
            AuditService.LogChanges(typeof(Supplier).ToString(), BusinessArea.SupplierDetails, user, before, after);
        }

        #endregion
    }
}
