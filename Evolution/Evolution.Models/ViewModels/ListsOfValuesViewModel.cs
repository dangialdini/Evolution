using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Evolution.Models.Models;

namespace Evolution.Models.ViewModels {
    public class ListsOfValuesViewModel : ViewModelBase {
        public int SelectedListId { set; get; }
        public List<LOVModel> Lists { set; get; } = new List<LOVModel>();
        public LOVListModel ListItems { set; get; } = new LOVListModel();
        public int GridIndex { set; get; } = 0;
    }
}
