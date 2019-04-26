using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Evolution.DAL;
using Evolution.Models.Models;
using Evolution.CommonService;
using Evolution.Enumerations;
using Evolution.Resources;
using AutoMapper;

namespace Evolution.AllocationService {
    public partial class AllocationService {
        // The following method is called by the Purchasing Service when a Purchase Order is saved
        // It could also be called manually by the user for allocation optimisation
        public void AllocateOnPurchaseOrderSave(PurchaseOrderHeaderModel poh) {
            var pohStatus = db.FindPurchaseOrderHeaderStatus(poh.POStatus);
            if(pohStatus != null && pohStatus.AllowAllocation) {
                // The status of the purchase allows an allocation

                // Only adjust allocations on products in the order



            }
        }


        // The following method is called from the Purchasing service when a Purchase Order is being split
        public void AllocateOnPurchaseOrderSplit(PurchaseOrderDetailModel pod,            // Line we split from
                                                 List<AllocationModel> allocList,         // Allocations linked to the line (ordered by DateCreated)
                                                 PurchaseOrderHeaderModel targetPo,       // PO we are splitting to (the 'later' PO))
                                                 PurchaseOrderDetailModel targetPod,      // Line we are spliting to
                                                 int numSplitItems,                       // Number of items being split
                                                 UserModel user) {

            // On entry, the allocList must be in DateCreated order
            // Every allocation will always have a purchase order line Id and a sales order line Id

            // Update the purchase order detail
            pod.OrderQty -= numSplitItems;

            if (allocList.Count > 0) {
                // Is window open of the allocation greater than or equal to the
                // the ReallisticETA of the PO were are spliting to ?
                var sod = db.FindSalesOrderDetail(allocList[0].SaleLineId.Value);
                if (sod.SalesOrderHeader.DeliveryWindowOpen != null && targetPo.RealisticRequiredDate != null) {
                    // Only do something if the window and required date are set

                    if (sod.SalesOrderHeader.DeliveryWindowOpen >= targetPo.RealisticRequiredDate) {
                        // Yes, Is there free stock on the later PO ?
                        if (getFreeStock(targetPod, allocList) >= numSplitItems) {
                            // Yes, assign allocation to later PO
                            allocList[0].PurchaseLineId = targetPod.Id;

                        } else {
                            // No, assign allocation to earlier PO
                            allocList[0].PurchaseLineId = pod.Id;
                        }

                    } else {
                        // No, is there free stock on the earlier PO ?
                        if (getFreeStock(pod, allocList) >= numSplitItems) {
                            // Yes, assign allocation to earlier PO
                            allocList[0].PurchaseLineId = pod.Id;

                        } else {
                            // No, assign allocation to later PO
                            allocList[0].PurchaseLineId = targetPod.Id;
                        }
                    }
                    InsertOrUpdateAllocation(allocList[0], user, LockAllocation(allocList[0]));
                }
            }
        }
    }
}
