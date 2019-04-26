using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Evolution.Models.Models {
    public class CancellationSummaryModel {
        public int CustomerId { set; get; } = 0;
        public int SohId { set; get; } = 0;
        public int SodId { set; get; } = 0;
        public int ProductId { set; get; } = 0;
        public int LocationId { set; get; } = 0;
        public int AllocationId { set; get; } = 0;
        public int PurchaseOrderDetailId { set; get; } = 0;
        public int UserId { set; get; } = 0;
        public int DeliveryWindowOpen { set; get; } = 0;
        public string CustomerName { set; get; } ="";
        public int SaleOrderNo { set; get; } = 0;
        public string ItemNumber { set; get; } = "";
        public string ItemName { set; get; } = "";
        public string LocationName { set; get; } = "";
        public int OrderQty { set; get; } = 0;
        public int AllocQty { set; get; } = 0;
        public string AccountManager { set; get; } = "";
        public string Email { set; get; } = "";
    }

    public class CancellationSummaryListModel : BaseListModel {
        public List<CancellationSummaryModel> Items { set; get; } = new List<CancellationSummaryModel>();
    }
}
