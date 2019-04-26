using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Evolution.Models.Models;

namespace Evolution.Models.ViewModels {
    public class PurchaseOrderSplitViewModel : ViewModelBase {

        public SplitPurchaseModel OrderDetails { set; get; } = new SplitPurchaseModel();

        public string PurchaseOrderList { set; get; } = "";
        public List<ListItemModel> LocationList = new List<ListItemModel>();
    }
}
