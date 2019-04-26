using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Evolution.Extensions;

namespace Evolution.Models.Models {
    public class CustomerMarketingModel {
        public int Id { set; get; } = 0;
        public int CompanyId { set; get; } = 0;
        public int CustomerId { set; get; } = 0;
        public int? CustomerContactId { set; get; } = null;
        public string ContactName { set; get; } = "";
        public int? MarketingGroupId { set; get; } = null;
        public string GroupName { set; get; } = "";
        public DateTimeOffset? DateFrom { set; get; } = null;
        public string DateFromISO { get { return DateFrom.ISODate(); } }
        public DateTimeOffset? DateTo { set; get; } = null;
        public string DateToISO { get { return DateTo.ISODate(); } }
    }

    public class CustomerMarketingListModel : BaseListModel {
        public List<CustomerMarketingModel> Items { set; get; } = new List<CustomerMarketingModel>();
    }
}
