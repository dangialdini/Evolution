using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Evolution.Enumerations {
    public enum SaleNextAction {
        ResolveConflict = 1,
        MissedDeliveryWindow = 2,
        GoingtoMissStrictWindowClose = 3,
        AwaitingDeliveryWindow = 4,
        ManualHold = 5,
        AwaitingStock = 6,
        AwaitingPaymentSalesperson = 7,
        AwaitingPaymentAccounts = 8,
        ShipSomething = 9,
        AccountOverLimit = 10,
        None = 99
    }
}
