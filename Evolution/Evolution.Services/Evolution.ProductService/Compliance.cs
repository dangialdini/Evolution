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
using Evolution.Extensions;
using Evolution.Enumerations;
using Evolution.Resources;
using AutoMapper;

namespace Evolution.ProductService {
    public partial class ProductService {
        
        #region Public methods

        public ProductComplianceListModel FindProductComplianceListModel(int productId,
                                                                         int index = 0, 
                                                                         int pageNo = 1, 
                                                                         int pageSize = Int32.MaxValue,
                                                                         string search = "",
                                                                         string sortColumn = "", 
                                                                         SortOrder sortOrder = SortOrder.Asc) {
            var model = new ProductComplianceListModel();

            model.GridIndex = index;
            var allItems = db.FindProductCompliances(productId, sortColumn, sortOrder)
                             .Where(pc => string.IsNullOrEmpty(search) ||
                                          (pc.LOVItem_ComplianceCategory != null && pc.LOVItem_ComplianceCategory.ItemText.ToLower().Contains(search.ToLower())) ||
                                          (pc.LOVItem_Market != null && pc.LOVItem_Market.ItemText.ToLower().Contains(search.ToLower()))
                                   );

            model.TotalRecords = allItems.Count();
            foreach (var item in allItems.Skip((pageNo - 1) * pageSize)
                                        .Take(pageSize)) {
                model.Items.Add(MapToModel(item));
            }

            return model;
        }

        public ProductComplianceModel FindProductComplianceModel(int id, int productId, bool bCreateEmptyIfNotfound = true) {
            ProductComplianceModel model = null;

            var p = db.FindProductCompliance(id);
            if (p == null) {
                if (bCreateEmptyIfNotfound) model = new ProductComplianceModel { ProductId = productId };

            } else {
                model = MapToModel(p);
            }

            return model;
        }

        public ProductComplianceModel MapToModel(ProductCompliance item) {
            if (item == null) {
                return null;
            } else {
                var newItem = Mapper.Map<ProductCompliance, ProductComplianceModel>(item);
                if (item.LOVItem_ComplianceCategory != null) newItem.ComplianceCategoryText = item.LOVItem_ComplianceCategory.ItemText;
                if (item.LOVItem_Market != null) newItem.MarketNameText = item.LOVItem_Market.ItemText;

                // Add the attachments
                foreach(var attachment in item.ProductComplianceAttachments) {
                    var newAttachment = new ProductComplianceAttachmentModel {
                        Id = attachment.Id,
                        ProductComplianceId = attachment.ProductComplianceId,
                        MediaId = attachment.MediaId,
                        FileName = attachment.Medium.FileName,
                        QualName = MediaServices.GetMediaFolder(-1, true) + attachment.Medium.FolderName + attachment.Medium.FileName
                    };
                    newItem.Attachments.Add(newAttachment);

                    if (!string.IsNullOrEmpty(newItem.AttachmentHtml)) newItem.AttachmentHtml += "<br/>";
                    newItem.AttachmentHtml += $"<a href=\"{newAttachment.QualName}\" target=\"_new\">{newAttachment.FileName}</a>";
                }
                return newItem;
            }
        }

        public ProductComplianceAttachmentListModel FindProductComplianceAttachmentListModel(int productComplianceId) {
            ProductComplianceAttachmentListModel model = new ProductComplianceAttachmentListModel();

            var pca = db.FindProductComplianceAttachmentListModel(productComplianceId);
            foreach(var item in pca) {
                model.Items.Add(MapToModel(item));
            }
            return model;
        }

        public ProductComplianceAttachmentModel MapToModel(ProductComplianceAttachment item) {
            if(item == null) {
                return null;
            } else {
                return Mapper.Map<ProductComplianceAttachment, ProductComplianceAttachmentModel>(item);
            }
        }

        public void AddMediaToProductCompliance(ProductComplianceModel productCompliance, MediaModel media) {
            db.AddMediaToProductCompliance(productCompliance.Id, media.Id);
        }

        public Error InsertOrUpdateProductCompliance(ProductComplianceModel productCompliance, string lockGuid) {
            Error error = ValidateModel(productCompliance);
            if (!error.IsError) {
                // Check that the lock is still current
                if (!db.IsLockStillValid(typeof(ProductCompliance).ToString(), productCompliance.Id, lockGuid)) {
                    error.SetError(EvolutionResources.errRecordChangedByAnotherUser, "ComplianceCategoryId");

                } else {
                    ProductCompliance temp = null;
                    if (productCompliance.Id != 0) temp = db.FindProductCompliance(productCompliance.Id);
                    if (temp == null) temp = new ProductCompliance();

                    Mapper.Map(productCompliance, temp);

                    db.InsertOrUpdateProductCompliance(temp);
                    productCompliance.Id = temp.Id;
                }
            }
            return error;
        }

        public Error DeleteProductCompliance(int id) {
            var error = new Error();

            var p = db.FindProductCompliance(id);
            if (p != null) {
                foreach(var attachment in p.ProductComplianceAttachments.ToList()) {
                    deleteProductComplianceAttachment(attachment.Id);
                }

                var mediaFolder = MediaServices.GetMediaFolder(MediaFolder.ProductCompliance,
                                                               0,   //company.Id,   // Products have no companyId
                                                               p.ProductId,
                                                               p.Id,
                                                               false,
                                                               false);
                MediaService.MediaService.DeleteDirectory(mediaFolder, true);

                db.DeleteProductCompliance(id);
            }
            return error;
        }

        public Error DeleteProductComplianceAttachment(ProductComplianceAttachmentModel attachment) {
            return deleteProductComplianceAttachment(attachment.Id);
        }

        public string LockProductCompliance(ProductComplianceModel model) {
            return db.LockRecord(typeof(ProductCompliance).ToString(), model.Id);
        }

        #endregion

        #region Private methods

        private Error deleteProductComplianceAttachment(int id) {
            var error = new Error();

            var p = db.FindProductComplianceAttachment(id);
            if (p != null) {
                var media = MediaServices.FindMediaModel(p.MediaId);
                db.DeleteProductComplianceAttachment(id);
                if (media != null) MediaServices.DeleteMedia(media);
            }

            return error;
        }

        public Error ValidateModel(ProductComplianceModel model) {
            var error = new Error();

            return error;
        }

        #endregion
    }
}
