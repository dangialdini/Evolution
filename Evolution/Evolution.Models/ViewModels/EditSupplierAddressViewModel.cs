using Evolution.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Evolution.Models.ViewModels {
    public class EditSupplierAddressViewModel : ViewModelBase {
        public SupplierAddressModel SupplierAddress { get; set; }
        public List<ListItemModel> AddressTypeList { get; set; }
        public List<ListItemModel> CountryList { get; set; }
    }
}
