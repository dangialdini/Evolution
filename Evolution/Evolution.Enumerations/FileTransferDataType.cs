using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Evolution.Enumerations {
    public enum FileTransferDataType {
        // This list must be synchronised with the 'File Transfer Data Type' LOV
        WarehousePick = 1,
        WarehousePurchase = 2,
        WarehouseCompletedPick = 3,
        WarehouseCompletedPurchase = 4,
        EDI = 5,
        StockAdjustment = 6,
        Buyer = 7,
        Company = 8,
        Inventory = 9,
        Shipment = 10,
        Product = 11,
        FreightForwarderPurchase = 12,
        WarehouseUnpackSlip = 13
    }
}
