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
    
    public partial class BrandBrandCategory
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public int BrandCategoryId { get; set; }
        public int BrandId { get; set; }
    
        public virtual Brand Brand { get; set; }
        public virtual BrandCategory BrandCategory { get; set; }
        public virtual Company Company { get; set; }
    }
}
