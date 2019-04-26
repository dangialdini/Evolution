using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Evolution.Models.Models;

namespace Evolution.Models.ViewModels {
    public class EditCustomerAddressViewModel : ViewModelBase {
        public CustomerAddressModel CustomerAddress { set; get; }
        public List<ListItemModel> AddressTypeList { set; get; }
        public List<ListItemModel> CountryList { set; get; }
    }
}
