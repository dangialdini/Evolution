using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Evolution.Extensions;

namespace Evolution.Models.Models {
    public class TransactionDrillDownModel {
        public int Id { get; set; }
        public int? TransactionLineID { get; set; }
        public int? TransactionLineNumber { get; set; }
        public int? ProductId { get; set; }
        public string ItemNumber { get; set; }
        public string ItemName { get; set; }
        public int? QtyOrdered { get; set; }
        public double? ValueOrdered { get; set; }
        public int? NumberInStock { get; set; }
        public int? QtyAvailableNow { get; set; }
        public double? ValueAvailableNow { get; set; }
        public double? PercentAvailableNow { get; set; }
        public int? QtyAvailableAtWished { get; set; }
        public double? ValueAvailableAtWished { get; set; }
        public double? PercentAvailableAtWished { get; set; }
        public int? QtyAvailableEver { get; set; }
        public double? ValueAvailableEver { get; set; }
        public double? PercentAvailableEver { get; set; }
        public int? QtyAvailableRuthless { get; set; }
        public double? ValueAvailableRuthless { get; set; }
        public double? PercentAvailableRuthless { get; set; }
        public DateTimeOffset? ExpectedCompletionDate { get; set; }
        public string NeighbourWarning { get; set; }

        // Additional fields
        public string ExpectedCompletionDateISO { get { return ExpectedCompletionDate.ISODate(); } }
    }

    public class TransactionDrillDownListModel : BaseListModel {
        public List<TransactionDrillDownModel> Items { set; get; } = new List<TransactionDrillDownModel>();
    }
}
