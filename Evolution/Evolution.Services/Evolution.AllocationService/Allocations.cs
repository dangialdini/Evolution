using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Evolution.DAL;
using Evolution.Models.Models;
using Evolution.CommonService;
using Evolution.Extensions;
using Evolution.Resources;
using AutoMapper;

namespace Evolution.AllocationService {
    public partial class AllocationService : CommonService.CommonService {

        #region Public methods

        public AllocationResultListModel FindAllocationsListModel(int purchaseOrderHeaderId, int index, int pageNo, int pageSize, string search) {
            var model = new AllocationResultListModel();

            int searchInt = 0;
            int.TryParse(search, out searchInt);

            // Do a case-insensitive search - stored proc call
            model.GridIndex = index;
            var allItems = db.FindAllocations(purchaseOrderHeaderId)
                             .Where(a => string.IsNullOrEmpty(search) ||
                                         (a.CustomerName != null && a.CustomerName.Contains(search)) ||
                                         (a.ItemNumber != null && a.ItemNumber.Contains(search)) ||
                                         (a.ItemName != null && a.ItemName.Contains(search)) ||
                                         (a.OrderNumber == searchInt))
                             .ToList();

            model.TotalRecords = allItems.Count();
            foreach (var item in allItems.Skip((pageNo - 1) * pageSize)
                                        .Take(pageSize)) {
                var newItem = mapToModel(item);
                model.Items.Add(newItem);
            }

            return model;
        }

        /*
        public AllocationListModel FindAllocationsListModel(PurchaseOrderDetailModel pod) {
            AllocationListModel model = new AllocationListModel();
            foreach (var item in db.FindAllocationsToPurchaseOrder(pod.PurchaseOrderHeaderId)
                                   .Where(a => a.PurchaseLineId == pod.Id)
                                   .OrderBy(a => a.DateCreated)) {
                var alloc = MapToModel(item);
                model.Items.Add(alloc);
            }
            return model;
        }
        */
        public AllocationListModel FindAllocationListModel(CompanyModel company, ProductModel product) {
            var model = new AllocationListModel();

            foreach (var item in db.FindAllocations(company.Id, product.Id)) {
                model.Items.Add(MapToModel(item));
            }

            return model;
        }

        public AllocationListModel FindAllocationsToPurchaseOrder(PurchaseOrderHeaderModel poh) {
            AllocationListModel model = new AllocationListModel();
            foreach (var item in db.FindAllocationsToPurchaseOrder(poh.Id)
                                   .OrderBy(a => a.PurchaseLineId)
                                   .ThenBy(a => a.DateCreated)) {
                var alloc = MapToModel(item);
                model.Items.Add(alloc);
            }
            return model;
        }

        public AllocationListModel FindAllocationsForSale(SalesOrderDetailModel sod) {
            AllocationListModel model = new AllocationListModel();
            foreach (var item in db.FindAllocationsForSalesOrderDetail(sod.CompanyId, sod.Id)
                                   .OrderBy(a => a.SaleLineId)
                                   .ThenBy(a => a.DateCreated)) {
                var alloc = MapToModel(item);
                model.Items.Add(alloc);
            }
            return model;
        }

        public bool IsAllocated(SalesOrderDetailModel sod) {
            return db.FindAllocationsForSalesOrderDetail(sod.CompanyId, sod.Id)
                     .Count() > 0;
        }

        public bool IsAllocatedToPurchaseOrder(SalesOrderDetailModel sod) {
            return db.FindAllocationsForSalesOrderDetail(sod.CompanyId, sod.Id)
                     .Where(a => a.PurchaseLineId != null)
                     .Count() > 0;
        }

        public string GetExpectedShipdate(SalesOrderDetailModel sod) {
            if(!IsAllocated(sod)) {
                // Not allocated
                return "TBA";

            } else {
                // It is allocated, but is it to stock or a purchase ?
                var allocs = db.FindAllocationsForSalesOrderDetail(sod.CompanyId, sod.Id)
                               .Where(a => a.PurchaseLineId == null)
                               .FirstOrDefault();
                if (allocs == null) {
                    // Allocated to stock
                    return "";

                } else {
                    // Allocated to a purchase
                    allocs = db.FindAllocationsForSalesOrderDetail(sod.CompanyId, sod.Id)
                                   .Where(a => a.PurchaseLineId != null &&
                                               a.PurchaseOrderDetail.PurchaseOrderHeader.RequiredDate != null)
                                   .FirstOrDefault();
                    if (allocs != null) {
                        return allocs.PurchaseOrderDetail.PurchaseOrderHeader.RequiredDate.ISODate();
                    } else {
                        return "TBA";
                    }
                }
            }
        }

        public AllocationSOListModel FindAvailabilityDetails(int index, int productId, int locationId, int brandCategoryId) {
            var model = new AllocationSOListModel();
            model.GridIndex = index;
            foreach (var item in db.FindAvailabilityDetails(productId, locationId, brandCategoryId)) {
                var newItem = Mapper.Map<FindAvailabilityDetails_Result, AllocationSOModel>(item);

                if (newItem.SalesPersonId > 0) {
                    var user = db.FindUser(newItem.SalesPersonId);
                    if (user != null) newItem.SalesPerson = (user.FirstName + " " + user.LastName).Trim();
                }
            }
            return model;
        }

        public AllocationPOListModel FindSaleDetails(int index, int productId, int locationId, int brandCategoryId) {
            var model = new AllocationPOListModel();
            model.GridIndex = index;
            foreach (var item in db.FindSaleDetails(productId, locationId, brandCategoryId)) {
                var newItem = Mapper.Map<FindSaleDetails_Result, AllocationPOModel> (item);
            }
            return model;
        }

        AllocationResultModel mapToModel(FindAllocations_Result item) {
            var newItem = Mapper.Map<FindAllocations_Result, AllocationResultModel>(item);
            return newItem;
        }

        public AllocationModel MapToModel(Allocation item) {
            var newItem = Mapper.Map<Allocation, AllocationModel>(item);
            return newItem;
        }

        public Error InsertOrUpdateAllocation(AllocationModel allocation, UserModel user, string lockGuid) {
            var error = validateModel(allocation);
            if (!error.IsError) {
                // Check that the lock is still current
                if (!db.IsLockStillValid(typeof(Allocation).ToString(), allocation.Id, lockGuid)) {
                    error.SetError(EvolutionResources.errRecordChangedByAnotherUser, "");

                } else {
                    Allocation temp = null;
                    if (allocation.Id != 0) temp = db.FindAllocation(allocation.Id);
                    if (temp == null) temp = new Allocation();

                    temp = Mapper.Map<AllocationModel, Allocation>(allocation);

                    db.InsertOrUpdateAllocation(temp);
                    allocation.Id = temp.Id;
                }
            }
            return error;
        }

        public void DeleteAllocation(int id) {
            db.DeleteAllocation(id);
        }

        public void DeleteAllocationsForSaleLine(CompanyModel company, int sodId) {
            db.DeleteAllocationsForSaleLine(company.Id, sodId);
        }

        public void DeleteAllocationsForPurchaseLine(CompanyModel company, int podId) {
            db.DeleteAllocationsForPurchaseLine(company.Id, podId);
        }

        public string LockAllocation(AllocationModel model) {
            return db.LockRecord(typeof(Allocation).ToString(), model.Id);
        }

        #endregion

        #region Private methods

        private Error validateModel(AllocationModel model) {
            var error = new Error();
            return error;
        }

        int getFreeStock(PurchaseOrderDetailModel pod, List<AllocationModel> allocList) {
            // The amount of 'free' stock on an order is the
            // OrderQty less the sum(Quantity) of all allocations pointing at the order detail line
            int allocated = allocList.Sum(al => al.Quantity.Value);

            return pod.OrderQty.Value - allocated;
        }

        #endregion
    }
}
