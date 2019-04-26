using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Evolution.Models.Models;

namespace Evolution.Models.ViewModels {
    public class EditCustomerAdditionalInfoViewModel : ViewModelBase {
        public CustomerAdditionalInfoModel CustomerAdditionalInfo { set; get; }

        public List<ListItemModel> RegionList { set; get; }
        public string InvoiceTemplateList { set; get; }			// In JSON format
        public string PacklistTemplateList { set; get; }		// In JSON format
        public List<ListItemModel> SourceList { set; get; }
        public List<ListItemModel> OrderTypeList { set; get; }
    }
}
