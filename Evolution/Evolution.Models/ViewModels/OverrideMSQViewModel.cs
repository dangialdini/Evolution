using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Evolution.Models.Models;

namespace Evolution.Models.ViewModels {
    public class OverrideMSQViewModel : ViewModelBase {
        public int SelectedUserId { set; get; } = 0;
        public int SalesOrderHeaderTempId { set; get; } = 0;

        public List<ListItemModel> UserList { set; get; } = new List<ListItemModel>();
    }
}
