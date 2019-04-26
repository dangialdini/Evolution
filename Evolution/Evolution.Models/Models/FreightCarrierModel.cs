using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Evolution.Models.Models {
    public class FreightCarrierModel {
        public int Id { set; get; } = 0;
        public int CompanyId { set; get; } = 0;
        public string FreightCarrier { set; get; } = "";
        public bool AutoBuildTrackingLink { set; get; } = false;
        public string HTTPPrefix { set; get; } = "";
        public bool Enabled { set; get; } = false;
    }

    public class FreightCarrierListModel : BaseListModel {
        public List<FreightCarrierModel> Items { set; get; } = new List<FreightCarrierModel>();
    }
}
