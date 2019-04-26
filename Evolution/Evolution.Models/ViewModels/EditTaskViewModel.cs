using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Evolution.Models.Models;

namespace Evolution.Models.ViewModels {
    public class EditTaskViewModel : ViewModelBase {
        public TaskModel Task { set; get; }
        public TaskFollowupModel Followup { set; get; }

        public List<ListItemModel> BusinessUnitList { set; get; } = new List<ListItemModel>();
        public List<ListItemModel> TaskTypeList { set; get; } = new List<ListItemModel>();
        public List<ListItemModel> StatusList { set; get; } = new List<ListItemModel>();
        public List<ListItemModel> UserList { set; get; } = new List<ListItemModel>();
    }
}
