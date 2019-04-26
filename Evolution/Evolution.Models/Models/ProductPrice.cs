using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Evolution.Models.Models {
    public class ProductPriceModel {
        public int Id { set; get; } = 0;
        public int ProductId { set; get; } = 0;
        public int QuantityBreak { set; get; } = 0;
        public decimal QuantityBreakAmount { set; get; } = 0;
        public string PriceLevel { set; get; } = "";
        public string PriceLevelNameId { set; get; } = "";
        public decimal SellingPrice { set; get; } = 0;
        public bool PriceIsInclusive { set; get; } = false;

        public class ProductPriceListModel : BaseListModel {
            public List<ProductPriceModel> Items { set; get; } = new List<ProductPriceModel>();
        }
    }
}
