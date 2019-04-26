using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Evolution.DAL;
using Evolution.Models.Models;
using Evolution.CommonService;
using Evolution.Resources;

namespace Evolution.ProductService {
    public partial class ProductService {

        #region Public members    

        public List<ListItemModel> FindBrandBrandCategoriesListItemModel(BrandCategoryModel brandCategory) {//int brandCategoryId) {
            return db.FindBrandBrandCategories(brandCategory.Id)
                     .Select(bc => new ListItemModel {
                         Id = bc.Brand.Id.ToString(),
                         Text = bc.Brand.BrandName,
                         ImageURL = ""
                     })
                     .ToList();
        }

        #endregion
    }
}
