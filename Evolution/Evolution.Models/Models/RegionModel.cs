using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Evolution.Models.Models {
    public class RegionModel {
        public int Id { set; get; } = 0;
        public int CompanyId { set; get; } = 0;
        public string RegionName { set; get; } = "";
        public string CountryCode { set; get; } = "";
        public string PostCodeFrom { set; get; } = "";
        public string PostCodeTo { set; get; } = "";
        public decimal? FreightRate { set; get; } = 0;
        public bool Enabled { set; get; } = false;
    }

    public class RegionListModel : BaseListModel {
        public List<RegionModel> Items { set; get; } = new List<RegionModel>();
    }
}
