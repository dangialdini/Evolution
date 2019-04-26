using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Evolution.Extensions;

namespace Evolution.Models.Models {
    public class AllocationResultModel {
        public int PurchaseOrderHeaderId { set; get; } = 0;
        public int SalesOrderHeaderId { set; get; } = 0;
        public string CustomerName { set; get; } = "";
        public int OrderNumber { set; get; } = 0;
        public string ItemNumber { set; get; } = "";
        public int Quantity { set; get; } = 0;
        public DateTimeOffset DeliveryWindowOpen { set; get; }
        public string DeliveryWindowOpenISO { get { return DeliveryWindowOpen.ISODate(); } }
        public DateTimeOffset DeliveryWindowClose { set; get; }
        public string DeliveryWindowCloseISO { get { return DeliveryWindowClose.ISODate(); } }
    }

    public class AllocationResultListModel : BaseListModel {
        public List<AllocationResultModel> Items { set; get; } = new List<AllocationResultModel>();
    }
}
