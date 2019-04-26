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

        public List<SalesOrderDetailModel> FindSalesOrderDetailListModel(CompanyModel company, SalesOrderHeaderModel soh) {
            var model = new List<SalesOrderDetailModel>();

            foreach(var item in db.FindSalesOrderDetails(company.Id, soh.Id)) {
                model.Add(MapToModel(item));
            }

            return model;
        }

        public SalesOrderDetailModel FindSalesOrderDetailModel(int id) {
            return MapToModel(db.FindSalesOrderDetail(id));
        }

        public SalesOrderDetailModel MapToModel(SalesOrderDetail item) {
            var newItem = Mapper.Map<SalesOrderDetail, SalesOrderDetailModel>(item);
            return newItem;
        }

        public SalesOrderDetailModel MapToModel(SalesOrderDetailModel item) {
            var newItem = Mapper.Map<SalesOrderDetailModel, SalesOrderDetailModel>(item);
            return newItem;
        }

        public Error InsertOrUpdateSalesOrderDetail(SalesOrderDetailModel sod, string lockGuid) {
            var error = validateModel(sod);
            if (!error.IsError) {
                // Check that the lock is still current
                if (!db.IsLockStillValid(typeof(SalesOrderDetailTemp).ToString(), sod.Id, lockGuid)) {
                    error.SetError(EvolutionResources.errRecordChangedByAnotherUser, "ProductName");

                } else {
                    SalesOrderDetail temp = null;
                    if (sod.Id != 0) {
                        // Existing record to be updated
                        temp = db.FindSalesOrderDetail(sod.Id);

                        // The following fields are not copied:
                        //      OriginalRowId
                        //      LineNumber
                        Mapper.Map<SalesOrderDetailModel, SalesOrderDetail>(sod, temp);
                        if (temp.LineNumber == null) temp.LineNumber = db.GetNextSalesOrderDetailLineNumber(temp.SalesOrderHeaderId, true);

                    } else {
                        // New record, so copy values

                        // The following fields are not copied:
                        //      OriginalRowId
                        //      LineNumber
                        temp = Mapper.Map<SalesOrderDetailModel, SalesOrderDetail>(sod);

                        temp.OriginalRowId = sod.OriginalRowId;
                        temp.LineNumber = db.GetNextSalesOrderDetailLineNumber(temp.SalesOrderHeaderId, true);
                    }

                    db.InsertOrUpdateSalesOrderDetail(temp);
                    sod.Id = temp.Id;
                }
            }
            return error;
        }

        #endregion

        #region Private methods

        private Error validateModel(SalesOrderDetailModel model) {
            var error = isValidRequiredInt(model.ProductId, "ProductName", EvolutionResources.errAValidItemMustBeSelected);
            if (!error.IsError) error = isValidNonRequiredString(getFieldValue(model.ProductDescription), 255, "ProductDescription", EvolutionResources.errTextDataRequiredInField);

            return error;
        }

        #endregion
    }
}
