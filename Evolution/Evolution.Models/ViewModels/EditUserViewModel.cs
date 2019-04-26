using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Evolution.Models.Models;

namespace Evolution.Models.ViewModels {
    public class EditUserViewModel : ViewModelBase {
        public UserModel UserData { set; get; }

        public List<ListItemModel> BrandCategoryList = new List<ListItemModel>();
        public List<ListItemModel> CompanyList = new List<ListItemModel>();
        public List<ListItemModel> DateFormatList = new List<ListItemModel>();
    }
}
