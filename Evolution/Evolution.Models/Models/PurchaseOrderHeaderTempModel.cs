using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Evolution.Models.Models {
    public class PurchaseOrderHeaderTempModel : PurchaseOrderHeaderModel {
        public int OriginalRowId { set; get; }
        public int PurchaseOrderHeaderTempId { set; get; }
        public int UserId { set; get; }
    }

    public class PurchaseOrderHeaderTempListModel : BaseListModel {
        public List<PurchaseOrderHeaderTempModel> Items { set; get; } = new List<PurchaseOrderHeaderTempModel>();
    }
}
