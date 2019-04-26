using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Evolution.Models.Models;

namespace Evolution.Models.ViewModels {
    public class AllocationsViewModel : ViewModelBase {
        public int BrandCategoryId { set; get; } = 0;
        public int LocationId { set; get; } = 0;
        public string ItemNumber { set; get; } = "";
        public string ProductName { set; get; } = "";


        public List<ListItemModel> BrandCategoryList { set; get; } = new List<ListItemModel>();
        public List<ListItemModel> LocationList { set; get; } = new List<ListItemModel>();
    }
}
