using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Evolution.Models.Models {
    public class PurchaseOrderHeaderStatusModel {
        public int Id { set; get; } = 0;
        public string StatusName { set; get; } = "";
        public int? Sequence { set; get; } = 0;
        public bool AllowManualSelection { set; get; } = false;
        public bool AllowAllocation { set; get; } = false;
        public int StatusValue { set; get; } = 0;
    }

    public class PurchaseOrderHeaderStatusListModel : BaseListModel {
        public List<PurchaseOrderHeaderStatusModel> Items { set; get; } = new List<PurchaseOrderHeaderStatusModel>();
    }
}
