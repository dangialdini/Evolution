//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Evolution.DAL
{
    using System;
    
    public partial class FindTransactionDrillDown_Result
    {
        public int Id { get; set; }
        public Nullable<int> TransactionLineID { get; set; }
        public Nullable<int> TransactionLineNumber { get; set; }
        public Nullable<int> ProductId { get; set; }
        public string ItemNumber { get; set; }
        public string ItemName { get; set; }
        public Nullable<int> QtyOrdered { get; set; }
        public Nullable<double> ValueOrdered { get; set; }
        public Nullable<int> NumberInStock { get; set; }
        public Nullable<int> QtyAvailableNow { get; set; }
        public Nullable<double> ValueAvailableNow { get; set; }
        public Nullable<double> PercentAvailableNow { get; set; }
        public Nullable<int> QtyAvailableAtWished { get; set; }
        public Nullable<double> ValueAvailableAtWished { get; set; }
        public Nullable<double> PercentAvailableAtWished { get; set; }
        public Nullable<int> QtyAvailableEver { get; set; }
        public Nullable<double> ValueAvailableEver { get; set; }
        public Nullable<double> PercentAvailableEver { get; set; }
        public Nullable<int> QtyAvailableRuthless { get; set; }
        public Nullable<double> ValueAvailableRuthless { get; set; }
        public Nullable<double> PercentAvailableRuthless { get; set; }
        public Nullable<System.DateTimeOffset> ExpectedCompletionDate { get; set; }
        public string NeighbourWarning { get; set; }
    }
}
