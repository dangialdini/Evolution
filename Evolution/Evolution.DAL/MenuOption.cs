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
    
    public partial class MenuOption
    {
        public int Id { get; set; }
        public string OptionTag { get; set; }
        public Nullable<int> ParentOptionId { get; set; }
        public int OptionType { get; set; }
        public string OptionText { get; set; }
        public string Tooltip { get; set; }
        public string Image { get; set; }
        public string Alignment { get; set; }
        public Nullable<int> RequiredLogin { get; set; }
        public string RequiredRoles { get; set; }
        public int RequiredObjectFlags { get; set; }
        public string Url { get; set; }
        public string WindowName { get; set; }
        public int OrderNo { get; set; }
        public bool Deletable { get; set; }
        public bool Enabled { get; set; }
    }
}