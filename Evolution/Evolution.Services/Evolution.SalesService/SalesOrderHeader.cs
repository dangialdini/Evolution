using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Evolution.DAL;
using Evolution.Models.Models;
using Evolution.CommonService;
using Evolution.Resources;
using Evolution.Enumerations;
using AutoMapper;

namespace Evolution.SalesService {
    public partial class SalesService {

        #region Public methods

        public SalesOrderHeaderListModel FindSalesOrderHeadersListModel(int companyId, int index, int pageNo, int pageSize, string search,
                                                                        int salesPersonId, int regionId, int countryId, int locationId, int soStatusId, int brandCategoryId,
                                                                        string sortColumn = "", SortOrder sortOrder = SortOrder.Asc) {
            var model = new SalesOrderHeaderListModel();

            int searchInt = -1;
            int.TryParse(search, out searchInt);

            // Do a case-insensitive search
            model.GridIndex = index;
            var allItems = db.FindSalesOrderHeaders(companyId, sortColumn, sortOrder)
                             .Where(s => (salesPersonId == 0 || s.SalespersonId == salesPersonId) &&
                                         (regionId == 0 || s.Customer.RegionId == regionId) &&
                                         (countryId == 0 || s.ShipCountryId == countryId) &&
                                         (locationId == 0 || s.LocationId == locationId) &&
                                         (soStatusId == 0 || s.SOStatus == soStatusId) &&
                                         (brandCategoryId == 0 || s.BrandCategoryId == brandCategoryId) &&
                                         ((string.IsNullOrEmpty(search) ||
                                          (s.Customer != null && (s.Customer.Name.Contains(search))) ||
                                          (s.CustPO != null && (s.CustPO.Contains(search))) ||
                                          (s.User_SalesPerson != null && ((s.User_SalesPerson.FirstName + " " + s.User_SalesPerson.LastName).Trim().Contains(search)))
                                         ) ||
                                         (searchInt == -1 || s.OrderNumber == searchInt)));

            model.TotalRecords = allItems.Count();
            foreach (var item in allItems.Skip((pageNo - 1) * pageSize)
                                        .Take(pageSize)) {
                model.Items.Add(MapToModel(item));
            }

            return model;
        }

        public SalesOrderHeaderListModel FindSalesOrderHeaderSummaryListModel(CompanyModel company, UserModel user,
                                                                              int index, int pageNo, int pageSize, string search) {
            // This is the similar to FindSalesOrderHeadersListModel above except that it
            // only shows active sales for the parameter user instead of all sales for all users
            var model = new SalesOrderHeaderListModel();

            int searchInt = -1;
            int.TryParse(search, out searchInt);

            // Do a case-insensitive search
            model.GridIndex = index;
            var allItems = db.FindSalesOrderHeaders(company.Id)
                             .Where(s => s.SalesOrderHeaderStatu.StatusValue != (int)SalesOrderHeaderStatus.Cancelled &&
                                         s.SalespersonId == user.Id &&
                                         ((string.IsNullOrEmpty(search) ||
                                          (s.Customer != null && (s.Customer.Name.Contains(search))) ||
                                          (s.CustPO != null && (s.CustPO.Contains(search))) ||
                                          (s.User_SalesPerson != null && ((s.User_SalesPerson.FirstName + " " + s.User_SalesPerson.LastName).Trim().Contains(search)))
                                         ) ||
                                         (searchInt == -1 || s.OrderNumber == searchInt)));

            model.TotalRecords = allItems.Count();
            foreach (var item in allItems.Skip((pageNo - 1) * pageSize)
                                        .Take(pageSize)) {
                var newItem = MapToModel(item);
                model.Items.Add(newItem);
            }

            return model;
        }

        public SalesOrderHeaderModel MapToModel(SalesOrderHeader item) {
            SalesOrderHeaderModel newItem = Mapper.Map<SalesOrderHeader, SalesOrderHeaderModel>(item);

            newItem.OrderNumberUrl = $"<a href=\"/Sales/Sales/Edit?id={item.Id}\">{item.OrderNumber}</a>";

            if (item.Customer != null) {
                newItem.CustomerName = item.Customer.Name;
                newItem.RegionText = item.Customer.Region.RegionName;
            }
            if (item.SalesOrderHeaderStatu != null) {
                newItem.SOStatusText = item.SalesOrderHeaderStatu.StatusName;
                newItem.SOStatusValue = (SalesOrderHeaderStatus)item.SalesOrderHeaderStatu.StatusValue;
            }
            if (item.SalesOrderHeaderSubStatu != null) {
                newItem.SOSubStatusText = item.SalesOrderHeaderSubStatu.StatusName;
            }

            newItem.SalesPersonName = db.MakeName(item.User_SalesPerson);

            if(item.SaleNextAction != null) newItem.NextActionText = item.SaleNextAction.NextActionDescription;
            if(item.BrandCategory != null) newItem.BrandCategoryText = item.BrandCategory.CategoryName;
            if(item.Country != null) newItem.CountryText = item.Country.CountryName;
            if(item.Location != null) newItem.LocationText = item.Location.LocationName;

            return newItem;
        }

        public SalesOrderHeaderModel FindSalesOrderHeaderModel(int id, CompanyModel company, bool bCreateEmptyIfNotfound = true, bool bLoadDetails = false) {
            SalesOrderHeaderModel model = null;

            var p = db.FindSalesOrderHeader(id);
            if (p == null) {
                if (bCreateEmptyIfNotfound) model = new SalesOrderHeaderModel {
                    CompanyId = company.Id,
                    OrderNumber = (int)LookupService.GetNextSequenceNumber(company, SequenceNumberType.SalesOrderNumber),
                    OrderDate = DateTimeOffset.Now,
                    ShipCountryId = company.DefaultCountryID,
                    LocationId = company.DefaultLocationID,
                    SOStatus = db.FindSalesOrderHeaderStatuses()
                                 .Where(sohs => sohs.StatusValue == (int)SalesOrderHeaderStatus.Quote)
                                 .FirstOrDefault()
                                 .Id,
                    NextActionId = db.FindSaleNextActions()
                                     .Where(sna => sna.Id == (int)Enumerations.SaleNextAction.None)
                                     .FirstOrDefault()
                                     .Id
                };

            } else {
                model = MapToModel(p);
            }
            if (model != null && bLoadDetails) model.SalesOrderDetails = FindSalesOrderDetailListModel(company, model);

            return model;
        }

        public SalesOrderHeaderModel FindSalesOrderHeaderModelFromTempId(int sohtId, CompanyModel company, bool bCreateEmptyIfNotfound = true) {
            var soht = db.FindSalesOrderHeaderTemp(sohtId);
            if(soht == null || soht.OriginalRowId == null) {
                return FindSalesOrderHeaderModel(0, company, bCreateEmptyIfNotfound);
            } else {
                return FindSalesOrderHeaderModel(soht.OriginalRowId.Value, company, bCreateEmptyIfNotfound);
            }
        }

        public SalesOrderHeaderListModel FindCreditCardSales(CompanyModel company, int cardId) {
            var model = new SalesOrderHeaderListModel();

            foreach (var item in db.FindSalesOrderHeaders(company.Id)
                                  .Where(soh => soh.CreditCardId == cardId)) {
                model.Items.Add(MapToModel(item));
            }

            return model;
        }

        public Error InsertOrUpdateSalesOrderHeader(SalesOrderHeaderModel soh, UserModel user, string lockGuid) {
            var error = validateModel(soh);
            if (!error.IsError) {
                // Check that the lock is still current
                if (!db.IsLockStillValid(typeof(SalesOrderHeader).ToString(), soh.Id, lockGuid)) {
                    error.SetError(EvolutionResources.errRecordChangedByAnotherUser, "OrderNumber");

                } else {
                    SalesOrderHeader temp = null;
                    if (soh.Id != 0) temp = db.FindSalesOrderHeader(soh.Id);
                    if (temp == null) temp = new SalesOrderHeader();

                    var before = Mapper.Map<SalesOrderHeader, SalesOrderHeader>(temp);

                    Mapper.Map<SalesOrderHeaderModel, SalesOrderHeader>(soh, temp);

                    db.InsertOrUpdateSalesOrderHeader(temp);
                    soh.Id = temp.Id;

                    logChanges(before, temp, user);

                    // If shanges have occured, we need to send an email to any sales person
                    // who has allocations reliant on the order
                    if (before.SOStatus != temp.SOStatus && temp.SOStatus == (int)SalesOrderHeaderStatus.Cancelled) {
                        // Order has been cancelled, so send a cancellation notification to everyone
                        /*
                    public int POStatus { set; get; } = 0;	// Cancelled
                    public DateTimeOffset? CancelDate { set; get; } = null;
                        
                        */

                    } else {
                        // Check the dates and email everyone of the changes
                        /*
                        public DateTimeOffset? RequiredDate { set; get; }
                        public DateTimeOffset? CompletedDate { set; get; }

                        public DateTimeOffset? RequiredShipDate { set; get; } = null;       // SRD Final
                        public DateTimeOffset? RealisticRequiredDate { set; get; } = null;  // Reallistic ETA

                        public DateTimeOffset? RequiredDate_Original { set; get; } = null;
                        public DateTimeOffset? DateOrderConfirmed { set; get; } = null;
                        public DateTimeOffset? RequiredShipDate_Original { set; get; } = null;  // SRD Initial
                        */
                    }
                }
            }
            return error;
        }

        public void DeleteSalesOrderHeader(SalesOrderHeaderModel model) {
            db.DeleteSalesOrderHeader(model.Id);
        }

        public void DeleteSalesOrderHeader(int id) {
            db.DeleteSalesOrderHeader(id);
        }

        public string LockSalesOrderHeader(SalesOrderHeaderModel model) {
            return db.LockRecord(typeof(SalesOrderHeader).ToString(), model.Id);
        }

        #endregion

        #region Private methods

        private Error validateModel(SalesOrderHeaderModel model) {
            var error = isValidNonRequiredString(getFieldValue(model.EndUserName), 52, "EndUserName", EvolutionResources.errTextDataRequiredInField);
            if (!error.IsError) error = isValidNonRequiredString(getFieldValue(model.ShipAddress1), 255, "ShipAddress1", EvolutionResources.errTextDataRequiredInField);
            if (!error.IsError) error = isValidNonRequiredString(getFieldValue(model.ShipAddress2), 255, "ShipAddress2", EvolutionResources.errTextDataRequiredInField);
            if (!error.IsError) error = isValidNonRequiredString(getFieldValue(model.ShipAddress3), 255, "ShipAddress3", EvolutionResources.errTextDataRequiredInField);
            if (!error.IsError) error = isValidNonRequiredString(getFieldValue(model.ShipAddress4), 255, "ShipAddress4", EvolutionResources.errTextDataRequiredInField);
            if (!error.IsError) error = isValidNonRequiredString(getFieldValue(model.ShipSuburb), 60, "ShipSuburb", EvolutionResources.errTextDataRequiredInField);
            if (!error.IsError) error = isValidNonRequiredString(getFieldValue(model.ShipState), 20, "ShipState", EvolutionResources.errTextDataRequiredInField);
            if (!error.IsError) error = isValidNonRequiredString(getFieldValue(model.ShipPostcode), 12, "ShipPostcode", EvolutionResources.errTextDataRequiredInField);
            if (!error.IsError) error = isValidNonRequiredString(getFieldValue(model.WarehouseInstructions), 100, "WarehouseInstructions", EvolutionResources.errTextDataRequiredInField);

            return error;
        }

        private void logChanges(SalesOrderHeader before, SalesOrderHeader after, UserModel user) {
            AuditService.LogChanges(typeof(SalesOrderHeader).ToString(), BusinessArea.SalesOrderDetails, user, before, after);
        }

        #endregion
    }
}
