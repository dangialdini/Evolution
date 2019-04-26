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

        public List<ListItemModel> FindFTPProtocolListItemModel() {
            List<ListItemModel> model = new List<ListItemModel>();

            foreach (string protocolName in Enum.GetNames(typeof(FTPProtocol))) {
                model.Add(new ListItemModel(protocolName, ((int)Enum.Parse(typeof(FTPProtocol), protocolName)).ToString()));
            }
            return model;
        }

        #endregion
    }
}
