using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Evolution.Models.Models {
    public class BrandCategorySalesPersonModel {
        public int Id { set; get; } = 0;
        public int CompanyId { set; get; } = 0;
        public int BrandCategoryId { set; get; } = 0;
        public string BrandCategoryName { set; get; } = "";
        public int CustomerId { set; get; } = 0;
        public int UserId { set; get; } = 0;
        public int SalesPersonTypeId { set; get; } = 0;

        // Additional fields
        public string UserName { set; get; } = "";
        public string SalesPersonTypeName { set; get; } = "";
    }

    public class BrandCategorySalesPersonListModel : BaseListModel {
        public List<BrandCategorySalesPersonModel> Items { set; get; } = new List<BrandCategorySalesPersonModel>();
    }
}
