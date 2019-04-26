using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Evolution.DAL;
using Evolution.Models.Models;

namespace Evolution.ProductService {
    public partial class ProductService {

        public ProductPriceModel FindProductPrice(CompanyModel company, int productId, int customerId) {
            ProductPriceModel model = null;

            var pp = db.FindProductPrices(productId)
                       .Where(p => p.PriceLevelNameId == "PLA")
                       .FirstOrDefault();
            if (pp != null) model = Mapper.Map<ProductPrice, ProductPriceModel>(pp);
            return model;
        }
    }
}
