using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Evolution.Models.Models;

namespace Evolution.Models.ViewModels {
    public class EditProductComplianceViewModel : FileUploadViewModel {
        public ProductComplianceModel ProductCompliance { set; get; }

        public List<ListItemModel> ComplianceCategoryList = new List<ListItemModel>();
        public List<ListItemModel> MarketList = new List<ListItemModel>();
    }
}
