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

        public PurchaseOrderHeaderTempModel FindPurchaseOrderHeaderTempModel(int id, CompanyModel company, bool bCreateEmptyIfNotfound = true) {
            PurchaseOrderHeaderTempModel model = null;

            var p = db.FindPurchaseOrderHeaderTemp(id);
            if (p == null) {
                if (bCreateEmptyIfNotfound) model = new PurchaseOrderHeaderTempModel {
                    CompanyId = company.Id
                };

            } else {
                model = mapToModel(p);
            }

            return model;
        }

        PurchaseOrderHeaderTempModel mapToModel(PurchaseOrderHeaderTemp item) {
            var newItem = Mapper.Map<PurchaseOrderHeaderTemp, PurchaseOrderHeaderTempModel>(item);

            var poStatus = db.FindPurchaseOrderHeaderStatus(newItem.POStatus);
            if (poStatus != null) {
                newItem.POStatusText = poStatus.StatusName;
                newItem.POStatusValue = (PurchaseOrderStatus)poStatus.StatusValue;
            }
            newItem.SalesPersonName = db.MakeName(item.User_SalesPerson);
            newItem.Splitable = item.PurchaseOrderDetailTemps.Count() > 0;

            return newItem;
        }

        public PurchaseOrderSummaryModel CreateOrderSummary(PurchaseOrderHeaderTempModel poht,
                                                            string dateFormat) {
            var model = Mapper.Map<PurchaseOrderHeaderTempModel, PurchaseOrderSummaryModel>(poht);

            model.OrderNumber = poht.OrderNumber;

            model.TotalCbms = 0;
            model.AllocValueEx = 0;
            model.OrderValueEx = 0;
            model.AllocatedPercent = 0;
            model.Tax = 0;
            model.Total = 0;

            // The tax code comes from the supplier
            double taxRate = 0;
            Supplier supplier = null;
            if(poht.SupplierId != null) {
                supplier = db.FindSupplier(poht.SupplierId.Value);
                if(supplier.TaxCode != null) {
                    model.TaxCode = supplier.TaxCode.TaxCode1;
                    if(supplier.TaxCode.TaxPercentageRate != null) taxRate = (double)supplier.TaxCode.TaxPercentageRate.Value;
                }
            }

            // Now traverse all the items on the order
            foreach (var orderLine in FindPurchaseOrderDetailTempsListModel(poht.CompanyId, poht.Id, 0, 1, 9999, "").Items) {
                if(orderLine.OrderQty != null) model.TotalCbms += orderLine.UnitCBM * orderLine.OrderQty;
                //model.AllocValueEx +=

                double linePrice = 0;
                if (orderLine.UnitPriceExTax != null && orderLine.OrderQty != null) {
                    linePrice += (double)orderLine.UnitPriceExTax * (double)orderLine.OrderQty;
                }
                model.OrderValueEx += (double)orderLine.LinePrice;

                //model.AllocatedPercent +=

                double gstAmount = linePrice / 100 * taxRate;
                model.Tax += gstAmount;
            }
            model.Total = model.OrderValueEx + model.Tax;

            // Order status
            var poStatus = db.FindPurchaseOrderHeaderStatus(poht.POStatus);
            if (poStatus != null) {
                model.POStatusText = poStatus.StatusName;
            }

            // Landing date
            model.LandingDate = formatDate(poht.LandingDate, dateFormat);

            // Reallistic ETA
            model.RealisticRequiredDate = formatDate(poht.RealisticRequiredDate, dateFormat);

            // Adv US Final
            model.RequiredDate = formatDate(poht.RequiredDate, dateFormat);

            // Completed Date
            model.CompletedDate = formatDate(poht.CompletedDate, dateFormat);

            return model;
        }

        public Error InsertOrUpdatePurchaseOrderHeaderTemp(PurchaseOrderHeaderTempModel poht, UserModel user, string lockGuid) {
            var error = validateModel(poht);
            if (!error.IsError) {
                // Check that the lock is still current
                if (!db.IsLockStillValid(typeof(PurchaseOrderHeaderTemp).ToString(), poht.Id, lockGuid)) {
                    error.SetError(EvolutionResources.errRecordChangedByAnotherUser, "OrderNumber");

                } else {
                    PurchaseOrderHeaderTemp temp = null;
                    if (poht.Id != 0) temp = db.FindPurchaseOrderHeaderTemp(poht.Id);
                    if (temp == null) temp = new PurchaseOrderHeaderTemp();

                    Mapper.Map<PurchaseOrderHeaderTempModel, PurchaseOrderHeaderTemp>(poht, temp);

                    db.InsertOrUpdatePurchaseOrderHeaderTemp(temp);
                    poht.Id = temp.Id;
                }
            }
            return error;
        }

        public string LockPurchaseOrderHeaderTemp(PurchaseOrderHeaderTempModel model) {
            return db.LockRecord(typeof(PurchaseOrderHeaderTemp).ToString(), model.Id);
        }

        #endregion

        #region Private methods

        private Error validateModel(PurchaseOrderHeaderTempModel model) {
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

        #endregion
    }
}
