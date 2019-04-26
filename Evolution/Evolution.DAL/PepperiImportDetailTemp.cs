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
    
    public partial class PepperiImportDetailTemp
    {
        public int Id { get; set; }
        public int PepperiImportHeaderTempId { get; set; }
        public Nullable<long> ItemWrntyId { get; set; }
        public string ItemExternalId { get; set; }
        public string ItemMainCategory { get; set; }
        public string ItemMainCategoryCode { get; set; }
        public string ItemName { get; set; }
        public Nullable<decimal> ItemPrice { get; set; }
        public Nullable<int> ItemInStockQuantity { get; set; }
        public Nullable<System.DateTimeOffset> TSANextAvailableDate { get; set; }
        public Nullable<int> TSATotalAvailable { get; set; }
        public string TSADuePDF { get; set; }
        public Nullable<decimal> TSALineAmount { get; set; }
        public Nullable<int> UnitsQuantity { get; set; }
        public Nullable<decimal> UnitPrice { get; set; }
        public Nullable<decimal> UnitDiscountPercentage { get; set; }
        public Nullable<decimal> UnitPriceAfterDiscount { get; set; }
        public Nullable<decimal> TotalUnitsPriceAfterDiscount { get; set; }
        public Nullable<System.DateTimeOffset> DeliveryDate { get; set; }
        public Nullable<long> TransactionWrntyId { get; set; }
        public Nullable<long> TransactionExternalId { get; set; }
        public Nullable<int> LineNumber { get; set; }
        public Nullable<int> ProductId { get; set; }
        public Nullable<int> BrandCategoryId { get; set; }
        public Nullable<decimal> DiscountPercent { get; set; }
        public Nullable<int> TaxCodeId { get; set; }
        public int CompanyId { get; set; }
    
        public virtual PepperiImportHeaderTemp PepperiImportHeaderTemp { get; set; }
        public virtual TaxCode TaxCode { get; set; }
    }
}
