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
    
    public partial class ProductIP
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int MarketId { get; set; }
    
        public virtual LOVItem LOVItem_Market { get; set; }
        public virtual Product Product { get; set; }
    }
}
