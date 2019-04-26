using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Evolution.Models.Models {
    public class PaymentTermModel {
        public int Id { set; get; } = 0;
        public int CompanyId { set; get; } = 0;
        public decimal? LatePaymentChargePercent { set; get; } = 0;
        public decimal? EarlyPaymentDiscountPercent { set; get; } = 0;
        public string TermsOfPaymentId { set; get; } = "";
        public int? ImportPaymentIsDue { set; get; } = 0;
        public int? DiscountDays { set; get; } = 0;
        public int? BalanceDueDays { set; get; } = 0;
        public string DiscountDate { set; get; } = "";
        public string BalanceDueDate { set; get; } = "";
        public bool Enabled { set; get; } = false;

        // Additional properties
        public string TermText { set; get; } = "";
    }

    public class PaymentTermListModel : BaseListModel {
        public List<PaymentTermModel> Items { set; get; } = new List<PaymentTermModel>();
    }
}
