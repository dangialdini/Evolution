using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Evolution.Models.Models;

namespace Evolution.Models.ViewModels {
    public class EditCustomerViewModel : ViewModelBase {
        public CustomerModel Customer { set; get; }
        public List<ListItemModel> CustomerTypeList { set; get; }
        public List<ListItemModel> CurrencyList { set; get; }
        public List<ListItemModel> TaxCodeList { set; get; }
        public List<ListItemModel> PriceLevelList { set; get; }
        public List<ListItemModel> PaymentTermList { set; get; }
        public List<ListItemModel> OrderTypeList { set; get; }
    }
}
