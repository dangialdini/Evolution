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
    
    public partial class Supplier
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Supplier()
        {
            this.SupplierAttachments = new HashSet<SupplierAttachment>();
            this.SupplierNotes = new HashSet<SupplierNote>();
            this.SupplierPayments = new HashSet<SupplierPayment>();
            this.PurchaseOrderHeaders = new HashSet<PurchaseOrderHeader>();
            this.PurchaseOrderHeaderTemps = new HashSet<PurchaseOrderHeaderTemp>();
            this.ShipmentContents = new HashSet<ShipmentContent>();
            this.FileImportRows = new HashSet<FileImportRow>();
            this.Products = new HashSet<Product>();
            this.SupplierAddresses = new HashSet<SupplierAddress>();
        }
    
        public int Id { get; set; }
        public Nullable<int> OriginalRowId { get; set; }
        public Nullable<int> CompanyId { get; set; }
        public System.DateTimeOffset CreatedDate { get; set; }
        public Nullable<int> CreatedById { get; set; }
        public string Name { get; set; }
        public Nullable<int> CurrencyId { get; set; }
        public string Notes { get; set; }
        public Nullable<int> SupplierTermId { get; set; }
        public Nullable<int> TaxCodeId { get; set; }
        public string CancelMessage { get; set; }
        public Nullable<int> PortId { get; set; }
        public Nullable<int> ShipMethodId { get; set; }
        public bool IsProductSupplier { get; set; }
        public string ContactName { get; set; }
        public string Phone1 { get; set; }
        public string Phone2 { get; set; }
        public Nullable<int> CommercialTermsId { get; set; }
        public string Email { get; set; }
        public string URL { get; set; }
        public bool IsVerticalSupplier { get; set; }
        public bool IsManufacturer { get; set; }
        public bool IsTrader { get; set; }
        public bool IsAgent { get; set; }
        public Nullable<int> FreightForwarderId_AU { get; set; }
        public Nullable<int> FreightForwarderId_DTC { get; set; }
        public Nullable<int> FreightForwarderId_UK { get; set; }
        public Nullable<int> FreightForwarderId_US { get; set; }
        public Nullable<int> PurchaserId { get; set; }
        public bool Enabled { get; set; }
    
        public virtual LOVItem LOVItem { get; set; }
        public virtual LOVItem LOVItem1 { get; set; }
        public virtual TaxCode TaxCode { get; set; }
        public virtual User User { get; set; }
        public virtual User User1 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SupplierAttachment> SupplierAttachments { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SupplierNote> SupplierNotes { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SupplierPayment> SupplierPayments { get; set; }
        public virtual Company Company { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PurchaseOrderHeader> PurchaseOrderHeaders { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PurchaseOrderHeaderTemp> PurchaseOrderHeaderTemps { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ShipmentContent> ShipmentContents { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<FileImportRow> FileImportRows { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Product> Products { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SupplierAddress> SupplierAddresses { get; set; }
    }
}
