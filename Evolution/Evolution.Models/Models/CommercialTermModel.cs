using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Evolution.Models.Models {
    public class CommercialTermModel {
        public int Id { set; get; } = 0;
        public int CompanyId { set; get; } = 0;
        public double? LatePaymentChargePercent { set; get; } = 0;
        public double? EarlyPaymentDiscountPercent { set; get; } = 0;
        public string TermsOfPaymentId { set; get; } = "";
        public int? ImportPaymentIsDue { set; get; } = 0;
        public Int16? DiscountDays { set; get; } = 0;
        public Int16? BalanceDueDays { set; get; } = 0;
        public string DiscountDate { set; get; } = "";
        public string BalanceDueDate { set; get; } = "";
        public bool Enabled { set; get; } = false;
    }

    public class CommercialTermListModel : BaseListModel {
        public List<CommercialTermModel> Items { set; get; } = new List<CommercialTermModel>();
    }
}
