using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Evolution.Models.Models;

namespace Evolution.Models.ViewModels {
    public class PicksListViewModel : ViewModelBase {
        public PicksListModel Picks { get; set; }

        public int SelectedSalesId { get; set; } = 0;
        public List<ListItemModel> SalesList = new List<ListItemModel>();
    }
}
