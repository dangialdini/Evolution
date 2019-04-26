using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Evolution.Extensions;

namespace Evolution.Models.Models {
    public class AllocationSOModel {
        public int SalesOrderHeaderId { set; get; }
        public int SalesOrderDetailId { set; get; }
        public int SalesPersonId { set; get; }
        public string CustomerName { set; get; }
        public int SalesOrderNumber { set; get; }
        public string CustPO { set; get; }
        public string SaleStatus { set; get; }
        public DateTimeOffset? OrderDate { set; get; }
        public string AllocationDetail { set; get; }
        public DateTimeOffset? AllocationDate { set; get; }
        public DateTimeOffset? DeliveryWindowOpen { set; get; }
        public DateTimeOffset? DeliveryWindowClose { set; get; }
        public int QuantityOnOrder { set; get; }
        public int QuantityAllocated { set; get; }
        public int QuantityRequired { set; get; }
        public int NextOrderAction { set; get; }

        // Additional fields
        public string OrderDateISO { get { return OrderDate.ISODate(); } }
        public string AllocationDateISO { get { return AllocationDate.ISODate(); } }
        public string DeliveryWindowOpenISO { get { return DeliveryWindowOpen.ISODate(); } }
        public string DeliveryWindowCloseISO { get { return DeliveryWindowClose.ISODate(); } }
        public string SalesPerson { set; get; }
    }

    public class AllocationSOListModel : BaseListModel {
        public List<AllocationSOModel> Items { set; get; } = new List<AllocationSOModel>();
    }
}
