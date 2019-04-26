using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Evolution.DAL;
using Evolution.Models.Models;
using Evolution.Models.ViewModels;
using Evolution.CommonService;
using Evolution.Enumerations;
using Evolution.Extensions;
using Evolution.Resources;

namespace Evolution.SalesService {
    public partial class SalesService {
        public TransactionDrillDownListModel FindTransactionDrillDown(SalesOrderHeaderModel soh,
                                                                      int index = 0, int pageNo = 1, int pageSize = Int32.MaxValue) {
            var model = new TransactionDrillDownListModel();

            model.GridIndex = index;
            var allItems = db.FindTransactionDrillDown(soh.Id, soh.LocationId)
                             .ToList();

            model.TotalRecords = allItems.Count();
            foreach (var item in allItems.Skip((pageNo - 1) * pageSize)
                                         .Take(pageSize)) {
                var newItem = Mapper.Map<FindTransactionDrillDown_Result, TransactionDrillDownModel>(item);
                model.Items.Add(newItem);
            }

            return model;
        }
    }
}
