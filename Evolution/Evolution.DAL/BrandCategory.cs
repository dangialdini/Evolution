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
    
    public partial class BrandCategory
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public BrandCategory()
        {
            this.BrandBrandCategories = new HashSet<BrandBrandCategory>();
            this.BrandCategorySalesPersons = new HashSet<BrandCategorySalesPerson>();
            this.CompanyBrandCategories = new HashSet<CompanyBrandCategory>();
            this.PurchaseOrderHeaders = new HashSet<PurchaseOrderHeader>();
            this.PurchaseOrderHeaderTemps = new HashSet<PurchaseOrderHeaderTemp>();
            this.Users = new HashSet<User>();
            this.SalesOrderHeaderTemps = new HashSet<SalesOrderHeaderTemp>();
            this.SalesOrderHeaders = new HashSet<SalesOrderHeader>();
            this.NuOrderImportTemps = new HashSet<NuOrderImportTemp>();
            this.ShopifyImportHeaderTemps = new HashSet<ShopifyImportHeaderTemp>();
        }
    
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public string CategoryName { get; set; }
        public bool Enabled { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<BrandBrandCategory> BrandBrandCategories { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<BrandCategorySalesPerson> BrandCategorySalesPersons { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<CompanyBrandCategory> CompanyBrandCategories { get; set; }
        public virtual Company Company { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PurchaseOrderHeader> PurchaseOrderHeaders { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PurchaseOrderHeaderTemp> PurchaseOrderHeaderTemps { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<User> Users { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SalesOrderHeaderTemp> SalesOrderHeaderTemps { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SalesOrderHeader> SalesOrderHeaders { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<NuOrderImportTemp> NuOrderImportTemps { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ShopifyImportHeaderTemp> ShopifyImportHeaderTemps { get; set; }
    }
}
