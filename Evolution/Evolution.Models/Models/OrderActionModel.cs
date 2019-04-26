using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Evolution.Models.Models {
    public class OrderActionItemModel {
        public int Id { set; get; }     // SOH Id
        public string NextActionText { set; get; }
        public string RegionName { set; get; }
        public string PostCode { set; get; }
        public int? OrderNumber { set; get; }
        public string OrderNumberUrl { set; get; }
        public string CustPO { set; get; }
        public string CustomerName { set; get; }
        public string CustomerUrl { set; get; }
        public string WarehouseInstructions { set; get; }
        public string OrderDateISO { set; get; }
        public string DeliveryWindowOpenISO { set; get; }
        public string DeliveryWindowCloseISO { set; get; }
        public string SalesPersonName { set; get; }
        public string FreightCarrier { set; get; }
    }

    public class OrderActionListModel : BaseListModel {
        public List<OrderActionItemModel> Items { set; get; } = new List<OrderActionItemModel>();
    }
}
