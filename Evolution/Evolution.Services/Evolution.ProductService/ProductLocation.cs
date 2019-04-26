using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Evolution.DAL;
using Evolution.Models.Models;
using Evolution.CommonService;
using Evolution.MediaService;
using Evolution.AuditService;
using Evolution.Enumerations;
using Evolution.Resources;
using AutoMapper;

namespace Evolution.ProductService {
    public partial class ProductService {

        #region Public members    

        public ProductLocationModel FindProductLocationModel(int id) {
            ProductLocationModel model = null;

            var pl = db.FindProductLocation(id);
            if (pl != null) {
                model = MapToModel(pl);
            }

            return model;
        }

        public ProductLocationModel FindProductLocationModel(CompanyModel company, ProductModel product, int locationId) {
            ProductLocationModel model = null;

            var pl = db.FindProductLocation(company.Id, product.Id, locationId);
            if (pl != null) {
                model = MapToModel(pl);
            }

            return model;
        }

        public ProductLocationModel MapToModel(ProductLocation item) {
            var newItem = Mapper.Map<ProductLocation, ProductLocationModel>(item);
            return newItem;
        }

        public Error InsertOrUpdateProductLocation(ProductLocationModel productLocation, UserModel user, string lockGuid) {
            Error error = ValidateModel(productLocation);
            if (!error.IsError) {
                // Check that the lock is still current
                if (!db.IsLockStillValid(typeof(ProductLocation).ToString(), productLocation.Id, lockGuid)) {
                    error.SetError(EvolutionResources.errRecordChangedByAnotherUser, "LocationId");

                } else {
                    ProductLocation temp = null;
                    if (productLocation.Id != 0) temp = db.FindProductLocation(productLocation.Id);
                    if (temp == null) temp = new ProductLocation();

                    Mapper.Map<ProductLocationModel, ProductLocation>(productLocation, temp);

                    db.InsertOrUpdateProductLocation(temp);
                    productLocation.Id = temp.Id;
                }
            }
            return error;
        }

        public Error DeleteProductLocation(int id) {
            var error = new Error();
            db.DeleteProductLocation(id);
            return error;
        }

        public string LockProductLocation(ProductLocationModel model) {
            return db.LockRecord(typeof(ProductLocation).ToString(), model.Id);
        }

        #endregion

        #region Private methods

        public Error ValidateModel(ProductLocationModel model) {
            var error = new Error();

            return error;
        }

        #endregion
    }
}
