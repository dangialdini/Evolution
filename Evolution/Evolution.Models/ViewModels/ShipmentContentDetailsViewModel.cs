using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Evolution.Models.Models;

namespace Evolution.Models.ViewModels {
    public class ShipmentContentDetailsViewModel : ViewModelBase {
        public ShipmentModel Shipment { set; get; } = new ShipmentModel();
        public ShipmentContentModel ShipmentContent { set; get; } = new ShipmentContentModel();
        public PurchaseOrderHeaderModel PurchaseOrder { set; get; } = new PurchaseOrderHeaderModel();

        public string LGS2 { set; get; }
        public string LGS3 { set; get; }
        public string BrandCategory { set; get; } = "";
        public decimal PurchaseAmount { set; get; } = 0;
        public decimal ExchangeRate { set; get; } = 0;
        public decimal CurrencyAmount { set; get; } = 0;
        
        public List<ListItemModel> CurrencyList { set; get; } = new List<ListItemModel>();
        public List<ListItemModel> PaymentTermsList { set; get; } = new List<ListItemModel>();
    }
}
