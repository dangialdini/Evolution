using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Evolution.Models.Models;

namespace Evolution.Models.ViewModels {
    public class EditSalesOrderHeaderTempViewModel : ViewModelBase {
        public SalesOrderHeaderTempModel SaleTemp { set; get; }

        public string LGST { set; get; } = "";
        public bool PartiallyComplete { set; get; } = false;

        public List<ListItemModel> LocationList = new List<ListItemModel>();
        public List<ListItemModel> ShippingTemplateList = new List<ListItemModel>();
        public List<ListItemModel> CountryList = new List<ListItemModel>();
        public List<ListItemModel> OrderTypeList = new List<ListItemModel>();
        public List<ListItemModel> BrandCategoryList = new List<ListItemModel>();
        public List<ListItemModel> SOStatusList = new List<ListItemModel>();
        public List<ListItemModel> UserList = new List<ListItemModel>();
        public List<ListItemModel> PaymentTermsList = new List<ListItemModel>();
        public List<ListItemModel> CreditCardList = new List<ListItemModel>();

        public List<ListItemModel> FreightCarrierList = new List<ListItemModel>();
        public List<ListItemModel> FreightTermList = new List<ListItemModel>();

        public List<ListItemModel> MethodSignedList = new List<ListItemModel>();
    }
}
