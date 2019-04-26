using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Evolution.Models.Models {
    public class LOVItemModel {
        public int Id { set; get; } = 0;
        public int? CompanyId { set; get; } = 0;
        public int LovId { set; get; } = 0;
        public string ItemText { set; get; } = "";
        public string ItemValue1 { set; get; } = "";
        public string ItemValue2 { set; get; } = "";
        public string Colour { set; get; } = "black";
        public int OrderNo { set; get; } = 0;
        public bool Enabled { set; get; } = false;
    }

    public class LOVListModel : BaseListModel {
        public List<LOVItemModel> Items { set; get; } = new List<LOVItemModel>();
    }
}
