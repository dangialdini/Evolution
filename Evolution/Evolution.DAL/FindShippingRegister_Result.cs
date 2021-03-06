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
    
    public partial class FindShippingRegister_Result
    {
        public int ShipmentId { get; set; }
        public Nullable<int> ShipmentContentId { get; set; }
        public Nullable<int> PohId { get; set; }
        public Nullable<int> SupplierId { get; set; }
        public string SupplierName { get; set; }
        public Nullable<decimal> OrderNumber { get; set; }
        public string ProductBrand { get; set; }
        public Nullable<double> CBMEstimate { get; set; }
        public Nullable<int> ShippingMethodId { get; set; }
        public string ShippingMethod { get; set; }
        public Nullable<int> PortDepartId { get; set; }
        public string PortDepart { get; set; }
        public Nullable<int> PortArrivalId { get; set; }
        public string PortArrival { get; set; }
        public Nullable<System.DateTimeOffset> DatePreAlertETA { get; set; }
        public Nullable<System.DateTimeOffset> OrderDate { get; set; }
        public Nullable<System.DateTimeOffset> DateDepartSupplier { get; set; }
        public Nullable<System.DateTimeOffset> CancelDate { get; set; }
        public Nullable<int> POStatus { get; set; }
        public string POStatusText { get; set; }
        public Nullable<System.DateTimeOffset> SupplierInvoiceDate { get; set; }
        public Nullable<int> SeasonId { get; set; }
        public string Season { get; set; }
        public Nullable<System.DateTimeOffset> RequiredShipDate { get; set; }
        public Nullable<System.DateTimeOffset> DateExpDel { get; set; }
        public Nullable<System.DateTimeOffset> Date100Shipped { get; set; }
        public Nullable<System.DateTimeOffset> DateWarehouseAccept { get; set; }
        public string CurrentStatus { get; set; }
        public Nullable<int> SalesPersonId { get; set; }
        public string SalesPerson { get; set; }
        public Nullable<int> BrandCategoryID { get; set; }
        public string BrandCategory { get; set; }
    }
}
