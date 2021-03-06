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
    
    public partial class SupplierAccountsQueue
    {
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
    
        public virtual Company Company { get; set; }
        public virtual LOVItem LOVItem { get; set; }
        public virtual LOVItem LOVItem1 { get; set; }
        public virtual TaxCode TaxCode { get; set; }
        public virtual User User { get; set; }
    }
}
