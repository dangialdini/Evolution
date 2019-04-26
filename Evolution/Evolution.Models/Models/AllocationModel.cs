using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Evolution.Models.Models {
    public class AllocationModel {
        public int Id { set; get; } = 0;
        public int CompanyId { set; get; } = 0;
        public int? ProductId { set; get; } = null;
        public int? SaleLineId { set; get; } = null;
        public int? PurchaseLineId { set; get; } = null;
        public int? Quantity { set; get; } = null;
        public int? LocationId { set; get; } = null;
        public DateTimeOffset? DateCreated { set; get; } = null;
    }

    public class AllocationListModel : BaseListModel {
        public List<AllocationModel> Items { set; get; } = new List<AllocationModel>();
    }
}
