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

        public List<ListItemModel> FindFileTransferTypeListItemModel() {
            List<ListItemModel> model = new List<ListItemModel>();

            foreach (string transferTypeName in Enum.GetNames(typeof(FileTransferType))) {
                model.Add(new ListItemModel(transferTypeName, ((int)Enum.Parse(typeof(FileTransferType), transferTypeName)).ToString()));
            }
            return model;
        }

        #endregion
    }
}
