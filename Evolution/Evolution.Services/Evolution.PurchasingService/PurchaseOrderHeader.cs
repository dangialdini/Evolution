using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Evolution.DAL;
using Evolution.Models.Models;
using Evolution.CommonService;
using Evolution.AuditService;
using Evolution.Enumerations;
using Evolution.Resources;
using AutoMapper;

namespace Evolution.PurchasingService {
    public partial class PurchasingService {

        #region Public methods

        public PurchaseOrderHeaderListModel FindPurchaseOrderHeadersListModel(int companyId, int index, int pageNo, int pageSize, string search,
                                                                              int poStatusId, int warehouseId, int brandCategoryId,
                                                                              string sortColumn = "", SortOrder sortOrder = SortOrder.Asc) {
            var model = new PurchaseOrderHeaderListModel();

            decimal searchNum = -1;
            decimal.TryParse(search, out searchNum);

            // Do a case-insensitive search
            model.GridIndex = index;
            var allItems = db.FindPurchaseOrderHeaders(companyId, sortColumn, sortOrder)
                             .Where(p => (poStatusId == 0 || p.POStatus == poStatusId) &&
                                         (warehouseId == 0 || p.LocationId == warehouseId) &&
                                         (brandCategoryId == 0 || p.BrandCategoryId == brandCategoryId) &&
                                         (string.IsNullOrEmpty(search) || 
                                            ((p.Supplier != null && p.Supplier.Name.Contains(search)) ||
                                             (p.SupplierInv != null && p.SupplierInv.Contains(search)) ||
                                             (p.User_SalesPerson != null && ((p.User_SalesPerson.FirstName + " " + p.User_SalesPerson.LastName).Trim().Contains(search)) ||
                                             (p.OrderNumber == searchNum)))));

            model.TotalRecords = allItems.Count();
            foreach (var item in allItems.Skip((pageNo - 1) * pageSize)
                                         .Take(pageSize)) {
                var newItem = MapToModel(item);
                model.Items.Add(newItem);
            }

            return model;
        }

        public PurchaseOrderHeaderListModel FindPurchaseOrderHeaderSummaryListModel(CompanyModel company,
                                                                                    UserModel user,
                                                                                    int index,
                                                                                    int pageNo,
                                                                                    int pageSize,
                                                                                    string search) {
            // This is the similar to FindPurchaseOrderHeadersListModel above except that it
            // only shows active purchases for the parameter user instead of all
            // purchases for all users
            var model = new PurchaseOrderHeaderListModel();

            model.GridIndex = index;
            var allItems = db.FindPurchaseOrderHeaders(company.Id)
                                  .Where(p => p.PurchaseOrderHeaderStatu.StatusValue != (int)PurchaseOrderStatus.Closed &&
                                              p.PurchaseOrderHeaderStatu.StatusValue != (int)PurchaseOrderStatus.Cancelled &&
                                              p.SalespersonId == user.Id &&
                                              ((string.IsNullOrEmpty(search) || (!string.IsNullOrEmpty(search) && p.Supplier != null && p.Supplier.Name.Contains(search)) ||
                                               (string.IsNullOrEmpty(search) || (!string.IsNullOrEmpty(search) && p.SupplierInv != null && p.SupplierInv.Contains(search))))));

            model.TotalRecords = allItems.Count();
            foreach (var item in allItems.Skip((pageNo - 1) * pageSize)
                                        .Take(pageSize)) {
                var newItem = MapToModel(item);
                model.Items.Add(newItem);
            }

            return model;
        }

        public PurchaseOrderHeaderModel MapToModel(PurchaseOrderHeader item) {
            PurchaseOrderHeaderModel newItem = null;
            if (item != null) {
                newItem = Mapper.Map<PurchaseOrderHeader, PurchaseOrderHeaderModel>(item);

                string poNumber = item.OrderNumber.ToString();
                if (poNumber.IndexOf(".") != -1) poNumber = poNumber.TrimEnd('0').TrimEnd('.');
                newItem.OrderNumberUrl = $"<a href=\"/Purchasing/EditPurchase/Edit?id={item.Id}\">{poNumber}</a>";

                if (item.Supplier != null) newItem.SupplierName = item.Supplier.Name;
                if (item.PurchaseOrderHeaderStatu != null) {
                    newItem.POStatusText = item.PurchaseOrderHeaderStatu.StatusName;
                    newItem.POStatusValue = (PurchaseOrderStatus)item.PurchaseOrderHeaderStatu.StatusValue;
                }

                newItem.SalesPersonName = db.MakeName(item.User_SalesPerson);
                newItem.Splitable = item.PurchaseOrderDetails.Count > 0;

                // Landing date
                if (newItem.OrderNumber != null) {
                    var company = CompanyService.FindCompanyModel(newItem.CompanyId);

                    var shipmentContent = item.ShipmentContents.FirstOrDefault();   // ShipmentService.FindShipmentContentByPONoModel(company, newItem.OrderNumber.Value);
                    if (shipmentContent != null && shipmentContent.ShipmentId != null) {
                        var shipment = ShipmentService.FindShipmentModel(shipmentContent.ShipmentId.Value, company);
                        newItem.LandingDate = shipment.DatePreAlertETA;
                    }
                }
            }
            return newItem;
        }

        PurchaseOrderHeaderModel mapToModel(PurchaseOrderHeaderModel item) {
            return Mapper.Map<PurchaseOrderHeaderModel, PurchaseOrderHeaderModel>(item);
        }

        public PurchaseOrderHeaderModel FindPurchaseOrderHeaderModel(int id, CompanyModel company, 
                                                                     bool bCreateEmptyIfNotfound = true) {
            PurchaseOrderHeaderModel model = null;

            var p = db.FindPurchaseOrderHeader(id);
            if (p == null) {
                if (bCreateEmptyIfNotfound) model = new PurchaseOrderHeaderModel {
                    CompanyId = company.Id,
                    OrderNumber = LookupService.GetNextSequenceNumber(company, SequenceNumberType.PurchaseOrderNumber)
                };

            } else {
                model = MapToModel(p);
            }

            return model;
        }

        public PurchaseOrderHeaderModel FindPurchaseOrderHeaderByPONumberModel(decimal poNumber, CompanyModel company) {
            PurchaseOrderHeaderModel model = null;

            var p = db.FindPurchaseOrderHeaders(company.Id)
                      .Where(poh => poh.OrderNumber == poNumber)
                      .FirstOrDefault();
            if (p != null) {
                model = MapToModel(p);
            }

            return model;
        }

        public string FindPurchaseOrderHeadersString(CompanyModel company, bool bInsertNone = false) {
            string rc = "";
            bool bFirst = true;

            foreach (var pohl in db.FindPurchaseOrderHeaders(company.Id)
                                   .Where(poh => poh.PurchaseOrderHeaderStatu.StatusValue != (int)PurchaseOrderStatus.Closed)) {
                if (bInsertNone && bFirst) rc = EvolutionResources.lblNone + "=0";
                if (!string.IsNullOrEmpty(rc)) rc += "|";
                rc += pohl.OrderNumber.ToString().TrimEnd('0').TrimEnd('.') + "=" + pohl.Id.ToString();
                bFirst = false;
            }

            return rc;
        }

        public PurchaseOrderHeaderListModel FindUndeliveredPurchaseOrders(CompanyModel company) {
            PurchaseOrderHeaderListModel model = new PurchaseOrderHeaderListModel();

            foreach (var poh in db.FindUndeliveredPurchaseOrderHeaders(company.Id)) {
                model.Items.Add(MapToModel(poh));
            }
            return model;
        }

        public decimal FindPurchaseOrderTotal(PurchaseOrderHeaderModel poh) {
            decimal rc = 0;
            foreach(var pod in FindPurchaseOrderDetailListModel(poh).Items) {
                rc += pod.UnitPriceExTax.Value * (decimal)pod.OrderQty.Value;
            }
            return rc;
        }

        public Error InsertOrUpdatePurchaseOrderHeader(PurchaseOrderHeaderModel poh, UserModel user, string lockGuid) {
            var error = validateModel(poh);
            if (!error.IsError) {
                // Check that the lock is still current
                if (!db.IsLockStillValid(typeof(PurchaseOrderHeader).ToString(), poh.Id, lockGuid)) {
                    error.SetError(EvolutionResources.errRecordChangedByAnotherUser, "OrderNumber");

                } else {
                    PurchaseOrderHeader temp = null;
                    if (poh.Id != 0) temp = db.FindPurchaseOrderHeader(poh.Id);
                    if (temp == null) temp = new PurchaseOrderHeader();

                    var before = Mapper.Map<PurchaseOrderHeader, PurchaseOrderHeader>(temp);

                    Mapper.Map<PurchaseOrderHeaderModel, PurchaseOrderHeader>(poh, temp);

                    // So objects are linked
                    temp.Company = db.FindCompany(poh.CompanyId);

                    db.InsertOrUpdatePurchaseOrderHeader(temp);
                    poh.Id = temp.Id;

                    logChanges(before, temp, user);

                    // If shanges have occured, we need to send an email to any sales person
                    // who has allocations reliant on the order
                    if (before.POStatus != temp.POStatus && temp.POStatus == (int)PurchaseOrderStatus.Cancelled) {
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

        public void DeletePurchaseOrderHeader(PurchaseOrderHeaderModel model) {
            db.DeletePurchaseOrderHeader(model.Id);
        }

        public void DeletePurchaseOrderHeader(int id) {
            db.DeletePurchaseOrderHeader(id);
        }

        public string LockPurchaseOrderHeader(PurchaseOrderHeaderModel model) {
            return db.LockRecord(typeof(PurchaseOrderHeader).ToString(), model.Id);
        }

        public int GetPurchaseCount(CompanyModel company) {
            return db.FindPurchaseOrderHeaders(company.Id)
                     .Count();
        }

        #endregion

        #region Private methods

        private Error validateModel(PurchaseOrderHeaderModel model) {
            var error = isValidNonRequiredString(getFieldValue(model.SupplierInv), 255, "SupplierInv", EvolutionResources.errTextDataRequiredInField);
            if (!error.IsError) error = isValidNonRequiredString(getFieldValue(model.ShipAddress1), 255, "ShipAddress1", EvolutionResources.errTextDataRequiredInField);
            if (!error.IsError) error = isValidNonRequiredString(getFieldValue(model.ShipAddress2), 255, "ShipAddress2", EvolutionResources.errTextDataRequiredInField);
            if (!error.IsError) error = isValidNonRequiredString(getFieldValue(model.ShipAddress3), 255, "ShipAddress3", EvolutionResources.errTextDataRequiredInField);
            if (!error.IsError) error = isValidNonRequiredString(getFieldValue(model.ShipAddress4), 255, "ShipAddress4", EvolutionResources.errTextDataRequiredInField);
            if (!error.IsError) error = isValidNonRequiredString(getFieldValue(model.OrderComment), 255, "OrderComment", EvolutionResources.errTextDataRequiredInField);
            if (!error.IsError) error = isValidNonRequiredString(getFieldValue(model.CancelMessage), 2048, "CancelMessage", EvolutionResources.errTextDataRequiredInField);
            if (!error.IsError) error = isValidNonRequiredString(getFieldValue(model.OrderConfirmationNo), 20, "OrderConfirmationNo", EvolutionResources.errTextDataRequiredInField);

            return error;
        }

        private void logChanges(PurchaseOrderHeader before, PurchaseOrderHeader after, UserModel user) {
            AuditService.LogChanges(typeof(PurchaseOrderHeader).ToString(), BusinessArea.PurchaseOrderDetails, user, before, after);
        }

        #endregion
    }
}
