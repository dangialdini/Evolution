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

namespace Evolution.SalesService {
    public partial class SalesService {

        #region Public methods

        public SalesOrderDetailTempListModel FindSalesOrderDetailTempsListModel(int companyId, int salesOrderHeaderTempId,
                                                                                int index = 0, int pageNo = 1, int pageSize = int.MaxValue, string search = "",
                                                                                string sortColumn = "", SortOrder sortOrder = SortOrder.Asc) {
            var model = new SalesOrderDetailTempListModel();

            int searchInt = 0;
            int.TryParse(search, out searchInt);

            // Do a case-insensitive search
            model.GridIndex = index;
            var allItems = db.FindSalesOrderDetailTemps(companyId, salesOrderHeaderTempId, sortColumn, sortOrder)
                             .Where(p => string.IsNullOrEmpty(search) ||
                                         (p.ProductDescription != null && p.ProductDescription.Contains(search)) ||
                                         (p.LineNumber == searchInt));

            model.TotalRecords = allItems.Count();
            foreach (var item in allItems.Skip((pageNo - 1) * pageSize)
                                         .Take(pageSize)) {
                model.Items.Add(MapToModel(item));
            }

            return model;
        }

        public SalesOrderDetailTempModel MapToModel(SalesOrderDetailTemp item) {
            var newItem = Mapper.Map<SalesOrderDetailTemp, SalesOrderDetailTempModel>(item);

            if (item.Product != null) {
                newItem.ProductName = item.Product.ItemNumber + " " + item.Product.ItemName;
                newItem.ProductCode = item.Product.ItemNumber;

                if (!string.IsNullOrEmpty(item.Product.SupplierItemNumber)) {
                    newItem.SupplierItemNumber = (item.Product.SupplierItemNumber == "0" ? "" : item.Product.SupplierItemNumber);
                }
            }
            if(item.TaxCode != null) newItem.TaxCodeText = item.TaxCode.TaxCode1;
            newItem.LineStatusText = (item.SalesOrderLineStatu == null ? "" : item.SalesOrderLineStatu.StatusName);
            if (newItem.UnitPriceExTax != null && newItem.OrderQty != null) {
                newItem.LinePrice = newItem.UnitPriceExTax * newItem.OrderQty;
                if(newItem.DiscountPercent != null && newItem.DiscountPercent.Value != 0) {
                    newItem.LinePrice = newItem.LinePrice / 100 * (100 - newItem.DiscountPercent.Value);
                }
            }
            newItem.UnitCBM = (item.Product != null ? item.Product.UnitCBM : 0);
            //newItem.Allocated = db.FindSalesAllocationCount(newItem.OriginalRowId);
            return newItem;
        }

        public SalesOrderDetailTempModel MapToModel(SalesOrderDetailTempModel item) {
            var newItem = Mapper.Map<SalesOrderDetailTempModel, SalesOrderDetailTempModel>(item);
            return newItem;
        }

        public SalesOrderDetailTempModel FindSalesOrderDetailTempModel(int id, CompanyModel company, bool bCreateEmptyIfNotfound = true) {
            SalesOrderDetailTempModel model = null;

            var p = db.FindSalesOrderDetailTemp(id);
            if (p == null) {
                if (bCreateEmptyIfNotfound) model = new SalesOrderDetailTempModel { CompanyId = company.Id,
                                                                                    LineStatusId = db.FindSalesOrderHeaderSubStatus(SalesOrderHeaderSubStatus.Unpicked).Id };

            } else {
                model = MapToModel(p);
            }

            return model;
        }

        public Error InsertOrUpdateSalesOrderDetailTemp(SalesOrderDetailTempModel sodt, UserModel user, string lockGuid) {
            if (sodt.ProductId == null) {
                var product = db.FindProduct(sodt.ProductName);
                if (product != null) sodt.ProductId = product.Id;
            }

            var error = validateModel(sodt);
            if (!error.IsError) {
                // Check that the lock is still current
                if (!db.IsLockStillValid(typeof(SalesOrderDetailTemp).ToString(), sodt.Id, lockGuid)) {
                    error.SetError(EvolutionResources.errRecordChangedByAnotherUser, "ProductName");

                } else {
                    var now = DateTimeOffset.Now;
                    if (sodt.DateCreated == null) sodt.DateCreated = now;
                    sodt.DateModified = now;

                    SalesOrderDetailTemp temp = null;
                    if (sodt.Id != 0) {
                        // Existing record to be updated
                        temp = db.FindSalesOrderDetailTemp(sodt.Id);

                        // The following fields are not copied:
                        //      OriginalRowId
                        //      LineNumber
                        Mapper.Map<SalesOrderDetailTempModel, SalesOrderDetailTemp>(sodt, temp);
                        if (temp.LineNumber == null) temp.LineNumber = db.GetNextSalesOrderDetailLineNumber(temp.SalesOrderHeaderTempId, true);

                    } else {
                        // New record, so copy values

                        // The following fields are not copied:
                        //      OriginalRowId
                        //      LineNumber
                        temp = Mapper.Map<SalesOrderDetailTempModel, SalesOrderDetailTemp>(sodt);

                        temp.OriginalRowId = sodt.OriginalRowId;
                        temp.LineNumber = db.GetNextSalesOrderDetailLineNumber(temp.SalesOrderHeaderTempId, true);
                    }

                    temp.UserId = user.Id;

                    if (sodt.UnitPriceExTax != null) {
                        sodt.LinePrice = sodt.UnitPriceExTax * sodt.OrderQty;
                        if (sodt.DiscountPercent != null && sodt.DiscountPercent.Value != 0) {
                            sodt.LinePrice = sodt.LinePrice / 100 * (100 - sodt.DiscountPercent);
                        }
                    }

                    db.InsertOrUpdateSalesOrderDetailTemp(temp);
                    sodt.Id = temp.Id;
                }
            }
            return error;
        }

        public void DeleteSalesOrderDetailTemp(int id) {
            db.DeleteSalesOrderDetailTemp(id);
        }

        public string LockSalesOrderDetailTemp(SalesOrderDetailTempModel model) {
            return db.LockRecord(typeof(SalesOrderDetailTemp).ToString(), model.Id);
        }

        #endregion

        #region Private methods

        private Error validateModel(SalesOrderDetailTempModel model) {
            var error = isValidRequiredInt(model.ProductId, "ProductName", EvolutionResources.errAValidItemMustBeSelected);
            if (!error.IsError) error = isValidNonRequiredString(getFieldValue(model.ProductDescription), 255, "ProductDescription", EvolutionResources.errTextDataRequiredInField);

            return error;
        }

        #endregion
    }
}
