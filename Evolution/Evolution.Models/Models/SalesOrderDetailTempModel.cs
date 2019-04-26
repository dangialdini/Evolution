using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Evolution.Models.Models {
    public class SalesOrderDetailTempModel : SalesOrderDetailModel {
        // Additional fields to SaleOrderDetailModel
        public int UserId { set; get; } = 0;
        public int SalesOrderHeaderTempId { set; get; } = 0;
        public string ProductCode { set; get; } = "";
        public string ProductName { set; get; } = "";
        public string TaxCodeText { set; get; } = "";
        public string LineStatusText { set; get; } = "";
        public string SupplierItemNumber { set; get; } = "";
        public decimal? LinePrice { set; get; } = null;
        public decimal? UnitCBMLinePrice { set; get; } = null;
        public double? UnitCBM { set; get; } = null;
        public int Allocated { set; get; } = 0;
    }

    public class SalesOrderDetailTempListModel : BaseListModel {
        public List<SalesOrderDetailTempModel> Items { set; get; } = new List<SalesOrderDetailTempModel>();
    }
}
