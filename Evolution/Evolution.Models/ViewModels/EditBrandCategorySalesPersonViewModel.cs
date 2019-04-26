using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Evolution.Models.Models;

namespace Evolution.Models.ViewModels {
    public class EditBrandCategorySalesPersonViewModel : ViewModelBase {
        public BrandCategorySalesPersonModel CustomerAccountManager { set; get; }

        public List<ListItemModel> BrandCategoryList { set; get; } = new List<ListItemModel>();
        public List<ListItemModel> SalesPersonList { set; get; } = new List<ListItemModel>();
        public List<ListItemModel> SalesPersonTypeList { set; get; } = new List<ListItemModel>();
    }
}
