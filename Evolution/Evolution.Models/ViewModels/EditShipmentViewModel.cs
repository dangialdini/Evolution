using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Evolution.Models.Models;

namespace Evolution.Models.ViewModels {
    public class EditShipmentViewModel : ViewModelBase {
        public ShipmentModel Shipment { set; get; }

        public List<ListItemModel> ShippingMethodList { set; get; }
        public List<ListItemModel> CarrierVesselList { set; get; }
        public List<ListItemModel> PortList { set; get; }
        public List<ListItemModel> SeasonList { set; get; }
    }
}
