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
    
    public partial class MethodSigned
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public MethodSigned()
        {
            this.SalesOrderHeaderTemps = new HashSet<SalesOrderHeaderTemp>();
            this.SalesOrderHeaders = new HashSet<SalesOrderHeader>();
            this.PepperiImportHeaderTemps = new HashSet<PepperiImportHeaderTemp>();
            this.NuOrderImportTemps = new HashSet<NuOrderImportTemp>();
            this.ShopifyImportHeaderTemps = new HashSet<ShopifyImportHeaderTemp>();
        }
    
        public int Id { get; set; }
        public string MethodSigned1 { get; set; }
        public string Category { get; set; }
        public bool Enabled { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SalesOrderHeaderTemp> SalesOrderHeaderTemps { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SalesOrderHeader> SalesOrderHeaders { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PepperiImportHeaderTemp> PepperiImportHeaderTemps { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<NuOrderImportTemp> NuOrderImportTemps { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ShopifyImportHeaderTemp> ShopifyImportHeaderTemps { get; set; }
    }
}
