using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Evolution.Models.Models {
    public class CreateOrderModel {
        public int LocationId { set; get; } = 0;
        public int SupplierId { set; get; } = 0;
        public int BrandCategoryId { set; get; } = 0;
    }
}
