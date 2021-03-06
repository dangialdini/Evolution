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
    
    public partial class CreditCard
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public CreditCard()
        {
            this.SalesOrderHeaderTemps = new HashSet<SalesOrderHeaderTemp>();
            this.SalesOrderHeaders = new HashSet<SalesOrderHeader>();
        }
    
        public int Id { get; set; }
        public int OriginalRowId { get; set; }
        public int CompanyId { get; set; }
        public Nullable<int> CustomerId { get; set; }
        public Nullable<int> CreditCardProviderId { get; set; }
        public string CreditCardNo { get; set; }
        public string NameOnCard { get; set; }
        public string Expiry { get; set; }
        public string CCV { get; set; }
        public string Notes { get; set; }
        public bool Enabled { get; set; }
    
        public virtual Customer Customer { get; set; }
        public virtual Company Company { get; set; }
        public virtual CreditCardProvider CreditCardProvider { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SalesOrderHeaderTemp> SalesOrderHeaderTemps { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SalesOrderHeader> SalesOrderHeaders { get; set; }
    }
}
