using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Evolution.Models.Models;

namespace Evolution.Models.ViewModels {
    public class ProductListViewModel : ViewModelBase {
        public ProductListModel Products { set; get; }

        public int SelectedBrandId { set; get; } = 0;
        public List<ListItemModel> BrandList = new List<ListItemModel>();
    }
}
