using Evolution.Models.Models;
using System.Collections.Generic;

namespace Evolution.LookupService {
    public partial class LookupService {

        #region Public members    

        public List<ListItemModel> FindDateFormatListItemModel() {
            var formatList = new List<ListItemModel>();
            formatList.Add(new ListItemModel { Id = "dd/MM/yyyy", Text = "dd/mm/yyyy", ImageURL = "" });
            formatList.Add(new ListItemModel { Id = "MM/dd/yyyy", Text = "mm/dd/yyyy", ImageURL = "" });
            return formatList;
        }

        #endregion
    }
}
