using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Evolution.Enumerations {
    public enum SalesOrderLineStatus {
        // This list must match the SalesOrderLineStatus table
        Unpicked = 1,
        PickingInProgress = 2,
        Complete = 4,
        Cancelled = 5,
        SentForPicking = 6
    }
}
