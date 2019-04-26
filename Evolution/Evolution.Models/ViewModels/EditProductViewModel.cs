using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Evolution.Models.Models;
using System.ComponentModel.DataAnnotations;

namespace Evolution.Models.ViewModels {
    public class EditProductViewModel : ViewModelBase {
        public ProductModel Product { set; get; }
        public string MediaPath { set; get; }
        public int? Approved {set;get;}

        public List<ListItemModel> ProductAvailabilityList { set; get; } = new List<ListItemModel>();
        public List<ListItemModel> MaterialList { set; get; } = new List<ListItemModel>();
        public List<ListItemModel> ABList { set; get; } = new List<ListItemModel>();
        public List<ListItemModel> UserList { set; get; } = new List<ListItemModel>();
        public List<ListItemModel> SupplierList { set; get; } = new List<ListItemModel>();
        public List<ListItemModel> ManufacturerList { set; get; } = new List<ListItemModel>();
        public List<ListItemModel> CountryList { set; get; } = new List<ListItemModel>();
        public List<ListItemModel> ProductStatusList { set; get; } = new List<ListItemModel>();
        public List<ListItemModel> PackPackingTypeList { set; get; } = new List<ListItemModel>();
        public List<ListItemModel> CategoryList { set; get; } = new List<ListItemModel>();
        public List<ListItemModel> FormatList { set; get; } = new List<ListItemModel>();
        public List<ListItemModel> FormatTypeList { set; get; } = new List<ListItemModel>();
        public List<ListItemModel> SeasonList { set; get; } = new List<ListItemModel>();
        public List<ListItemModel> PackingTypeList { set; get; } = new List<ListItemModel>();
        public List<ListItemModel> KidsAdultsList { set; get; } = new List<ListItemModel>();
        public List<ListItemModel> AgeGroupList { set; get; } = new List<ListItemModel>();
        public List<ListItemModel> DvlptTypeList { set; get; } = new List<ListItemModel>();
        public List<ListItemModel> PCProductList { set; get; } = new List<ListItemModel>();
        public List<ListItemModel> PCDvlptList { set; get; } = new List<ListItemModel>();
        public List<ListItemModel> WebCategoryList { set; get; } = new List<ListItemModel>();
        public List<ListItemModel> WebSubCategoryList { set; get; } = new List<ListItemModel>();
    }
}
