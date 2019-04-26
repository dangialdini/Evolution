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
    
    public partial class SupplierTerm
    {
        public int Id { get; set; }
        public Nullable<int> OriginalRowId { get; set; }
        public int CompanyId { get; set; }
        public string SupplierTermName { get; set; }
        public Nullable<decimal> ValueBeforeProduction { get; set; }
        public Nullable<decimal> ValueOnBOL { get; set; }
        public Nullable<decimal> ValueBeforeLoading { get; set; }
        public Nullable<decimal> ValueAfterInvoice { get; set; }
        public Nullable<int> DaysAfterInvoice { get; set; }
        public Nullable<decimal> ValueAfterProduction { get; set; }
        public bool Enabled { get; set; }
    
        public virtual Company Company { get; set; }
    }
}