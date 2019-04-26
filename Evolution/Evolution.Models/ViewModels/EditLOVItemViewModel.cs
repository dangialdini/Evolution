using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Evolution.Models.Models;

namespace Evolution.Models.ViewModels {
    public class EditLOVItemViewModel : ViewModelBase {
        public LOVItemModel LovItem { set; get; }
        public List<ListItemModel> ColourList { set; get; } = new List<ListItemModel>();
    }
}
