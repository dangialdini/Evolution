using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Evolution.Enumerations {
    public enum MessageTemplateType {
        None = 0,
        SystemError = 1,
        EMailHeader = 2,
        EMailFooter = 3,
        TestMessage = 4,
        PurchaseOrderNotificationFF = 5,
        PurchaseOrderNotificationSupplier = 6,
        UnpackSlipNotification = 7,
        UndeliveredPurchaseNotification = 8,
        MSQChangeNotification = 9
    }
}
