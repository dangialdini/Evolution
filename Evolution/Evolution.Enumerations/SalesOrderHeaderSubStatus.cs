namespace Evolution.Enumerations {
    public enum SalesOrderHeaderSubStatus {
        // The values here must correspond with values in the SalesOrderHeaderSubStatus table
        Unpicked = 1,
        PartiallyComplete = 2,
        Complete = 4,
        PartiallyCompletePartiallyCancelled = 5,
        Cancelled = 6
    }
}
