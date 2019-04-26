using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Evolution.Models.Models {
    public class MarketingGroupModel {
        public int Id { set; get; } = 0;
        public int CompanyId { set; get; } = 0;
        public string MarketingGroupName { set; get; } = "";
        public bool Enabled { set; get; } = false;
    }

    public class MarketingGroupListModel : BaseListModel {
        public List<MarketingGroupModel> Items { set; get; } = new List<MarketingGroupModel>();
    }
}
