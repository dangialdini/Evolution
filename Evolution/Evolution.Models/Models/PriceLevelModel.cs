using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Evolution.Models.Models {
    public class PriceLevelModel {
        public int Id { set; get; } = 0;
        public int CompanyId { set; get; } = 0;
        public string Mneumonic { set; get; } = "";
        public string Description { set; get; } = "";
        public string ImportPriceLevel { set; get; } = "";
        public string ImportSalesTaxCalcMethod { set; get; } = "";
        public bool Enabled { set; get; } = false;
    }

    public class PriceLevelListModel : BaseListModel {
        public List<PriceLevelModel> Items { set; get; } = new List<PriceLevelModel>();
    }
}
