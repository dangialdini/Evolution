using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Evolution.DAL;
using Evolution.Models.Models;
using Evolution.CommonService;
using Evolution.Resources;
using AutoMapper;

namespace Evolution.AllocationService {
    public partial class AllocationService {
        public DateTimeOffset? CalculateExpectedCompletionDate(SalesOrderDetailModel sod) {
            DateTimeOffset? result = null;
            DateTimeOffset nullDate = new DateTimeOffset(1899, 1, 1, 0, 0, 0, new TimeSpan());
            DateTimeOffset? dteNow = null;
            DateTimeOffset? dteEver = db.FindAllocationECD(sod.Id).FirstOrDefault();

            var cnt = db.FindAllocationsForSalesOrderDetail(sod.CompanyId, sod.Id)
                        .Count();
            if (cnt > 0) { 
                dteNow = DateTimeOffset.Now;
            } else {
                dteNow = nullDate;
            }

            if ((dteEver == null || dteEver == nullDate) && 
                (dteNow == null || dteNow == nullDate)) {
                result = null;
            } else {
                if (dteEver > dteNow) {
                    result = dteEver;
                } else {
                    result = dteNow;
                }
            }
            if (result == nullDate) result = null;

            return result;
        }
    }
}
