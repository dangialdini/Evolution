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

        public List<ListItemModel> FindLogSectionListItemModel(bool bInsertAll = false) {
            List<ListItemModel> model = new List<ListItemModel>();

            string[] logSections = EvolutionResources.strLogSection.Split('|');

            foreach (string logSection in logSections) {
                string[] keyValue = logSection.Split('=');
                if(keyValue.Length == 1) {
                    model.Add(new ListItemModel(keyValue[0], keyValue[0]));
                } else {
                    if((bInsertAll && keyValue[0] == "-1") || keyValue[0] != "-1") {
                        model.Add(new ListItemModel(keyValue[1], keyValue[0]));
                    }
                }
            }
            return model;
        }

        #endregion
    }
}

