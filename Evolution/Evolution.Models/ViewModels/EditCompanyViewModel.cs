using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Evolution.Models.Models;

namespace Evolution.Models.ViewModels {
    public class EditCompanyViewModel : ViewModelBase {
        public CompanyModel Company { set; get; }
        public List<ListItemModel> LogoList { set; get; }
        public List<ListItemModel> LocationList { set; get; }
        public List<ListItemModel> WarehouseTemplateList { set; get; }
        public List<ListItemModel> SupplierTemplateList { set; get; }
        public List<ListItemModel> FreightForwarderTemplateList { set; get; }
        public List<ListItemModel> CountryList { set; get; }
        public List<ListItemModel> CurrencyList { set; get; }
        public List<ListItemModel> DateFormatList { set; get; }
        public List<ListItemModel> UnitOfMeasureList { set; get; }
        public List<ListItemModel> UserList { set; get; }
    }
}
