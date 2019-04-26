using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Evolution.Models.Models;

namespace Evolution.Models.ViewModels {
    public class AddPOToShipmentPopupViewModel : ViewModelBase {
        public int ShipmentId { set; get; } = 0;
        public int PurchaseOrderHeaderId { set; get; } = 0;

        public List<ListItemModel> PurchaseOrderList = new List<ListItemModel>();
    }
}
