using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Evolution.Models.Models;

namespace Evolution.Models.ViewModels {
    public class EditSaleImportDataMappingsViewModel : EditDataMappingViewModel {
        public int SOStatus { set; get; } = 0;
        public int SourceId { set; get; } = 0;

        public List<ListItemModel> SOStatusList { set; get; } = new List<ListItemModel>();
        public List<ListItemModel> SourceList { set; get; } = new List<ListItemModel>();
    }
}
