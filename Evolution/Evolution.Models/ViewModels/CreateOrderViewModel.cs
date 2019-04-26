using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Evolution.Models.Models;

namespace Evolution.Models.ViewModels {
    public class CreateOrderViewModel : ViewModelBase {
        public CreateOrderModel Order { set; get; }

        public List<ListItemModel> LocationList = new List<ListItemModel>();
        public List<ListItemModel> SupplierList = new List<ListItemModel>();
        public List<ListItemModel> BrandCategoryList = new List<ListItemModel>();
    }
}
