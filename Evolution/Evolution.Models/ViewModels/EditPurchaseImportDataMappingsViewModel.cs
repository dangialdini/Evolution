using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Evolution.Models.Models;

namespace Evolution.Models.ViewModels {
    public class EditPurchaseImportDataMappingsViewModel : EditDataMappingViewModel {
        public int LocationId { set; get; } = 0;

        public List<ListItemModel> LocationList { set; get; } = new List<ListItemModel>();
    }
}
