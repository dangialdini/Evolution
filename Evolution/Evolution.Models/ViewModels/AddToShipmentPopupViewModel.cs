using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Evolution.Models.Models;

namespace Evolution.Models.ViewModels {
    public class AddToShipmentPopupViewModel : ViewModelBase {
        public int ShipmentId { set; get; } = 0;
        public string SelectedPOs { set; get; } = "";
        public string ReturnUrl { set; get; } = "";

        public List<ListItemModel> ShipmentList = new List<ListItemModel>();
    }
}
