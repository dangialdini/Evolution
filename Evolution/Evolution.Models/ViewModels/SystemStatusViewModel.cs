using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Evolution.Models.Models;

namespace Evolution.Models.ViewModels {
    public class SystemStatusViewModel : ViewModelBase {
        public List<ListItemModel> Attributes { set; get; } = new List<ListItemModel>();
    }
}
