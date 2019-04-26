using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Evolution.Models.Models;

namespace Evolution.Models.ViewModels {
    public class CustomerAddressListViewModel : ViewModelBase {
        public CustomerAddressListModel Addresses { set; get; }
        public List<ListItemModel> DateFormatList { set; get; }
    }
}
