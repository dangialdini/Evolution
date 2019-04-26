using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Evolution.Models.Models {
    public class PricingInfoModel {
        public decimal SellingPrice { set; get; } = 0;
        public int MinSaleQty { set; get; } = 1;
    }
}
