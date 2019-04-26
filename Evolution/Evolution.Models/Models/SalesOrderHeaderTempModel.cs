using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Evolution.Models.Models {
    public class SalesOrderHeaderTempModel : SalesOrderHeaderModel {
        public int OriginalRowId { set; get; }
        public int SalesOrderHeaderTempId { set; get; }
        public int UserId { set; get; }
        public bool IsMSQOverridable { set; get; } = false;
        public int? OverrideApproverId { set; get; } = null;
    }

    public class SalesOrderHeaderTempListModel : BaseListModel {
        public List<SalesOrderHeaderTempModel> Items { set; get; } = new List<SalesOrderHeaderTempModel>();
    }
}
