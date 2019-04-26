using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Evolution.Models.Models;

namespace Evolution.Models.ViewModels {
    public class EditPurchaseOrderHeaderTempViewModel : ViewModelBase {
        public PurchaseOrderHeaderTempModel PurchaseTemp { set; get; }

        public string LGST { set; get; } = "";

        public List<ListItemModel> LocationList = new List<ListItemModel>();
        public List<ListItemModel> SupplierList = new List<ListItemModel>();
        public List<ListItemModel> POStatusList = new List<ListItemModel>();
        public List<ListItemModel> UserList = new List<ListItemModel>();
        public List<ListItemModel> PaymentTermsList = new List<ListItemModel>();
        public List<ListItemModel> CommercialTermsList = new List<ListItemModel>();
        public List<ListItemModel> PortList = new List<ListItemModel>();
        public List<ListItemModel> ShipMethodList = new List<ListItemModel>();
        public List<ListItemModel> ContainerTypeList = new List<ListItemModel>();
        public List<ListItemModel> FreightForwarderList = new List<ListItemModel>();
        public List<ListItemModel> CurrencyList = new List<ListItemModel>();
        public List<ListItemModel> BrandCategoryList = new List<ListItemModel>();
    }
}
