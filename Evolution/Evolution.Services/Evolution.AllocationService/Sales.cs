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
        // The following method is called by the Sales Service when a Sales Order is saved.
        // It could also be called manually by the user for allocation optimisation
        public void AllocateOnSalesOrderSave(SalesOrderHeaderModel soh) {
            if (soh.SOStatus != null) {
                var sohStatus = db.FindSalesOrderHeaderStatus(soh.SOStatus.Value);
                if(sohStatus != null && sohStatus.AllowAllocation) {
                    // The status of the sale allows an allocation

                    // Find all the lines which have an AllocQty different to the OrderQty
                    // as these may need modification
                    var orderLines = db.FindSalesOrderDetails(soh.CompanyId, soh.Id)
                                       .Where(sod => sod.AllocQty != sod.OrderQty)
                                       .ToList();

                    foreach(var sod in orderLines) {
                        db.NewItemAllocation(sod.Id, sod.ProductId, soh.LocationId, soh.DeliveryWindowOpen);
                    }
                }
            }
        }

        public void RemoveAllocations(SalesOrderHeaderModel soh) {

        }
    }
}
