using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Evolution.Models.Models {
    public class BrandModel {
        public int Id { set; get; } = 0;
        public string BrandName { set; get; } = "";     
        public int ProductCount { set; get; } = 0;
        public bool Enabled { set; get; } = false;
    }

    public class BrandListModel : BaseListModel {
        public List<BrandModel> Items { set; get; } = new List<BrandModel>();
    }
}
