using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Evolution.Models.Models {
    public class ProductIPModel {
        public int Id { set; get; } = 0;
        public int ProductId { set; get; }
        public int MarketId { set; get; }

        // Aditional fields
        public string MarketNameText { set; get; }
        public bool Selected { set; get; } = false;
    }

    public class ProductIPListModel : BaseListModel {
        public List<ProductIPModel> Items { set; get; } = new List<ProductIPModel>();
    }
}
