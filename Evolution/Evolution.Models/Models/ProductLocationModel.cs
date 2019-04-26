using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Evolution.Models.Models {
    public class ProductLocationModel {
        public int Id { set; get; } = 0;
        public int CompanyId { set; get; } = 0;
        public int ProductId { set; get; } = 0;
        public int LocationId { set; get; } = 0;
        public double QuantityOnHand { set; get; } = 0;
        public double SellOnOrder { set; get; } = 0;
        public double PurchaseOnOrder { set; get; } = 0;
    }

    public class ProductLocationListModel : BaseListModel {
        public List<ProductLocationModel> Items { set; get; } = new List<ProductLocationModel>();
    }
}
