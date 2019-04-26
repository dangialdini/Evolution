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

namespace Evolution.LookupService {
    public partial class LookupService {

        #region Public members    

        public List<ListItemModel> FindSaleNextActionListItemModel(bool bInsertBlack = false) {
            var list = db.FindSaleNextActions()
                     .Select(sna => new ListItemModel {
                         Id = sna.Id.ToString(),
                         Text = sna.NextActionDescription,
                         ImageURL = ""
                     })
                     .ToList();
            if (bInsertBlack) list.Insert(0, new ListItemModel("", "0"));
            return list;
        }

        public int? FindSaleNextActionId(Enumerations.SaleNextAction nextAction) {
            var temp = db.FindSaleNextActions()
                         .Where(sna => sna.Id == (int)nextAction)
                         .FirstOrDefault();
            if (temp == null) {
                return null;
            } else {
                return temp.Id;
            }
        }
        
        #endregion
    }
}
