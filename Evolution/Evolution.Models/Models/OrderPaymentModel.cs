using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Evolution.Extensions;

namespace Evolution.Models.Models {
    public class OrderPaymentModel {
        public int Id { set; get; } = 0;
        public int CompanyId { set; get; } = 0;
        public int OrderNo { set; get; } = 0;
        public DateTimeOffset PaymentDate { set; get; }
        public string PaymentDateISO { get { return PaymentDate.ISODate(); } }
        public decimal Amount { set; get; } = 0;
        public int CurrencyId { set; get; } = 0;
        public string CurrencyName { set; get; } = "";
        public bool IsDeposit { set; get; } = false;
        public bool IsSchedule { set; get; } = false;
        public bool IsChasing { set; get; } = false;
        public bool IsDelayed { set; get; } = false;
        public string Comment { set; get; } = "";
        public int SupplierPaymentLineId { set; get; } = 0;
    }

    public class OrderPaymentListModel : BaseListModel {
        public List<OrderPaymentModel> Items { set; get; } = new List<OrderPaymentModel>();
    }
}
