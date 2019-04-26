using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Evolution.Models.Models {
    public class SplitPurchaseModel {
        public int PurchaseOrderHeaderId { set; get; } = 0;
        public string SupplierName { set; get; } = "";
        public decimal OrderNumber { set; get; } = 0;
        public string AdvertisedETA { set; get; } = "";
        public DateTimeOffset? NewOrderAdvertisedETA { set; get; }
        public int? LocationId { set; get; } = 0;
        public List<SplitPurchaseItemModel> SplitItems { set; get; } = new List<SplitPurchaseItemModel>();
    }

    public class SplitPurchaseItemModel {
        public int PurchaseOrderDetailId { set; get; } = 0;
        public int NewOrderQty { set; get; } = 0;
        public int TargetOrderQty { set; get; } = 0;
        public int TargetOrderId { set; get; } = 0;
        public int RowNumber { set; get; } = 0;
    }
}
