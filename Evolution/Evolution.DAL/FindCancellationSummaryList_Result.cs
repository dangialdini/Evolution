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
    
    public partial class FindCancellationSummaryList_Result
    {
        public int CustomerId { get; set; }
        public int SohId { get; set; }
        public int SodId { get; set; }
        public Nullable<int> ProductId { get; set; }
        public Nullable<int> LocationId { get; set; }
        public Nullable<int> AllocationId { get; set; }
        public Nullable<int> PurchaseOrderDetailId { get; set; }
        public int UserId { get; set; }
        public Nullable<System.DateTimeOffset> DeliveryWindowOpen { get; set; }
        public string CustomerName { get; set; }
        public Nullable<int> SaleOrderNo { get; set; }
        public string ItemNumber { get; set; }
        public string ItemName { get; set; }
        public string LocationName { get; set; }
        public Nullable<int> OrderQty { get; set; }
        public Nullable<int> AllocQty { get; set; }
        public string AccountManager { get; set; }
        public string Email { get; set; }
    }
}
