using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Evolution.Models.Models;

namespace Evolution.Models.ViewModels {
    public class NewCustomerViewModel : ViewModelBase {
        public NewCustomerModel Customer { set; get; } = new NewCustomerModel();
        public CustomerAdditionalInfoModel AdditionalInfo { set; get; } = new CustomerAdditionalInfoModel();

        public List<ListItemModel> CountryList { set; get; }
        public List<ListItemModel> CustomerTypeList { set; get; }
        public List<ListItemModel> RegionList { set; get; }
    }
}
