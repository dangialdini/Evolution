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

        public PurchaseOrderDetailListModel FindPurchaseOrderDetailListModel(CompanyModel company,
                                                                             int pohId,
                                                                             int index, 
                                                                             int pageNo, 
                                                                             int pageSize) {
            var model = new PurchaseOrderDetailListModel();

            model.GridIndex = index;
            var allItems = db.FindPurchaseOrderDetails(company.Id, pohId);

            model.TotalRecords = allItems.Count();
            foreach (var item in allItems.Skip((pageNo - 1) * pageSize)
                                        .Take(pageSize)) {
                var newItem = MapToModel(item);
                model.Items.Add(newItem);
            }

            return model;
        }

        public PurchaseOrderDetailListModel FindPurchaseOrderDetailListModel(PurchaseOrderHeaderModel poh) {
            var model = new PurchaseOrderDetailListModel();

            var allItems = db.FindPurchaseOrderDetails(poh.CompanyId, poh.Id);

            model.TotalRecords = allItems.Count();
            foreach (var item in allItems) {
                var orderLine = MapToModel(item);
                model.Items.Add(orderLine);
            }
            return model;
        }

        public PurchaseOrderDetailModel FindPurchaseOrderDetailModel(int id) {
            PurchaseOrderDetailModel model = null;
            var item = db.FindPurchaseOrderDetail(id);
            if (item != null) model = MapToModel(item);
            return model;
        }

        public PurchaseOrderDetailModel MapToModel(PurchaseOrderDetail item) {
            var newItem = Mapper.Map<PurchaseOrderDetail, PurchaseOrderDetailModel>(item);
            return newItem;
        }

        public PurchaseOrderDetailModel MapToModel(PurchaseOrderDetailModel item) {
            var newItem = Mapper.Map<PurchaseOrderDetailModel, PurchaseOrderDetailModel>(item);
            return newItem;
        }

        public Error InsertOrUpdatePurchaseOrderDetail(PurchaseOrderDetailModel pod, UserModel user, string lockGuid) {
            var error = validateModel(pod);
            if (!error.IsError) {
                // Check that the lock is still current
                if (!db.IsLockStillValid(typeof(PurchaseOrderDetail).ToString(), pod.Id, lockGuid)) {
                    error.SetError(EvolutionResources.errRecordChangedByAnotherUser, "ProductName");

                } else {
                    PurchaseOrderDetail before = new PurchaseOrderDetail();

                    var temp = new PurchaseOrderDetail();
                    if (pod.Id != 0) {
                        // Updating existing record
                        temp = db.FindPurchaseOrderDetail(pod.Id);

                        Mapper.Map<PurchaseOrderDetail, PurchaseOrderDetail>(temp, before);

                        // The following does not copy:
                        //      OriginalRowId
                        //      LineNumber
                        Mapper.Map<PurchaseOrderDetailModel, PurchaseOrderDetail>(pod, temp);

                    } else {
                        // New record

                        // The following does not copy:
                        //      OriginalRowId
                        //      LineNumber
                        Mapper.Map<PurchaseOrderDetailModel, PurchaseOrderDetail>(pod, temp);

                        temp.OriginalRowId = pod.OriginalRowId;
                        temp.LineNumber = pod.LineNumber;
                    }

                    db.InsertOrUpdatePurchaseOrderDetail(temp);
                    pod.Id = temp.Id;

                    logChanges(before, temp, user);
                }
            }
            return error;
        }
        
        public void DeletePurchaseOrderDetail(PurchaseOrderDetailModel pod) {
            db.DeletePurchaseOrderDetail(pod.Id);
        }

        public string LockPurchaseOrderDetail(PurchaseOrderDetailModel model) {
            return db.LockRecord(typeof(PurchaseOrderDetail).ToString(), model.Id);
        }

        #endregion

        #region Private methods

            private Error validateModel(PurchaseOrderDetailModel model) {
            var error = new Error();
            return error;
        }

        private void logChanges(PurchaseOrderDetail before, PurchaseOrderDetail after, UserModel user) {
            AuditService.LogChanges(typeof(PurchaseOrderDetail).ToString(), BusinessArea.PurchaseOrderDetails, user, before, after);
        }

        #endregion
    }
}
