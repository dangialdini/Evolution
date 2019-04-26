using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Evolution.Extensions;

namespace Evolution.Models.Models {
    public class AllocationPOModel {
        public int PurchaseOrderHeaderId { set; get; }
        public int PurchaseOrderDetailId { set; get; }
        public decimal PurchaseOrderNumber { set; get; }
        public string PurchaseStatus { set; get; }
        public DateTimeOffset? PurchaseDate { set; get; }
        public DateTimeOffset? PurchaseETA { set; get; }
        public int QuantityOnOrder { set; get; }
        public int QuantityAllocated { set; get; }
        public int QuantityFree { set; get; }

        // Additional fields
        public string PurchaseDateISO { get { return PurchaseDate.ISODate(); } }
        public string PurchaseETAISO { get { return PurchaseETA.ISODate(); } }
    }

    public class AllocationPOListModel : BaseListModel {
        public List<AllocationPOModel> Items { set; get; } = new List<AllocationPOModel>();
    }
}
