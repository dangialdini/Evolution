using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Evolution.DAL;
using Evolution.Models.Models;
using Evolution.CommonService;
using Evolution.Enumerations;
using Evolution.Resources;
using AutoMapper;

namespace Evolution.LookupService {
    public partial class LookupService {

        #region Public members    

        public List<ListItemModel> FindSalesOrderHeaderSubStatusListItemModel() {
            return db.FindSalesOrderHeaderSubStatuses()
                     .Select(sohss => new ListItemModel {
                         Id = sohss.Id.ToString(),
                         Text = sohss.StatusName,
                         ImageURL = ""
                     })
                     .ToList();
        }

        #endregion
    }
}
