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
    
    public partial class CustomerDefault
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public Nullable<int> CountryId { get; set; }
        public string Postcode { get; set; }
        public Nullable<int> CurrencyId { get; set; }
        public Nullable<int> TermId { get; set; }
        public Nullable<int> PriceLevelId { get; set; }
        public Nullable<int> TaxCodeId { get; set; }
        public Nullable<int> TaxCodeWithoutTaxId { get; set; }
        public Nullable<decimal> CreditLimit { get; set; }
        public Nullable<decimal> VolumeDiscount { get; set; }
        public Nullable<int> PrintedForm { get; set; }
        public Nullable<int> ShippingMethodId { get; set; }
        public Nullable<int> SalespersonId { get; set; }
        public Nullable<int> FreightCarrierId { get; set; }
        public bool IsManualFreight { get; set; }
        public bool OnHold { get; set; }
        public bool SendInvoices { get; set; }
        public bool EmailAcctMgrOnSaleChange { get; set; }
        public bool EmailAcctMgrOnLinkedPurchaseChange { get; set; }
        public bool RequireAuthorisation4OrderQtyChange { get; set; }
        public bool AllowSalesInNonMSQMultiples { get; set; }
        public string ProductLabelName { get; set; }
        public bool SendPOSFile { get; set; }
        public bool RemoveCustNameFromAddressWhenDrop { get; set; }
        public bool ExcludeFromSalesGraphs { get; set; }
        public Nullable<int> CustomerTypeId { get; set; }
        public Nullable<decimal> FreightRate { get; set; }
        public Nullable<decimal> MinFreightPerOrder { get; set; }
        public Nullable<decimal> MinFreightThreshold { get; set; }
        public Nullable<decimal> FreightWhenBelowThreshold { get; set; }
        public Nullable<byte> DefaultTemplateType { get; set; }
        public Nullable<int> DefaultTemplateId { get; set; }
        public bool Enabled { get; set; }
    
        public virtual Company Company { get; set; }
        public virtual Country Country { get; set; }
        public virtual Currency Currency { get; set; }
        public virtual FreightCarrier FreightCarrier { get; set; }
        public virtual PaymentTerm PaymentTerm { get; set; }
        public virtual TaxCode TaxCode { get; set; }
        public virtual TaxCode TaxCode_IfNoTaxId { get; set; }
        public virtual User User_SalesPerson { get; set; }
        public virtual PriceLevel PriceLevel { get; set; }
        public virtual DocumentTemplate DocumentTemplate { get; set; }
        public virtual LOVItem LOVItem_CustomerType { get; set; }
    }
}
