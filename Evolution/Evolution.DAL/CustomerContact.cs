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
    
    public partial class CustomerContact
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public CustomerContact()
        {
            this.CreditClaimHeaders = new HashSet<CreditClaimHeader>();
            this.MarketingGroupSubscriptions = new HashSet<MarketingGroupSubscription>();
        }
    
        public int Id { get; set; }
        public Nullable<int> OriginalRowId { get; set; }
        public int CompanyId { get; set; }
        public int CustomerId { get; set; }
        public string ContactFirstname { get; set; }
        public string ContactSurname { get; set; }
        public string ContactKnownAs { get; set; }
        public string ContactEmail { get; set; }
        public string Position { get; set; }
        public string ContactSalutation { get; set; }
        public string ContactPhone1 { get; set; }
        public string ContactPhone2 { get; set; }
        public string ContactPhone3 { get; set; }
        public string ContactFax { get; set; }
        public string ContactPhoneNotes { get; set; }
        public string ContactNotes { get; set; }
        public bool SendStatement { get; set; }
        public bool SendInvoice { get; set; }
        public bool MailingList { get; set; }
        public bool PrimaryContact { get; set; }
        public bool ReceiveCatalog { get; set; }
        public bool Enabled { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<CreditClaimHeader> CreditClaimHeaders { get; set; }
        public virtual Customer Customer { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<MarketingGroupSubscription> MarketingGroupSubscriptions { get; set; }
        public virtual Company Company { get; set; }
    }
}
