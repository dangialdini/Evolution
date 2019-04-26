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

        public List<ListItemModel> FindMethodSignedListItemModel() {
            return db.FindMethodSigneds()
                     .Select(ms => new ListItemModel {
                         Id = ms.Id.ToString(),
                         Text = ms.MethodSigned1,
                         ImageURL = ""
                     })
                     .ToList();
        }

        public MethodSignedModel FindMethodSignedModel(string methodSigned) {
            var dbMethod = db.FindMethodSigned(methodSigned);            
            return new MethodSignedModel {
                Id = dbMethod.Id,
                MethodSigned = dbMethod.MethodSigned1,
                Category = dbMethod.Category,
                Enabled = dbMethod.Enabled,
            };
        }

        #endregion
    }
}
