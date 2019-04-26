namespace Evolution.Enumerations {
    // The values here must correspond with values in the PurchaseOrderHeaderStatus table
    public enum PurchaseOrderStatus {
        Quote =1,
        OrderPIIssued = 2,
        Closed = 3,
        Landed = 5,
        ReceivedByWarehouse = 6,
        Unpacked = 7,
        OrderGoodsLeftSupplier = 8,
        Cancelled = 9,
        OrderPlanned = 11,
        OrderPOSentToSupplier = 12,
        OrderGoodsOnWater = 13,
        Superceded = 14
    }
}
