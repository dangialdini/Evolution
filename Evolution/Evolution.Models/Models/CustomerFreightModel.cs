using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Evolution.Models.Models {
    public class CustomerFreightModel {
        public int CustomerId { set; get; } = 0;
        public int CompanyId { set; get; } = 0;
        public string ShipMethodAccount { set; get; } = "";
        public bool IsManualFreight { set; get; } = false;
        public decimal? FreightRate { set; get; } = null;
        public decimal? MinFreightPerOrder { set; get; } = null;
        public int? FreightCarrierId { set; get; } = null;
        public string DeliveryInstructions { set; get; } = "";
        public string DeliveryContact { set; get; } = "";
        public decimal? MinFreightThreshold { set; get; } = 0;
        public decimal? FreightWhenBelowThreshold { set; get; } = 0;
        public string WarehouseInstructions { set; get; } = "";
        public int? FreightTermId { set; get; } = null;
    }
}
