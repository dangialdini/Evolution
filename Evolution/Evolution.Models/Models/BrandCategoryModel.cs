using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Evolution.Models.Models {
    public class BrandCategoryModel {
        public int Id { set; get; } = 0;
        public int CompanyId { set; get; } = 0;
        public string CategoryName { set; get; } = "";
        public int BrandCount { set; get; } = 0;
        public bool Enabled { set; get; } = false;
    }

    public class BrandCategoryListModel : BaseListModel {
        public List<BrandCategoryModel> Items { set; get; } = new List<BrandCategoryModel>();
    }
}
