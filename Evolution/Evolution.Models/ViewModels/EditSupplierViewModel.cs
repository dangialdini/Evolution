using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Evolution.Models.Models;

namespace Evolution.Models.ViewModels {
    public class EditSupplierViewModel : ViewModelBase {
        public SupplierModel Supplier { set; get; }
        public SupplierAddressModel SupplierAddress { set; get; }
        public List<ListItemModel> CountryList { set; get; }
        public List<ListItemModel> CurrencyList { set; get; }
        public List<ListItemModel> TaxCodeList { set; get; }
        public List<ListItemModel> SupplierTermList { set; get; }
        public List<ListItemModel> CommercialTermList { set; get; }
        //public List<ListItemModel> FreightForwarderList { set; get; }
        public List<ListItemModel> PortList { set; get; }
        public List<ListItemModel> ShipMethodList { set; get; }
    }
}
