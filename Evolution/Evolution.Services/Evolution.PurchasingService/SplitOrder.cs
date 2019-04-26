using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using Evolution.DAL;
using Evolution.Models.Models;
using Evolution.CommonService;
using Evolution.AuditService;
using Evolution.Enumerations;
using Evolution.Extensions;
using Evolution.Resources;
using AutoMapper;

namespace Evolution.PurchasingService {
    public partial class PurchasingService {

        #region Public methods

        #region Split model retrieval

        public PurchaseDetailSplitListModel FindSplitPurchaseDetailsListModel(CompanyModel company,
                                                                              int pohId,
                                                                              int index,
                                                                              int pageNo,
                                                                              int pageSize) {
            var model = new PurchaseDetailSplitListModel();

            model.GridIndex = index;
            var allItems = db.FindPurchaseOrderDetails(company.Id, pohId);

            model.TotalRecords = allItems.Count();
            foreach (var item in allItems.Skip((pageNo - 1) * pageSize)
                                        .Take(pageSize)) {
                var newItem = MapToSplitModel(item);
                model.Items.Add(newItem);
            }

            return model;
        }

        public PurchaseOrderDetailSplitModel MapToSplitModel(PurchaseOrderDetail item) {
            var newItem = Mapper.Map<PurchaseOrderDetail, PurchaseOrderDetailSplitModel>(item);
            newItem.PurchaseOrderDetailId = item.Id;
            newItem.ItemNumber = item.Product.ItemNumber;
            newItem.OrigOrderQty = (item.OrderQty == null ? 0 : item.OrderQty.Value);
            newItem.RemainingQty = newItem.OrigOrderQty;
            return newItem;
        }

        #endregion

        #region Splitting process

        PurchaseOrderHeaderModel origPoh = null;                                // Original POH
        List<PurchaseOrderDetailModel> podList;

        PurchaseOrderHeaderModel updatedPoh = null;                             // New version of original POH
        PurchaseOrderDetailListModel updatedDetails = new PurchaseOrderDetailListModel();
        List<AllocationModel> updatedAllocations = new List<AllocationModel>();

        PurchaseOrderHeaderModel newPoh = null;                                 // New POH - when items moved to a new order
        int numItems = 0;

        public Error SplitOrder(CompanyModel company, SplitPurchaseModel model, UserModel user,
                                     string lockGuid,
                                     ref int updatedPOId, ref int newPOId) {
            var error = new Error();

            updatedPOId = 0;
            newPOId = 0;

            // Create a transaction as this involves a lot of changes which we
            // will need to roll back if there is a failure.
            // We don't create the transaction in test mode as the data is specifically created for the
            // test, so it doesn't matter if we mess it up. The key is that if there is a failure, we
            // can look at the database to see the state of the data at the point of failure without it
            // being relled back.
            DbContextTransaction trans = null;
            if (!db.IsTesting) trans = db.Database.BeginTransaction();
            try {
                // Validate the user splitting selections
                if (!error.IsError) error = validateSplit(company, model, user, lockGuid);

                // Get the original order into memory
                if (!error.IsError) error = copyOrder(company, model, user);

                // Split the order in memory
                if (!error.IsError) error = doSplit(company, model, user);

                // Recreate the order PDFs
                if (!error.IsError) error = recreatePurchaseOrderPdfs(model, company, user, updatedPoh, newPoh);

                // return the ids of the copy of the original order and a new order (if any)
                if (updatedPoh != null) updatedPOId = updatedPoh.Id;
                if (newPoh != null) newPOId = newPoh.Id;

                if(trans != null) trans.Commit();

            } catch (Exception e1) {
                if(trans != null) trans.Rollback();
                error.SetError(e1);
            }

            return error;
        }

        #endregion

        #region Private methods

        private Error validateSplit(CompanyModel company, SplitPurchaseModel model, UserModel user,
                                         string lockGuid) {
            var error = new Error();

            // Check that the lock is still current because we are about to change it by
            // moving detail lines from it
            if (!db.IsLockStillValid(typeof(PurchaseOrderHeader).ToString(), model.PurchaseOrderHeaderId, lockGuid)) {
                error.SetError(EvolutionResources.errRecordChangedByAnotherUser, "NewOrderAdvertisedETA");

            } else {
                podList = new List<PurchaseOrderDetailModel>();

                numItems = 0;
                origPoh = FindPurchaseOrderHeaderModel(model.PurchaseOrderHeaderId, company, false);

                foreach (var splitItem in model.SplitItems) {
                    // Count the items to be split/moved
                    numItems += splitItem.NewOrderQty + splitItem.TargetOrderQty;

                    // Get the items on the order
                    var pod = FindPurchaseOrderDetailModel(splitItem.PurchaseOrderDetailId);
                    if (pod == null) {
                        error.SetRecordError("PurchaseOrderDetail", splitItem.PurchaseOrderDetailId);
                        break;
                    } else if (splitItem.NewOrderQty + splitItem.TargetOrderQty > pod.OrderQty) {
                        if (splitItem.NewOrderQty > 0) {
                            error.SetError(EvolutionResources.errCantSplitMoreThanOriginalQty, $"txtSplitToNewOrderQty{splitItem.RowNumber}_4_0");
                        } else {
                            error.SetError(EvolutionResources.errCantSplitMoreThanOriginalQty, $"txtTargetOrderQty{splitItem.RowNumber}_5_0");
                        }
                        break;
                    } else {
                        // If we have a target order...
                        if (splitItem.TargetOrderId != 0) {
                            var tempPoh = db.FindPurchaseOrderHeader(splitItem.TargetOrderId);
                            if (tempPoh != null) {
                                // Found it, so check the base numbers (the value to the left of the decimal)
                                if ((int)origPoh.OrderNumber != (int)tempPoh.OrderNumber) {
                                    // Base is different, but is target a root order ?
                                    if ((int)tempPoh.OrderNumber != tempPoh.OrderNumber) {
                                        // Target order isn't root
                                        error.SetError(EvolutionResources.errYouCannotMoveItemsToNonRootOrder, $"ddlTargetOrder{splitItem.RowNumber}_6_0");
                                        break;
                                    } else {
                                        podList.Add(pod);
                                    }
                                } else {
                                    // Base is the same, so valid target order
                                    podList.Add(pod);
                                }

                            } else {
                                // Not found
                                error.SetError(EvolutionResources.errMustSelectValidOrderNumber, $"ddlTargetOrder{splitItem.RowNumber}_6_0");
                                break;
                            }

                        } else {
                            // No target order so make sure qty is 0 for target order
                            if (splitItem.TargetOrderQty > 0) {
                                error.SetError(EvolutionResources.errTargetQtyMustBeZeroWhenNoOrderSpecified, $"txtTargetOrderQty{splitItem.RowNumber}_6_0");
                                break;
                            } else {
                                podList.Add(pod);
                            }
                        }
                    }
                }
                if (!error.IsError && numItems == 0)
                    error.SetError(EvolutionResources.errNoItemsHaveBeenSpecifiedToMoveOrSplit, "");
            }
            return error;
        }

        private Error copyOrder(CompanyModel company, SplitPurchaseModel model, UserModel user) {
            var error = new Error();

            var updatedOrderNo = LookupService.GetNextSequenceNumber(company, SequenceNumberType.PurchaseOrderNumber, origPoh.OrderNumber.Value, true);

            var newPohId = db.CopyPurchaseOrder(model.PurchaseOrderHeaderId, updatedOrderNo, true).First().Value;
            updatedPoh = FindPurchaseOrderHeaderModel(newPohId, company, false);
            updatedDetails = FindPurchaseOrderDetailListModel(updatedPoh);

            updatedAllocations = AllocationService.FindAllocationsToPurchaseOrder(updatedPoh).Items;

            return error;
        }

        private Error doSplit(CompanyModel company, SplitPurchaseModel model, UserModel user) {
            var error = new Error();

            string  lgs;

            // Proceed to do the split
            for (int i = 0; i < podList.Count() && !error.IsError; i++) {
                var podListItem = podList[i];
                var splitItem = model.SplitItems[i];

                var updatedItem = updatedDetails.Items
                                                .Where(ud => ud.OriginalRowId == podListItem.Id)
                                                .FirstOrDefault();

                PurchaseOrderDetailModel pod = new PurchaseOrderDetailModel {
                    CompanyId = company.Id,
                    PurchaseOrderHeaderId = 0,
                    LineNumber = 0,
                    ProductId = podListItem.ProductId.Value,
                    ProductDescription = podListItem.ProductDescription,
                    UnitPriceExTax = podListItem.UnitPriceExTax,
                    TaxCodeId = podListItem.TaxCodeId,
                    DiscountPercent = podListItem.DiscountPercent,
                    //LineStatus = podListItem.LineStatus,
                    //IsReceived = podListItem.IsReceived
                };

                if (splitItem.NewOrderQty > 0) {
                    // Add a new line to a new order
                    if (newPoh == null) {
                        // Create the new order first
                        newPoh = mapToModel(updatedPoh);
                        newPoh.Id = 0;
                        newPoh.OrderNumber = LookupService.GetNextSequenceNumber(company, SequenceNumberType.PurchaseOrderNumber, 0, false);
                        error = InsertOrUpdatePurchaseOrderHeader(newPoh, user, "");
                    }
                    if (!error.IsError) {
                        pod.PurchaseOrderHeaderId = newPoh.Id;
                        pod.OrderQty = splitItem.NewOrderQty;
                        pod.LineNumber = db.GetNextPurchaseOrderDetailLineNumber(pod.PurchaseOrderHeaderId, false);
                        InsertOrUpdatePurchaseOrderDetail(pod, user, "");

                        // Move the # of split item allocations across to the new purchase order line
                        pod.OrderQty -= splitItem.NewOrderQty;
                        var allocList1 = updatedAllocations.Where(a => a.PurchaseLineId == pod.Id)
                                                           .ToList();
                        AllocationService.AllocateOnPurchaseOrderSplit(updatedItem, allocList1, newPoh, pod, splitItem.NewOrderQty, user);
                    }
                }

                if (!error.IsError && splitItem.TargetOrderQty > 0) {
                    // Add a new line to the target order
                    pod.Id = 0;
                    pod.PurchaseOrderHeaderId = splitItem.TargetOrderId;
                    pod.OrderQty = splitItem.TargetOrderQty;
                    pod.LineNumber = db.GetNextPurchaseOrderDetailLineNumber(pod.PurchaseOrderHeaderId, false);

                    error = InsertOrUpdatePurchaseOrderDetail(pod, user, "");

                    // Move the # of split item allocations across to the new purchase order line
                    pod.OrderQty -= splitItem.TargetOrderQty;
                    var allocList2 = updatedAllocations.Where(a => a.PurchaseLineId == pod.Id)
                                                       .ToList();
                    AllocationService.AllocateOnPurchaseOrderSplit(updatedItem, allocList2, newPoh, pod, splitItem.TargetOrderQty, user);
                }

                if (!error.IsError) {
                    InsertOrUpdatePurchaseOrderDetail(updatedItem, user, LockPurchaseOrderDetail(updatedItem));
                }
            }

            if (!error.IsError) {
                // Obsolete the existing order
                origPoh.POStatus = LookupService.FindPurchaseOrderHeaderStatusByValueModel(PurchaseOrderStatus.Superceded).Id;
                lgs = LockPurchaseOrderHeader(origPoh);
                error = InsertOrUpdatePurchaseOrderHeader(origPoh, user, lgs);
            }
            return error;
        }

        private Error recreatePurchaseOrderPdfs(SplitPurchaseModel model, 
                                                CompanyModel company,
                                                UserModel user,
                                                PurchaseOrderHeaderModel updatedOrder,
                                                PurchaseOrderHeaderModel newOrder) {
            var error = new Error();

            // Recreate purchase order PDFs

            // Newly created order (if any)
            if (newOrder != null) {
                error = createRevisedPurchaseOrder(newOrder.Id, company, user, "Purchase Order Created with items moved from Order " + model.OrderNumber.ToString());
            }

            // New Source order (Order split from)
            error = createRevisedPurchaseOrder(updatedOrder.Id, company, user, "Purchase Order Updated (due to items split out)");

            // Orders split to
            foreach (var targetOrderId in model.SplitItems
                                               .Where(si => si.TargetOrderId > 0)
                                               .Select(si => si.TargetOrderId)
                                               .Distinct()) {
                error = createRevisedPurchaseOrder(targetOrderId, company, user, "Purchase Order Updated - items moved from Order " + model.OrderNumber.ToString());
                if (error.IsError) break;
            }

            return error;
        }

        private Error createRevisedPurchaseOrder(int pohId, CompanyModel company, UserModel sender, string subject) {
            var error = new Error();

            var poh = db.FindPurchaseOrderHeader(pohId);

            string pdfFile = "";
            error = CreatePurchaseOrderPdf(poh, 
                                           company.POSupplierTemplateId, //DocumentTemplateType.PurchaseOrder, 
                                           null, 
                                           ref pdfFile);
            if (!error.IsError) {
                NoteService.NoteService noteService = new NoteService.NoteService(db);

                error = noteService.AttachNoteToPurchaseOrder(poh,
                                                              sender,
                                                              subject, "",
                                                              pdfFile.ToStringList(),
                                                              FileCopyType.Copy);
            }
            return error;
        }

        #endregion

        #endregion
    }
}
