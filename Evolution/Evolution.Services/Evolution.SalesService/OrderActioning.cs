using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Evolution.DAL;
using Evolution.Models.Models;
using Evolution.CommonService;
using Evolution.Enumerations;
using Evolution.Extensions;
using Evolution.Resources;

namespace Evolution.SalesService {
    public partial class SalesService {

        #region Public methods

        public OrderActionListModel FindOrderActioning(int companyId, int locationId, int regionId, int nextActionId, int brandCategoryId,
                                                       int index, int pageNo = 1,
                                                       int pageSize = Int32.MaxValue,
                                                       string search = "",
                                                       string sortColumn = "", SortOrder sortOrder = SortOrder.Asc) {
            var model = new OrderActionListModel();

            int searchInt = -1;
            int.TryParse(search, out searchInt);

            // Do a case-insensitive search
            model.GridIndex = index;
            var allItems = db.FindSalesOrderHeaders(companyId, sortColumn, sortOrder)
                             .Where(s => s.BrandCategoryId == brandCategoryId &&
                                         s.LocationId == locationId &&
                                         (regionId == 0 || s.Customer.RegionId == regionId) &&
                                         (nextActionId == 0 || s.NextActionId == nextActionId) &&
                                         s.SalesOrderHeaderStatu.Id == (int)SalesOrderHeaderStatus.ConfirmedOrder &&
                                         s.NextActionId != (int)Enumerations.SaleNextAction.None &&
                                          ((string.IsNullOrEmpty(search) ||
                                          (s.Customer != null && (s.Customer.Name.Contains(search))) ||
                                          (s.CustPO != null && (s.CustPO.Contains(search))) ||
                                          (s.User_SalesPerson != null && ((s.User_SalesPerson.FirstName + " " + s.User_SalesPerson.LastName).Trim().Contains(search)))
                                         ) ||
                                         (searchInt == -1 || s.OrderNumber == searchInt)));

            model.TotalRecords = allItems.Count();
            foreach (var soh in allItems.Skip((pageNo - 1) * pageSize)
                                        .Take(pageSize)) {
                var item = new OrderActionItemModel {
                    Id = soh.Id,
                    NextActionText = (soh.SaleNextAction == null ? "" : soh.SaleNextAction.NextActionDescription),
                    RegionName = soh.Customer.Region.RegionName,
                    PostCode = soh.ShipPostcode,
                    OrderNumber = soh.OrderNumber,
                    OrderNumberUrl = $"<a href=\"/Sales/Sales/Edit?id={soh.Id}\">{soh.OrderNumber}</a>",
                    CustPO = soh.CustPO,
                    CustomerName = soh.Customer.Name,
                    CustomerUrl = $"<a href=\"/Customers/Customers/Edit?id={soh.CustomerId}\">{soh.Customer.Name}</a>",
                    WarehouseInstructions = soh.WarehouseInstructions,
                    OrderDateISO = getFieldValueISODate(soh.OrderDate),
                    DeliveryWindowOpenISO = getFieldValueISODate(soh.DeliveryWindowOpen),
                    DeliveryWindowCloseISO = getFieldValueISODate(soh.DeliveryWindowClose),
                    SalesPersonName = (soh.User_SalesPerson == null ? "" : (soh.User_SalesPerson.FirstName + " " + soh.User_SalesPerson.LastName).Trim()),
                    FreightCarrier = (soh.FreightCarrier == null ? "" : soh.FreightCarrier.FreightCarrier1)
                };
                model.Items.Add(item);
            }
            return model;
        }

        #endregion
    }
}
