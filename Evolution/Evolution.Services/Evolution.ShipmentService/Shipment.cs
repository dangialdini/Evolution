using System;
using System.Collections.Generic;
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

        public List<ListItemModel> FindShipmentsListItemModel(CompanyModel company, bool bInsertNew = false) {
            List<ListItemModel> items = new List<ListItemModel>();
            items = db.FindShipments(company.Id)
                      .OrderByDescending(s => s.Id)
                      .Select(s => new ListItemModel {
                        Id = s.Id.ToString(),
                        Text = s.Id.ToString(),
                        ImageURL = ""
                        })
                      .ToList();

            if (bInsertNew) items.Insert(0, new ListItemModel(EvolutionResources.lblNewShipment, "0"));
            return items;
        }

        public ShipmentResultListModel FindShipmentsListModel(int companyId, 
                                                              int index, 
                                                              int pageNo, 
                                                              int pageSize, 
                                                              string search,
                                                              int purchaserId = 0, 
                                                              int brandCatId = 0,
                                                              int openorderId = 1,
                                                              int orderStatusId = 0,
                                                              string sortColumn = "",
                                                              SortOrder sortOrder = SortOrder.Asc) {
            var model = new ShipmentResultListModel();

            decimal numValue = 0;
            bool gotNum = decimal.TryParse(search, out numValue);

            // Do a case-insensitive search
            model.GridIndex = index;
            var allItems = db.FindShippingRegister(companyId)
                             .Where(s => (purchaserId == 0 || s.SalesPersonId == purchaserId) &&
                                         (brandCatId == 0 || s.BrandCategoryID == brandCatId) &&
                                         ((openorderId == 0 && s.POStatus != (int)PurchaseOrderStatus.Closed) || (openorderId == 1)) &&
                                         (orderStatusId == 0 || s.POStatus == orderStatusId) &&
                                         (string.IsNullOrEmpty(search) ||
                                          ((s.SupplierName != null && s.SupplierName.ToLower().Contains(search.ToLower())) ||
                                          (s.SalesPerson != null && s.SalesPerson.ToLower().Contains(search.ToLower())) ||
                                          (s.ProductBrand != null && s.ProductBrand.ToLower().Contains(search.ToLower())) ||
                                          (s.ShippingMethod != null && s.ShippingMethod.ToLower().Contains(search.ToLower())) ||
                                          (s.PortDepart != null && s.PortDepart.ToLower().Contains(search.ToLower())) ||
                                          (s.PortArrival != null && s.PortArrival.ToLower().Contains(search.ToLower())) ||
                                          (s.Season != null && s.Season.ToLower().Contains(search.ToLower())) ||
                                          (s.CurrentStatus != null && s.CurrentStatus.ToLower().Contains(search.ToLower())) ||
                                          (s.BrandCategory != null && s.BrandCategory.ToLower().Contains(search.ToLower())))) &&
                                         (!gotNum || (gotNum && (s.OrderNumber == numValue ||
                                                                 s.ShipmentId == numValue))))
                             .ToList();

            model.TotalRecords = allItems.Count();
            foreach (var item in allItems.Skip((pageNo - 1) * pageSize)
                                        .Take(pageSize)) {
                model.Items.Add(MapToModel(item));
            }
            return model;
        }

        public ShipmentModel FindShipmentModel(int id, CompanyModel company, bool bCreateEmptyIfNotfound = true) {
            ShipmentModel model = null;

            var s = db.FindShipment(id);
            if (s == null) {
                if (bCreateEmptyIfNotfound) model = new ShipmentModel { CompanyId = company.Id };
            } else {
                model = MapToModel(s);
            }

            return model;
        }

        public ShipmentModel MapToModel(Shipment item) {
            var newItem = Mapper.Map<Shipment, ShipmentModel>(item);

            if (item.CarrierVessel != null) newItem.CarrierVesselText = item.CarrierVessel.CarrierVesselName;
            if (item.LOVItem_ShippingMethod != null) newItem.ShippingMethodText = item.LOVItem_ShippingMethod.ItemText;
            if (item.Port_PortDeparture != null) newItem.PortDepartText = item.Port_PortDeparture.PortName;
            if (item.Port_PortArrival != null) newItem.PortArrivalText = item.Port_PortArrival.PortName;

            // Calculate the CBM estimate by totalling all the Purchase orders 
            newItem.CBMEstimate = 0;
            newItem.CBMCharged = 0;
            foreach (var shipmentContent in item.ShipmentContents) {
                if (shipmentContent.PurchaseOrderHeaderId != null) {
                    foreach (var pod in db.FindPurchaseOrderDetails(item.CompanyId, shipmentContent.PurchaseOrderHeaderId.Value)) {
                        if(pod.OrderQty != null && pod.Product != null && pod.Product.UnitCBM != null)
                            newItem.CBMEstimate += pod.OrderQty * pod.Product.UnitCBM;
                    }
                }
                if(shipmentContent.CBMCharged != null) newItem.CBMCharged += shipmentContent.CBMCharged;
            }
            return newItem;
        }

        public ShipmentResultModel MapToModel(FindShippingRegister_Result item) {
            ShipmentResultModel newItem = Mapper.Map<FindShippingRegister_Result, ShipmentResultModel>(item);

            if (!string.IsNullOrEmpty(newItem.SalesPerson)) newItem.SalesPerson = newItem.SalesPerson.WordCapitalise();

            return newItem;
        }

        private void mapToEntity(ShipmentModel model, Shipment entity) {
            Mapper.Map<ShipmentModel, Shipment>(model, entity);
        }

        public ShipmentModel CreateShipment(CompanyModel company, UserModel user) {
            var model = new ShipmentModel {
                CompanyId = company.Id
            };
            InsertOrUpdateShipment(model, user, "");
            return model;
        }

        public Error InsertOrUpdateShipment(ShipmentModel shipment, UserModel user, string lockGuid) {
            var error = validateShipmentModel(shipment);
            if (!error.IsError) {
                // Check that the lock is still current
                if (!db.IsLockStillValid(typeof(Shipment).ToString(), shipment.Id, lockGuid)) {
                    error.SetError(EvolutionResources.errRecordChangedByAnotherUser, "VoyageNo");

                } else {
                    Shipment temp = null;
                    if (shipment.Id != 0) temp = db.FindShipment(shipment.Id);
                    if (temp == null) temp = new Shipment();

                    mapToEntity(shipment, temp);

                    db.InsertOrUpdateShipment(temp);
                    shipment.Id = temp.Id;
                }
            }
            return error;
        }

        public void DeleteShipment(int id) {
            db.DeleteShipment(id);
        }

        public string LockShipment(ShipmentModel model) {
            return db.LockRecord(typeof(Shipment).ToString(), model.Id);
        }

        #endregion

        #region Private members

        private Error validateShipmentModel(ShipmentModel model) {
            var error = isValidNonRequiredString(getFieldValue(model.VoyageNo), 25, "VoyageNo", EvolutionResources.errTextDataRequiredInField);
            if (!error.IsError) error = isValidNonRequiredString(getFieldValue(model.ConsolidationName), 52, "ConsolidationName", EvolutionResources.errTextDataRequiredInField);
            if (!error.IsError) error = isValidNonRequiredString(getFieldValue(model.Comments), 255, "Comments", EvolutionResources.errTextDataRequiredInField);
            if (!error.IsError) error = isValidNonRequiredString(getFieldValue(model.ConsignmentNo), 50, "ConsignmentNo", EvolutionResources.errTextDataRequiredInField);
            if (!error.IsError) error = isValidNonRequiredString(getFieldValue(model.SeasonText), 15, "Season", EvolutionResources.errTextDataRequiredInField);
            if (!error.IsError) error = isValidNonRequiredString(getFieldValue(model.ContainerNo), 255, "ContainerNo", EvolutionResources.errTextDataRequiredInField);

            return error;
        }

        #endregion
    }
}
