using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Evolution.Models.Models {
    public class PurchaseOrderDetailSplitModel {
        public int Id { set; get; } = 0;
        public int CompanyId { set; get; } = 0;
        public int PurchaseOrderHeaderId { set; get; } = 0;
        public int PurchaseOrderDetailId { set; get; } = 0;
        public int? ProductId { set; get; } = null;
        public string ProductDescription { set; get; } = "";
        public int OrigOrderQty { set; get; } = 0;
        public int RemainingQty { set; get; } = 0;
        public int SplitToNewOrderQty { set; get; } = 0;
        public int TargetOrderQty { set; get; } = 0;
        public int? TargetPurchaseOrderHeaderId { set; get; } = 0;

        // Additional fields
        public string ItemNumber { set; get; } = "";
    }

    public class PurchaseDetailSplitListModel : BaseListModel {
        public List<PurchaseOrderDetailSplitModel> Items { set; get; } = new List<PurchaseOrderDetailSplitModel>();
    }
}
