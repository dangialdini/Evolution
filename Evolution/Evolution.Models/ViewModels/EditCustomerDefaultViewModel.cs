using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Evolution.Models.Models;

namespace Evolution.Models.ViewModels {
    public class EditCustomerDefaultViewModel : ViewModelBase {
        public CustomerDefaultModel CustomerDefault { set; get; }

        public List<ListItemModel> CountryList { set; get; }
        public List<ListItemModel> CurrencyList { set; get; }
        public List<ListItemModel> CustomerTypeList { set; get; }
        public List<ListItemModel> SalesPersonList { set; get; }
        public List<ListItemModel> PaymentTermsList { set; get; }
        public List<ListItemModel> TaxCodeList { set; get; }
        public List<ListItemModel> ShippingMethodList { set; get; }
        public List<ListItemModel> FreightCarrierList { set; get; }
        public string InvoiceTemplateList { set; get; }			// In JSON format
        public string PacklistTemplateList { set; get; }		// In JSON format
        public List<ListItemModel> PriceLevelsList { get; set; }
    }
}
