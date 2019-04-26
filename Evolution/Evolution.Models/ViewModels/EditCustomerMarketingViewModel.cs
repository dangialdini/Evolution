using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Evolution.Models.Models;

namespace Evolution.Models.ViewModels {
    public class EditCustomerMarketingViewModel : ViewModelBase {
        public CustomerMarketingModel CustomerMarketing { set; get; }
        public List<ListItemModel> MarketingGroupList { set; get; }
        public List<ListItemModel> CustomerContactList { set; get; }
    }
}
