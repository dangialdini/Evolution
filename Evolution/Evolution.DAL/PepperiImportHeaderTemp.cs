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
    
    public partial class PepperiImportHeaderTemp
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public PepperiImportHeaderTemp()
        {
            this.PepperiImportDetailTemps = new HashSet<PepperiImportDetailTemp>();
        }
    
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public Nullable<int> WrntyId { get; set; }
        public string OrderType { get; set; }
        public string Status { get; set; }
        public Nullable<System.DateTimeOffset> CreationDateTime { get; set; }
        public Nullable<System.DateTimeOffset> ModificationDateTime { get; set; }
        public Nullable<System.DateTimeOffset> ActionDateTime { get; set; }
        public Nullable<System.DateTimeOffset> DeliveryDate { get; set; }
        public string Remark { get; set; }
        public string CatalogId { get; set; }
        public string CatalogDescription { get; set; }
        public Nullable<byte> CatalogPriceFactor { get; set; }
        public Nullable<System.DateTimeOffset> CatalogExpirationDate { get; set; }
        public string AgentName { get; set; }
        public Nullable<long> AgentExternalId { get; set; }
        public string AgentEmail { get; set; }
        public Nullable<long> AccountWrntyId { get; set; }
        public Nullable<long> AccountExternalId { get; set; }
        public Nullable<System.DateTimeOffset> AccountCreationDate { get; set; }
        public string AccountName { get; set; }
        public string AccountPhone { get; set; }
        public string AccountMobile { get; set; }
        public string AccountFax { get; set; }
        public string AccountEmail { get; set; }
        public string AccountStreet { get; set; }
        public string AccountCity { get; set; }
        public string AccountState { get; set; }
        public Nullable<long> AccountCountryId { get; set; }
        public string AccountCountry { get; set; }
        public string AccountZipCode { get; set; }
        public string AccountPriceLevelName { get; set; }
        public string BillToName { get; set; }
        public string BillToStreet { get; set; }
        public string BillToCity { get; set; }
        public string BillToState { get; set; }
        public Nullable<long> BillToCountryId { get; set; }
        public string BillToCountry { get; set; }
        public string BillToZipCode { get; set; }
        public string BillToPhone { get; set; }
        public Nullable<long> ShipToExternalId { get; set; }
        public string ShipToName { get; set; }
        public string ShipToStreet { get; set; }
        public string ShipToCity { get; set; }
        public string ShipToState { get; set; }
        public Nullable<long> ShipToCountryId { get; set; }
        public string ShipToCountry { get; set; }
        public string ShipToZipCode { get; set; }
        public string ShipToPhone { get; set; }
        public string Currency { get; set; }
        public Nullable<int> TotalItemsCount { get; set; }
        public Nullable<decimal> SubTotal { get; set; }
        public Nullable<decimal> SubTotalAfterItemsDiscount { get; set; }
        public Nullable<decimal> GrandTotal { get; set; }
        public Nullable<decimal> DiscountPercentage { get; set; }
        public Nullable<decimal> TaxPercentage { get; set; }
        public Nullable<decimal> TSAGST { get; set; }
        public Nullable<System.DateTimeOffset> TSADeliveryWindowOpen { get; set; }
        public Nullable<System.DateTimeOffset> TSADeliveryWindowClose { get; set; }
        public string TSAOrderTakenBy { get; set; }
        public Nullable<decimal> TSATaxRate { get; set; }
        public Nullable<decimal> TSASubTotalBeforeTax { get; set; }
        public Nullable<decimal> TSAGrandTotal { get; set; }
        public Nullable<long> SalespersonId { get; set; }
        public string Filespec { get; set; }
        public Nullable<bool> IsNewCustomer { get; set; }
        public Nullable<int> CustomerId { get; set; }
        public Nullable<int> OrderNumber { get; set; }
        public Nullable<int> SOStatus { get; set; }
        public Nullable<int> SOSubStatus { get; set; }
        public Nullable<int> LocationId { get; set; }
        public bool IsConfirmedAddress { get; set; }
        public string SignedBy { get; set; }
        public Nullable<int> MethodSignedId { get; set; }
        public bool IsMSQProblem { get; set; }
        public bool IsOverrideMSQ { get; set; }
        public Nullable<int> BrandCategoryId { get; set; }
        public Nullable<int> SourceId { get; set; }
        public Nullable<int> NextActionId { get; set; }
    
        public virtual Company Company { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PepperiImportDetailTemp> PepperiImportDetailTemps { get; set; }
        public virtual Location Location { get; set; }
        public virtual MethodSigned MethodSigned { get; set; }
        public virtual SalesOrderHeaderStatu SalesOrderHeaderStatu { get; set; }
        public virtual SalesOrderHeaderSubStatu SalesOrderHeaderSubStatu { get; set; }
    }
}
