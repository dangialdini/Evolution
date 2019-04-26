using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Evolution.Models.Models {
    public class PurchaseOrderSummaryModel {
        // Order metrics
        public decimal? OrderNumber { set; get; } = 0;
        public double? TotalCbms { set; get; } = null;
        public double? AllocValueEx { set; get; } = null;
        public double? OrderValueEx { set; get; } = null;
        public double? AllocatedPercent { set; get; } = null;
        public string TaxCode { set; get; } = "";
        public double? Tax { set; get; } = null;
        public double? Total { set; get; } = null;

        // Order status
        public string POStatusText { set; get; } = "";

        // Order dates
        public string LandingDate { set; get; } = "";
        public string RealisticRequiredDate { set; get; } = "";          // Reallistic ETA
        public string RequiredDate { set; get; } = "";                   // Avd US Final
        public string CompletedDate { set; get; } = "";

        public string CurrencySymbol { set; get; } = "";
    }
}
