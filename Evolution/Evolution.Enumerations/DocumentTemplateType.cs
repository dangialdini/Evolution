using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Evolution.Enumerations {
    public enum DocumentTemplateType {
        None = 0,
        PurchaseOrder = 1,
        SaleCancellation = 2,
        OrderConfirmation = 3,
        ProFormaInvoice = 4,
        ConfirmedOrder = 5,
        SendPOtoWarehouse = 6,
        SendPOtoSupplier = 7,
        SendPOtoFreightForwarder = 8,
        PackingSlip = 9,
        PackingSlipRetail = 10
    }
}
