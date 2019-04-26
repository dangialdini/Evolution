using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Evolution.CommonService;
using Evolution.DAL;
using Evolution.Models.Models;
using Evolution.MediaService;
using Evolution.Resources;
using Evolution.Enumerations;
using Evolution.MapperService;

namespace Evolution.CompanyService
{
    public class CompanyService : CommonService.CommonService {

        #region Construction

        public CompanyService(EvolutionEntities dbEntities) : base(dbEntities) {}

        #endregion

        #region Public members    

        public List<ListItemModel> FindCompaniesListItemModel() {
            return db.FindCompanies()
                     .Select(c => new ListItemModel {
                         Id = c.Id.ToString(),
                         Text = c.FriendlyName,
                         ImageURL = ""
                     })
                     .ToList();
        }

        public List<CompanyModel> FindCompaniesModel(bool bShowHidden = false) {
            List<CompanyModel> model = new List<CompanyModel>();

            foreach(var company in db.FindCompanies()
                                     .Where(c => bShowHidden == true ||
                                                 c.Enabled == true)) {
                model.Add(MapToModel(company));
            }
            return model;
        }

        public CompanyListModel FindCompaniesListModel(int index, int pageNo, int pageSize, string search, bool bShowHidden = false) {
            var model = new CompanyListModel();

            int numValue = 0;
            bool bGotNum = int.TryParse(search, out numValue);

            // Do a case-insensitive search
            model.GridIndex = index;
            var allItems = db.FindCompanies(bShowHidden)
                             .Where(c => string.IsNullOrEmpty(search) ||
                                         (c.CompanyName != null && c.CompanyName.ToLower().Contains(search.ToLower())) ||
                                         (c.FriendlyName != null && c.FriendlyName.ToLower().Contains(search.ToLower())) ||
                                         (c.CompanyAddress != null && c.CompanyAddress.ToLower().Contains(search.ToLower())) ||
                                         (c.PhoneNumber != null && c.PhoneNumber.ToLower().Contains(search.ToLower())) ||
                                         (c.FaxNumber != null && c.FaxNumber.ToLower().Contains(search.ToLower())) ||
                                         (c.Website != null && c.Website.ToLower().Contains(search.ToLower())) ||
                                         (c.ShippingAddress != null && c.ShippingAddress.ToLower().Contains(search.ToLower())));

            model.TotalRecords = allItems.Count();
            foreach(var item in allItems.Skip((pageNo - 1) * pageSize)
                                        .Take(pageSize)) {
                model.Items.Add(MapToModel(item));
            }
            return model;
        }

        public Company FindCompany(int id) {
            return db.FindCompany(id);
        }

        public CompanyModel FindCompanyFriendlyNameModel(string friendlyName) {
            CompanyModel model = null;
            var c = db.FindCompanies()
                      .Where(co => co.FriendlyName == friendlyName)
                      .FirstOrDefault();
            if (c != null) model = MapToModel(c);

            return model;
        }

        public CompanyModel FindCompanyModel(int id, bool bCreateEmptyIfNotfound = true) {
            CompanyModel model = null;

            var c = db.FindCompany(id);
            if (c == null) {
                if (bCreateEmptyIfNotfound) model = new CompanyModel();

            } else {
                model = MapToModel(c);
            }

            return model;
        }

        public CompanyModel FindParentCompanyModel() {
            CompanyModel model = null;

            var c = db.FindParentCompany();
            if (c != null) model = MapToModel(c);

            return model;
        }

        public CompanyModel MapToModel(Company item) {
            //var newItem = Mapper.Map<Company, CompanyModel>(item);
            var newItem = new CompanyModel();
            Mapper.Map(item, newItem);
            ApplyUnitsOfMeasure(newItem);
            return newItem;
        }

        public CompanyModel MapToModel(CompanyModel item) {
            //var newItem = Mapper.Map<CompanyModel, CompanyModel>(item);
            var newItem = new CompanyModel();
            Mapper.Map(item, newItem);
            ApplyUnitsOfMeasure(newItem);
            return newItem;
        }

        private void mapToEntity(CompanyModel model, Company entity) {
            //Mapper.Map<CompanyModel, Company>(model, entity);
            Mapper.Map(model, entity);
        }

        private void mapToEntity(Company entity1, Company entity2) {
            //Mapper.Map<Company, Company>(entity1, entity2);
            Mapper.Map(entity1, entity2);
        }

        private void ApplyUnitsOfMeasure(CompanyModel model) {
            if (model.UnitOfMeasure == UnitOfMeasure.Imperial) {
                model.LengthUnit = "in";
                model.LongLengthUnit = "ft";
                model.WeightUnit = "lb";
            } else {
                model.LengthUnit = "cm";
                model.LongLengthUnit = "m";
                model.WeightUnit = "kg";
            }
        }

        public Error InsertOrUpdateCompany(CompanyModel company, UserModel user, string lockGuid) {
            var error = validateModel(company);
            if (!error.IsError) {
                // Check that the lock is still current
                if (!db.IsLockStillValid(typeof(Company).ToString(), company.Id, lockGuid)) {
                    error.SetError(EvolutionResources.errRecordChangedByAnotherUser, "Name");

                } else {
                    Company temp = null;
                    if (company.Id != 0) temp = db.FindCompany(company.Id);
                    if (temp == null) temp = new Company();

                    var before = new Company();
                    mapToEntity(temp, before);

                    mapToEntity(company, temp);

                    db.InsertOrUpdateCompany(temp);
                    company.Id = temp.Id;

                    logChanges(before, temp, user);

                    if(temp.IsParentCompany) {
                        // Company is the parent company, so switch this setting off on all other companies
                        foreach(var nonParentCompany in db.FindCompanies()
                                                          .Where(c => c.Id != company.Id &&
                                                                      c.IsParentCompany == true)
                                                          .ToList()) {
                            CompanyModel model = new CompanyModel { Id = nonParentCompany.Id };

                            LockCompany(model);      // Forces other users to refresh as a result of this change

                            nonParentCompany.IsParentCompany = false;
                            db.InsertOrUpdateCompany(nonParentCompany);
                        }
                    }

                    // Create company folders
                    MediaService.MediaService mediaService = new MediaService.MediaService(db);
                    mediaService.CreateCompanyFolders(company.Id);
                }
            }
            return error;
        }

        public string LockCompany(CompanyModel model) {
            return db.LockRecord(typeof(Company).ToString(), model.Id);
        }

        #endregion

        #region Private methods

        private Error validateModel(CompanyModel model) {
            var error = isValidRequiredString(getFieldValue(model.CompanyName), 255, "CompanyName", EvolutionResources.errCompanyNameRequired);
            if (!error.IsError) error = isValidRequiredString(getFieldValue(model.FriendlyName), 32, "FriendlyName", EvolutionResources.errTextDataRequiredInField);
            if (!error.IsError) error = isValidNonRequiredString(getFieldValue(model.ABN), 16, "ABN", EvolutionResources.errTextDataRequiredInField);
            if (!error.IsError) error = isValidNonRequiredString(getFieldValue(model.CompanyAddress), 255, "CompanyAddress", EvolutionResources.errTextDataRequiredInField);
            if (!error.IsError) error = isValidNonRequiredString(getFieldValue(model.PhoneNumber), 15, "PhoneNumber", EvolutionResources.errTextDataRequiredInField);
            if (!error.IsError) error = isValidNonRequiredString(getFieldValue(model.FaxNumber), 15, "FaxNumber", EvolutionResources.errTextDataRequiredInField);
            if (!error.IsError) error = isValidNonRequiredString(getFieldValue(model.Website), 64, "Website", EvolutionResources.errTextDataRequiredInField);
            if (!error.IsError) error = isValidNonRequiredString(getFieldValue(model.ShippingAddress), 255, "ShippingAddress", EvolutionResources.errTextDataRequiredInField);
            if (!error.IsError) error = isValidNonRequiredString(getFieldValue(model.BankName), 64, "BankName", EvolutionResources.errTextDataRequiredInField);
            if (!error.IsError) error = isValidNonRequiredString(getFieldValue(model.AccountName), 30, "AccountName", EvolutionResources.errTextDataRequiredInField);
            if (!error.IsError) error = isValidNonRequiredString(getFieldValue(model.AccountNumber), 16, "AccountNumber", EvolutionResources.errTextDataRequiredInField);
            if (!error.IsError) error = isValidNonRequiredString(getFieldValue(model.Swift), 20, "Swift", EvolutionResources.errTextDataRequiredInField);
            if (!error.IsError) error = isValidNonRequiredString(getFieldValue(model.Branch), 50, "Branch", EvolutionResources.errTextDataRequiredInField);
            if (!error.IsError) error = isValidNonRequiredString(getFieldValue(model.BranchAddress), 255, "BranchAddress", EvolutionResources.errTextDataRequiredInField);
            if (!error.IsError) error = isValidNonRequiredString(getFieldValue(model.AccountBSB), 7, "AccountBSB", EvolutionResources.errTextDataRequiredInField);
            if (!error.IsError) error = isValidNonRequiredString(getFieldValue(model.StatementBody), 8192, "StatementBody", EvolutionResources.errTextDataRequiredInField);
            if (!error.IsError) error = isValidNonRequiredString(getFieldValue(model.CancelMessage), 8192, "CancelMessage", EvolutionResources.errTextDataRequiredInField);
            if (!error.IsError) error = isValidNonRequiredString(getFieldValue(model.RoutingNo), 100, "RoutingNo", EvolutionResources.errTextDataRequiredInField);
            if (!error.IsError) error = isValidRequiredString(getFieldValue(model.EmailAddressPurchasing), 100, "EmailAddressPurchasing", EvolutionResources.errTextDataRequiredInField);
            if (!error.IsError) error = isValidNonRequiredEMail(getFieldValue(model.EmailAddressSales), 100, "EmailAddressSales", EvolutionResources.errTextDataRequiredInField);
            if (!error.IsError) error = isValidNonRequiredEMail(getFieldValue(model.EmailAddressAccounts), 100, "EmailAddressAccounts", EvolutionResources.errTextDataRequiredInField);
            if (!error.IsError) error = isValidNonRequiredEMail(getFieldValue(model.EmailAddressBCC), 100, "EmailAddressBCC", EvolutionResources.errTextDataRequiredInField);
            if (!error.IsError) error = isValidNonRequiredString(getFieldValue(model.MarginLogo), 64, "MarginLogo", EvolutionResources.errTextDataRequiredInField);
            if (!error.IsError) error = isValidNonRequiredString(getFieldValue(model.FormLogo), 64, "FormLogo", EvolutionResources.errTextDataRequiredInField);
            if (!error.IsError) error = isValidRequiredString(getFieldValue(model.DateFormat), 10, "DateFormat", EvolutionResources.errTextDataRequiredInField);

            if (!error.IsError) {
                // Check if a record with the same name already exists
                var company = db.FindCompanies(true)
                                .Where(c => c.CompanyName == model.CompanyName ||
                                            c.FriendlyName == model.FriendlyName)
                                .FirstOrDefault(); ;
                if (company != null &&                 // Record was found
                    ((company.Id == 0 ||               // when creating new or
                      company.Id != model.Id))) {      // when updating existing
                    if (company.FriendlyName == model.FriendlyName) {
                        error.SetError(EvolutionResources.errCompanyAlreadyExists, "FriendlyName");
                    } else {
                        error.SetError(EvolutionResources.errCompanyAlreadyExists, "CompanyName");
                    }
                }
            }
            return error;
        }

        private void logChanges(Company before, Company after, UserModel user) {
            AuditService.LogChanges(typeof(Company).ToString(), BusinessArea.CompanyDetails, user, before, after);
        }

        #endregion
    }
}
