namespace Evolution.Enumerations {
    // The values here must correspond with values in the SalesOrderHeaderStatus table
    public enum SalesOrderHeaderStatus {
        Quote = 1,
        UnconfirmedOrder = 2,
        ConfirmedOrder = 3,
        Cancelled = 7,
        HeldOrder = 9
    }
}
