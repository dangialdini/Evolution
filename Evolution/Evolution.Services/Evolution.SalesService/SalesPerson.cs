using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Evolution.DAL;
using Evolution.Models.Models;
using Evolution.CommonService;
using Evolution.Resources;
using Evolution.Enumerations;
using AutoMapper;

namespace Evolution.SalesService {
    public partial class SalesService {

        #region Public methods

        public List<ListItemModel> FindSalesPersonListItemModel(CompanyModel company) {
            List<ListItemModel> model = new List<ListItemModel>();
            foreach (var item in db.FindSalesOrderHeaders(company.Id)
                                   .Where(soh => soh.User_SalesPerson != null)
                                   .Select(soh => soh.User_SalesPerson)
                                   .Distinct()) {
                model.Add(new ListItemModel {
                    Id = item.Id.ToString(),
                    Text = db.MakeName(item),
                    ImageURL = ""
                });
            }
            return model.OrderBy(m => m.Text)
                        .ToList();
        }

        #endregion
    }
}
