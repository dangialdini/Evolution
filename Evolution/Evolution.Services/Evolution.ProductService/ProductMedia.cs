using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Evolution.DAL;
using Evolution.Models.Models;
using Evolution.CommonService;
using Evolution.FileCompressionService;
using Evolution.Enumerations;
using Evolution.Extensions;
using Evolution.Resources;

namespace Evolution.ProductService {
    public partial class ProductService {

        public ProductMediaListModel FindProductMediaListModel(int id, int index) {
            var model = new ProductMediaListModel { GridIndex = index };

            var allItems = db.FindProductMedias(id);

            model.TotalRecords = allItems.Count();

            foreach (var item in allItems) {
                var media = MediaServices.MapToModel(item.Medium, MediaSize.Medium, (int)MediaSize.MediumW, (int)MediaSize.MediumH);

                var newItem = new ProductMediaModel {
                    Id = item.Id,
                    ProductId = item.ProductId,
                    MediaId = item.MediaId,
                    MediaFile = media.MediaFile,
                    MediaHtml = media.MediaHtml,
                    IsPrimary = (item.Id == item.Product.PrimaryMediaId ? true : false)
                };
                model.Items.Add(newItem);
            }

            return model;
        }

        public string GetProductImage(ProductModel product, MediaSize thumbSize, int fitX, int fitY, bool bHttp) {
            string rc = null;
            if(product != null) {
                if(product.PrimaryMediaId != null) {
                    var prodMedia = db.FindProductMedia(product.PrimaryMediaId.Value);
                    if(prodMedia != null) {
                        var media = MediaServices.FindMediaModel(prodMedia.MediaId, thumbSize, fitX, fitY);
                        if (media != null) {
                            string temp = MediaServices.GetMediaFileName(media, false);
                            if (File.Exists(temp)) {
                                int imageW = 0,
                                    imageH = 0;
                                rc = MediaServices.GetMediaThumb(media, thumbSize, fitX, fitY, ref imageW, ref imageH);
                            }
                        }
                    }
                }
            }
            if(rc == null) rc = GetConfigurationSetting("SiteFolder", "") + "\\Content\\Default.jpg";
            return rc;
        }

        public Error AddMediaToProduct(ProductModel product, 
                                       CompanyModel company, 
                                       UserModel user, 
                                       string mediaFile,
                                       ProductMediaModel prodMedia,      // output
                                       FileCopyType copyType) {
            var error = new Error();

            var media = new MediaModel();

            error = MediaServices.InsertOrUpdateMedia(media,
                                                      company,
                                                      user,
                                                      MediaFolder.Product,
                                                      mediaFile,
                                                      "",
                                                      product.Id,
                                                      -1,
                                                      copyType);
            if (!error.IsError) {
                var tempProdMedia = db.FindProductMedias(product.Id)
                                      .Where(pm => pm.MediaId == media.Id)
                                      .FirstOrDefault();
                if (tempProdMedia == null) {
                    // Not currently linked, so link it
                    tempProdMedia = new ProductMedia {
                        Id = 0,
                        ProductId = product.Id,
                        MediaId = media.Id
                    };
                    db.InsertOrUpdateProductMedia(tempProdMedia);
                }
                Mapper.Map<ProductMedia, ProductMediaModel>(tempProdMedia, prodMedia);
            }

            return error;
        }

        public Error SetPrimaryMedia(ProductModel product, ProductMediaModel productMedia, UserModel user, string lgs) {
            return SetPrimaryMedia(product, productMedia.Id, user, lgs);
        }

        public Error SetPrimaryMedia(ProductModel product, int productMediaId, UserModel user, string lgs) {
            var error = new Error();
            var tempProduct = FindProductModel(product.Id, null, null, false);
            if (tempProduct != null) {
                tempProduct.PrimaryMediaId = product.PrimaryMediaId = productMediaId;
                error = InsertOrUpdateProduct(tempProduct, user, lgs);
            }
            return error;
        }

        public Error InsertOrUpdateProductMedia(ProductMediaModel prodMedia, UserModel user, string lockGuid) {
            Error error = validateModel(prodMedia);
            if (!error.IsError) {
                // Check that the lock is still current
                if (!db.IsLockStillValid(typeof(Product).ToString(), prodMedia.Id, lockGuid)) {
                    error.SetError(EvolutionResources.errRecordChangedByAnotherUser, "");

                } else {
                    // Save the product media
                    ProductMedia temp = null;
                    if (prodMedia.Id != 0) temp = db.FindProductMedia(prodMedia.Id);
                    if (temp == null) temp = new ProductMedia();

                    Mapper.Map<ProductMediaModel, ProductMedia>(prodMedia, temp);

                    db.InsertOrUpdateProductMedia(temp);
                    prodMedia.Id = temp.Id;
                }
            }
            return error;
        }

        public Error DeleteProductMedia(int id) {
            var error = new Error();

            var prodMedia = db.FindProductMedia(id);
            var media = MediaServices.MapToModel(prodMedia.Medium, MediaSize.Small, 0, 0);
            db.DeleteProductMedia(id);

            MediaServices.DeleteMedia(media);

            return error;
       }

        private Error validateModel(ProductMediaModel prodMedia) {
            return new Error();
        }
    }
}
