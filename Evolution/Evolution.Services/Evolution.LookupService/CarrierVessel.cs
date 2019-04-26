using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Evolution.DAL;
using Evolution.Models.Models;
using Evolution.CommonService;
using Evolution.Resources;

namespace Evolution.LookupService {
    public partial class LookupService {

        #region Public members    

        public List<ListItemModel> FindCarrierVesselListItemModel() {
            return db.FindCarrierVessels()
                     .Select(cv => new ListItemModel {
                         Id = cv.Id.ToString(),
                         Text = cv.CarrierVesselName,
                         ImageURL = ""
                     })
                     .ToList();
        }

        #endregion
    }
}
