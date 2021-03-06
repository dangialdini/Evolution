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
    
    public partial class PickDetail
    {
        public int Id { get; set; }
        public Nullable<int> PickHeaderId { get; set; }
        public Nullable<int> ProductId { get; set; }
        public Nullable<int> QtyToPick { get; set; }
        public Nullable<int> QtyPicked { get; set; }
        public Nullable<int> PickDetailStatusId { get; set; }
        public Nullable<int> SalesOrderDetailId { get; set; }
        public Nullable<int> PickLocationId { get; set; }
        public bool IsReportedToWebsite { get; set; }
        public Nullable<int> OriginalRowId { get; set; }
        public int CompanyId { get; set; }
    
        public virtual Location Location { get; set; }
        public virtual PickDetailStatu PickDetailStatu { get; set; }
        public virtual PickHeader PickHeader { get; set; }
        public virtual Product Product { get; set; }
        public virtual Company Company { get; set; }
        public virtual SalesOrderDetail SalesOrderDetail { get; set; }
    }
}
