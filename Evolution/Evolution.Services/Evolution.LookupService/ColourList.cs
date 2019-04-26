using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Evolution.DAL;
using Evolution.Models.Models;
using Evolution.Enumerations;

namespace Evolution.LookupService {
    public partial class LookupService {

        #region Public members    

        public List<ListItemModel> FindColourListItemModel() {
            var items = new List<ListItemModel>();

            var lov = db.FindLOV(LOVName.Colours);
            if (lov != null) {
                items = lov.LOVItems
                            .Where(li => li.Enabled == true)
                            .Select(li => new ListItemModel {
                                Id = li.ItemText,
                                Text = li.ItemText,
                                ImageURL = ""
                            })
                            .ToList();
            }
            return items;
        }

        #endregion
    }
}
