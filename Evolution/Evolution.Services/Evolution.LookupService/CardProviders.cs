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

        public List<ListItemModel> FindCreditCardProviders() {
            return db.FindCreditCardProviders()
                     .Select(ccp => new ListItemModel {
                         Id = ccp.Id.ToString(),
                         Text = ccp.ProviderName,
                         ImageURL = ccp.IconFile
                     })
                     .ToList();
        }

        #endregion
    }
}
