using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Evolution.Models.Models;

namespace Evolution.Models.ViewModels {
    public class BrandsViewModel : ViewModelBase {
        public int SelectedBrandId { set; get; } = 0;
        public List<ListItemModel> Brands { set; get; } = new List<ListItemModel>();
        public int GridIndex { set; get; } = 0;
    }
}
