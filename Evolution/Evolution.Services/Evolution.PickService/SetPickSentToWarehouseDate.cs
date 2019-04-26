using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Evolution.Models.Models;

namespace Evolution.PickService {
    public partial class PickService : CommonService.CommonService {
        public void SetPickSentToWarehouseDate(PickHeaderModel pick, DateTimeOffset dt) {
            var p = db.FindPickHeader(pick.Id);
            if (p != null) {
                pick.STWDate = p.STWDate = dt;
                db.InsertOrUpdatePickHeader(p);
            }
        }
    }
}
