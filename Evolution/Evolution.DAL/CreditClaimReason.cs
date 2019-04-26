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
    
    public partial class CreditClaimReason
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public CreditClaimReason()
        {
            this.CreditClaimHeaders = new HashSet<CreditClaimHeader>();
            this.CreditClaimLines = new HashSet<CreditClaimLine>();
        }
    
        public int Id { get; set; }
        public int OriginalRowId { get; set; }
        public int CompanyId { get; set; }
        public string ReasonName { get; set; }
        public Nullable<int> DefaultItemConditionId { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<CreditClaimHeader> CreditClaimHeaders { get; set; }
        public virtual CreditClaimItemCondition CreditClaimItemCondition { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<CreditClaimLine> CreditClaimLines { get; set; }
        public virtual Company Company { get; set; }
    }
}
