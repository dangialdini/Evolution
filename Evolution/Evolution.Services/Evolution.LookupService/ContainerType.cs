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

        public List<ListItemModel> FindContainerTypeListItemModel() {
            return db.FindContainerTypes()
                     .Select(ct => new ListItemModel {
                         Id = ct.Id.ToString(),
                         Text = ct.ContainerType1,
                         ImageURL = ""
                     })
                     .ToList();
        }

        #endregion
    }
}
