using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Evolution.Models.Models;

namespace Evolution.Models.ViewModels {
    public class EditCustomerFreightViewModel : ViewModelBase {
        public CustomerFreightModel CustomerFreight { set; get; }

        public List<ListItemModel> FreightCarrierList = new List<ListItemModel>();
        public List<ListItemModel> FreightTermList = new List<ListItemModel>();
    }
}
