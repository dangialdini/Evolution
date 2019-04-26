using System;
using System.IO;
using System.Linq;
using Evolution.CommonService;
using Evolution.DAL;
using Evolution.Models.Models;
using Evolution.Extensions;
using Evolution.Enumerations;
using Evolution.Resources;
using AutoMapper;

namespace Evolution.ShipmentService {
    public partial class ShipmentService {

        #region Public members

        public ShipmentContentModel FindShipmentContentByPONoModel(CompanyModel company, decimal purchaseOrderNo, bool bCreateEmptyIfNotfound = true) {
            ShipmentContentModel model = null;

            // Business rule is that there should only be one record returned
            var s = db.FindShipmentContents(company.Id)
                      .Where(sc => sc.PurchaseOrderHeader.OrderNumber == purchaseOrderNo)
                      .FirstOrDefault();
            if (s == null) {
                if (bCreateEmptyIfNotfound) model = new ShipmentContentModel { CompanyId = company.Id };
            } else {
                model = MapToModel(s);
            }

            return model;
        }

        public ShipmentContentListModel FindShipmentContentListModel(CompanyModel company, int shipmentId, int index) {
            var model = new ShipmentContentListModel();

            foreach (var item in db.FindShipmentContents(company.Id, shipmentId)) {
                model.Items.Add(MapToModel(item));

            }
            return model;
        }

        public ShipmentContentModel FindShipmentContentModel(int contentId) {
            ShipmentContentModel model = null;
            var item = db.FindShipmentContent(contentId);
            if (item != null) model = MapToModel(item);
            return model;
        }

        public ShipmentContentModel MapToModel(ShipmentContent item) {
            var newItem = Mapper.Map<ShipmentContent, ShipmentContentModel>(item);

            newItem.CBMEstimate = 0;
            if (item.PurchaseOrderHeader != null) {
                newItem.OrderNumber = item.PurchaseOrderHeader.OrderNumber;

                foreach(var pod in item.PurchaseOrderHeader.PurchaseOrderDetails) {
                    if (pod.OrderQty != null && pod.Product != null && pod.Product.UnitCBM != null)
                        newItem.CBMEstimate += (double)pod.OrderQty.Value * pod.Product.UnitCBM.Value;
                }
            }
            if (item.Supplier != null) newItem.SupplierName = item.Supplier.Name;

            return newItem;
        }

        private void mapToEntity(ShipmentContentModel model, ShipmentContent entity) {
            Mapper.Map<ShipmentContentModel, ShipmentContent>(model, entity);
        }


        public void AddPurchaseOrders(CompanyModel company, UserModel user,
                                      ShipmentModel shipment, string pohIdList) {
            if (!string.IsNullOrEmpty(pohIdList)) {
                string[] pohList = pohIdList.Split(',');

                var shipmentContent = FindShipmentContentListModel(company, shipment.Id, 0);

                foreach (var po in pohList) {
                    int pohId = Convert.ToInt32(po);

                    if(shipmentContent.Items
                                      .Where(sc => sc.PurchaseOrderHeaderId == pohId)
                                      .Count() == 0) {
                        // Not already on the shipment, so add it
                        var poh = db.FindPurchaseOrderHeader(pohId);
                        if (poh != null) {
                            var content = new ShipmentContentModel {
                                CompanyId = company.Id,
                                ShipmentId = shipment.Id,
                                PurchaseOrderHeaderId = pohId,
                                OrderNumber = poh.OrderNumber,       // Was PONo
                                CBMEstimate = db.FindPurchaseOrderCBMs(poh.Id),
                                //public double? CBMCharged { set; get; } = 0;
                                SupplierId = poh.SupplierId
                                //public string ProductBrand { set; get; } = "";
                            };
                            InsertOrUpdateShipmentContent(content, user, "");
                        }
                    }
                }
            }
        }

        public ShipmentContentModel AddPurchaseOrder(CompanyModel company, UserModel user,
                                                     ShipmentModel shipment, PurchaseOrderHeaderModel poh) {
            ShipmentContentModel newItem = null;
            var shipmentContent = FindShipmentContentListModel(company, shipment.Id, 0);

            if (shipmentContent.Items
                                .Where(sc => sc.PurchaseOrderHeaderId == poh.Id)
                                .Count() == 0) {
                // Not already on the shipment, so add it
                Supplier supplier = null;
                if (poh.SupplierId != null) supplier = db.FindSupplier(poh.SupplierId.Value);

                var content = new ShipmentContentModel {
                    CompanyId = company.Id,
                    ShipmentId = shipment.Id,
                    PurchaseOrderHeaderId = poh.Id,
                    OrderNumber = poh.OrderNumber,       // Was PONo
                    CBMEstimate = db.FindPurchaseOrderCBMs(poh.Id),
                    //public double? CBMCharged { set; get; } = 0;
                    SupplierId = poh.SupplierId,
                    SupplierName = (supplier == null ? "" : supplier.Name),
                    //public string ProductBrand { set; get; } = "";
                };
                InsertOrUpdateShipmentContent(content, user, "");
                newItem = content;
            }
            return newItem;
        }

        public Error InsertOrUpdateShipmentContent(ShipmentContentModel content, UserModel user, string lockGuid) {
            var error = validateShipmentContentModel(content);
            if (!error.IsError) {
                // Check that the lock is still current
                if (!db.IsLockStillValid(typeof(ShipmentContent).ToString(), content.Id, lockGuid)) {
                    error.SetError(EvolutionResources.errRecordChangedByAnotherUser, "OrderNumber");

                } else {
                    ShipmentContent temp = null;
                    if (content.Id != 0) temp = db.FindShipmentContent(content.Id);
                    if (temp == null) temp = new ShipmentContent();

                    mapToEntity(content, temp);

                    db.InsertOrUpdateShipmentContent(temp);
                    content.Id = temp.Id;
                }
            }
            return error;
        }

        public void DeleteShipmentContent(int id, bool bDeleteShipmentIfNoContent) {
            db.DeleteShipmentContent(id, bDeleteShipmentIfNoContent);
        }

        #endregion

        #region Private members

        private Error validateShipmentContentModel(ShipmentContentModel model) {
            var error = isValidNonRequiredString(getFieldValue(model.ProductBrand), 150, "ProductBrand", EvolutionResources.errTextDataRequiredInField);
            if(!error.IsError) error = isValidNonRequiredString(getFieldValue(model.Comments), 255, "Comments", EvolutionResources.errTextDataRequiredInField);

            return error;
        }

        #endregion
    }
}
