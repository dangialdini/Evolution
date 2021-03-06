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
    using System.Collections.Generic;
    
    public partial class PurchaseOrderHeaderTemp
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public PurchaseOrderHeaderTemp()
        {
            this.PurchaseOrderDetailTemps = new HashSet<PurchaseOrderDetailTemp>();
        }
    
        public int Id { get; set; }
        public Nullable<int> OriginalRowId { get; set; }
        public int CompanyId { get; set; }
        public int UserId { get; set; }
        public Nullable<int> dSource { get; set; }
        public Nullable<int> SourceId { get; set; }
        public Nullable<int> SupplierId { get; set; }
        public Nullable<decimal> OrderNumber { get; set; }
        public string SupplierInv { get; set; }
        public Nullable<System.DateTimeOffset> OrderDate { get; set; }
        public Nullable<System.DateTimeOffset> RequiredDate { get; set; }
        public Nullable<System.DateTimeOffset> CompletedDate { get; set; }
        public string ShipAddress1 { get; set; }
        public string ShipAddress2 { get; set; }
        public string ShipAddress3 { get; set; }
        public string ShipAddress4 { get; set; }
        public string ShipSuburb { get; set; }
        public string ShipState { get; set; }
        public string ShipPostcode { get; set; }
        public int POStatus { get; set; }
        public Nullable<int> SalespersonId { get; set; }
        public string OrderMemo { get; set; }
        public string OrderComment { get; set; }
        public Nullable<int> ReferralSource { get; set; }
        public Nullable<int> LocationId { get; set; }
        public Nullable<int> CurrencyId { get; set; }
        public Nullable<int> PaymentTermsId { get; set; }
        public Nullable<decimal> OrderBalance { get; set; }
        public Nullable<int> UnpackedBy { get; set; }
        public Nullable<int> EnteredBy { get; set; }
        public Nullable<System.DateTimeOffset> UnpackCommenced { get; set; }
        public Nullable<System.DateTimeOffset> UnpackCompleted { get; set; }
        public Nullable<System.DateTimeOffset> ReceiptedIntoStock { get; set; }
        public Nullable<System.DateTimeOffset> RequiredShipDate { get; set; }
        public Nullable<System.DateTimeOffset> CancelDate { get; set; }
        public string CancelMessage { get; set; }
        public bool StockOrder { get; set; }
        public Nullable<int> CommercialTermsId { get; set; }
        public Nullable<int> FreightForwarderId { get; set; }
        public Nullable<int> PortId { get; set; }
        public Nullable<int> ShipMethodId { get; set; }
        public Nullable<System.DateTimeOffset> RealisticRequiredDate { get; set; }
        public Nullable<int> StockTransferNo { get; set; }
        public Nullable<System.DateTimeOffset> DateSentToWhs { get; set; }
        public Nullable<System.DateTimeOffset> DateSentToFF { get; set; }
        public Nullable<double> ExchangeRate { get; set; }
        public Nullable<System.DateTimeOffset> RequiredDate_Original { get; set; }
        public Nullable<System.DateTimeOffset> DatePOSentToSupplier { get; set; }
        public Nullable<System.DateTimeOffset> DateOrderConfirmed { get; set; }
        public string OrderConfirmationNo { get; set; }
        public Nullable<System.DateTimeOffset> SupplierInvoiceDate { get; set; }
        public bool IsFirstOrder { get; set; }
        public Nullable<int> PortArrivalId { get; set; }
        public bool IsDeposit2BePaid { get; set; }
        public Nullable<System.DateTimeOffset> RequiredShipDate_Original { get; set; }
        public string LateReason1 { get; set; }
        public Nullable<float> LatenessFaultPercentage1 { get; set; }
        public string LateReason2 { get; set; }
        public Nullable<float> LatenessFaultPercentage2 { get; set; }
        public Nullable<int> ContainerTypeId { get; set; }
        public Nullable<int> BrandCategoryId { get; set; }
    
        public virtual BrandCategory BrandCategory { get; set; }
        public virtual Company Company { get; set; }
        public virtual ContainerType ContainerType { get; set; }
        public virtual Currency Currency { get; set; }
        public virtual FreightForwarder FreightForwarder { get; set; }
        public virtual Location Location { get; set; }
        public virtual LOVItem LOVItem_CommercialTerms { get; set; }
        public virtual LOVItem LOVItem_ShipMethod { get; set; }
        public virtual PaymentTerm PaymentTerm { get; set; }
        public virtual Port Port_PortArrival { get; set; }
        public virtual Port Port_Port { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PurchaseOrderDetailTemp> PurchaseOrderDetailTemps { get; set; }
        public virtual Supplier Supplier { get; set; }
        public virtual User User_EnteredBy { get; set; }
        public virtual User User_SalesPerson { get; set; }
        public virtual User User_UnpackedBy { get; set; }
        public virtual User User_User { get; set; }
    }
}
