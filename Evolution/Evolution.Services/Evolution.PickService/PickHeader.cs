using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Evolution.CommonService;
using Evolution.DAL;
using Evolution.Models.Models;
using Evolution.Extensions;
using Evolution.Enumerations;
using Evolution.Resources;

namespace Evolution.PickService {
    public partial class PickService {

        public PicksListModel FindPicksListModel(int companyId,
                                                 DateTimeOffset? dateFrom, DateTimeOffset? dateTo,
                                                 int index = 0, int pageNo = 1, int pageSize = Int32.MaxValue, string search = "", 
                                                 string sortColumn = "", SortOrder sortOrder = SortOrder.Asc) {
            var model = new PicksListModel();
            int intValue = search.ParseInt();

            // Do a case-sensitive search
            model.GridIndex = index;
            var allItems = db.FindPickHeaders(companyId, sortColumn, sortOrder)
                                .Where(ph => ((dateFrom == null || ph.STWDate > dateFrom) && (dateTo == null || ph.STWDate < dateTo)) &&
                                             string.IsNullOrEmpty(search) ||
                                             (ph.InvoiceNumber != null && ph.InvoiceNumber.ToString().ToLower().Contains(search.ToLower())) ||
                                             (ph.Customer.Name != null && ph.Customer.Name.ToLower().Contains(search.ToLower())) ||
                                             (ph.PickDetails.Where(pd => (pd.SalesOrderDetail.SalesOrderHeader.CustPO != null && pd.SalesOrderDetail.SalesOrderHeader.CustPO.ToLower().Contains(search.ToLower())) ||
                                                                         (pd.SalesOrderDetail.SalesOrderHeader.OrderNumber.ToString().ToLower().Contains(search.ToLower())) ||
                                                                         (pd.PickHeader.Id == intValue)).Count() > 0))
                                .ToList();
            
            model.TotalRecords = allItems.Count();
            foreach(var item in allItems.Skip((pageNo - 1) * pageSize)
                                        .Take(pageSize)) {
                model.Items.Add(mapToModel(item));
            }
            return model;
        }

        public PickHeaderModel mapToModel(PickHeader pick) {
            PickHeaderModel newItem = Mapper.Map<PickHeader, PickHeaderModel>(pick);

            if (pick.Customer != null) newItem.CustomerName = pick.Customer.Name;
            if (pick.PickStatusId != null) newItem.Status = db.FindPickStatus(pick.PickStatusId.Value).StatusName;
            if (pick.LocationId != null) newItem.LocationName = db.FindLocation(pick.LocationId.Value).LocationName;
            if (pick.ShipCountryId != null) newItem.ShipCountry = db.FindCountry(pick.ShipCountryId.Value).CountryName;

            return newItem;
        }

        public void DeletePick(PickHeaderModel pick) {
            db.DeletePickHeader(pick.Id);       // Also deletes the details
        }
    }
}
