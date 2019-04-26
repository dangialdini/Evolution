using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Evolution.Models.Models {
    public class SalesOrderSummaryModel {
        public double SubTotal { set; get; } = 0;
        public string TaxName { set; get; } = "";
        public double TaxTotal { set; get; } = 0;
        public double Total { set; get; } = 0;
        public double TotalCbms { set; get; } = 0;
        public string CurrencySymbol { set; get; } = "";
    }
}
