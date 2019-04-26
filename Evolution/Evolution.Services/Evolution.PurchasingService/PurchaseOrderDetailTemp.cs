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

        public PurchaseOrderDetailTempListModel FindPurchaseOrderDetailTempsListModel(int companyId, int purchaseOrderHeaderTempId,
                                                                                      int index, int pageNo, int pageSize, string search,
                                                                                      string sortColumn = "", SortOrder sortOrder = SortOrder.Asc) {
            var model = new PurchaseOrderDetailTempListModel();

            int searchInt = 0;
            int.TryParse(search, out searchInt);

            // Do a case-insensitive search
            model.GridIndex = index;
            var allItems = db.FindPurchaseOrderDetailTemps(companyId, purchaseOrderHeaderTempId, sortColumn, sortOrder)
                             .Where(p => string.IsNullOrEmpty(search) ||
                                         (p.ProductDescription != null && p.ProductDescription.Contains(search)) ||
                                         (p.LineNumber == searchInt));

            model.TotalRecords = allItems.Count();
            foreach (var item in allItems.Skip((pageNo - 1) * pageSize)
                                        .Take(pageSize)) {
                var newItem = MapToModel(item);
                model.Items.Add(newItem);
            }

            return model;
        }

        public PurchaseOrderDetailTempModel MapToModel(PurchaseOrderDetailTemp item) {
            var newItem = Mapper.Map<PurchaseOrderDetailTemp, PurchaseOrderDetailTempModel>(item);
            if (item.Product != null) {
                newItem.ProductCode = item.Product.ItemNumber;
                newItem.ProductName = item.Product.ItemNumber + " " + item.Product.ItemName;
                newItem.SupplierItemNumber = (item.Product.SupplierItemNumber == "0" ? "" : item.Product.SupplierItemNumber);
            }
            if (item.TaxCode != null) newItem.TaxCodeText = item.TaxCode.TaxCode1;
            //newItem.LineStatusText = (item.LOVItem == null ? "" : item.LOVItem.ItemText);
            if (newItem.UnitPriceExTax != null) newItem.LinePrice = newItem.UnitPriceExTax * newItem.OrderQty;
            newItem.UnitCBM = (item.Product != null ? item.Product.UnitCBM : 0);
            newItem.Allocated = db.FindPurchaseAllocationCount(newItem.OriginalRowId);
            return newItem;
        }

        public PurchaseOrderDetailTempModel MapToModel(PurchaseOrderDetailTempModel item) {
            var newItem = Mapper.Map<PurchaseOrderDetailTempModel, PurchaseOrderDetailTempModel>(item);
            return newItem;
        }

        public PurchaseOrderDetailTempModel FindPurchaseOrderDetailTempModel(int id, CompanyModel company, bool bCreateEmptyIfNotfound = true) {
            PurchaseOrderDetailTempModel model = null;

            var p = db.FindPurchaseOrderDetailTemp(id);
            if (p == null) {
                if (bCreateEmptyIfNotfound) model = new PurchaseOrderDetailTempModel { CompanyId = company.Id };

            } else {
                model = MapToModel(p);
            }

            return model;
        }

        public Error InsertOrUpdatePurchaseOrderDetailTemp(PurchaseOrderDetailTempModel podt, UserModel user, string lockGuid) {
            if(podt.ProductId == null) { 
                var product = db.FindProduct(podt.ProductName);
                if (product != null) podt.ProductId = product.Id;
            }
    
            var error = validateModel(podt);
            if (!error.IsError) {
                // Check that the lock is still current
                if (!db.IsLockStillValid(typeof(PurchaseOrderDetailTemp).ToString(), podt.Id, lockGuid)) {
                    error.SetError(EvolutionResources.errRecordChangedByAnotherUser, "ProductName");

                } else {
                    PurchaseOrderDetailTemp temp = null;
                    if (podt.Id != 0) {
                        // Existing record to be updated
                        temp = db.FindPurchaseOrderDetailTemp(podt.Id);

                        // The following fields are not copied:
                        //      OriginalRowId
                        //      LineNumber
                        Mapper.Map<PurchaseOrderDetailTempModel, PurchaseOrderDetailTemp>(podt, temp);
                        if(temp.LineNumber == null) temp.LineNumber = db.GetNextPurchaseOrderDetailLineNumber(temp.PurchaseOrderHeaderTempId, true);

                    } else {
                        // New record, so copy values

                        // The following fields are not copied:
                        //      OriginalRowId
                        //      LineNumber
                        temp = Mapper.Map<PurchaseOrderDetailTempModel, PurchaseOrderDetailTemp>(podt);

                        temp.OriginalRowId = podt.OriginalRowId;
                        temp.LineNumber = db.GetNextPurchaseOrderDetailLineNumber(temp.PurchaseOrderHeaderTempId, true);
                    }

                    temp.UserId = user.Id;

                    if (podt.UnitPriceExTax != null) {
                        podt.LinePrice = podt.UnitPriceExTax * podt.OrderQty;
                        if (podt.DiscountPercent != null && podt.DiscountPercent.Value != 0) {
                            podt.LinePrice = podt.LinePrice / 100 * (100 - podt.DiscountPercent);
                        }

                        var taxCode = db.FindTaxCode(podt.TaxCodeId);
                        if (taxCode != null) {
                            temp.UnitPriceExTax = podt.UnitPriceExTax.Value / 100 * taxCode.TaxPercentageRate;
                        }
                    }

                    db.InsertOrUpdatePurchaseOrderDetailTemp(temp);

                    if (podt.UnitPriceExTax != null) podt.LinePrice = podt.UnitPriceExTax * podt.OrderQty;
                    podt.Id = temp.Id;
                }
            }
            return error;
        }

        public void DeletePurchaseOrderDetailTemp(int id) {
            db.DeletePurchaseOrderDetailTemp(id);
        }

        public string LockPurchaseOrderDetailTemp(PurchaseOrderDetailTempModel model) {
            return db.LockRecord(typeof(PurchaseOrderDetailTemp).ToString(), model.Id);
        }

        #endregion

        #region Private methods

        private Error validateModel(PurchaseOrderDetailTempModel model) {
            var error = isValidRequiredInt(model.ProductId, "ProductName", EvolutionResources.errAValidItemMustBeSelected);
            if (!error.IsError) error = isValidNonRequiredString(getFieldValue(model.ProductDescription), 255, "ProductDescription", EvolutionResources.errTextDataRequiredInField);

            return error;
        }

        #endregion
    }
}
