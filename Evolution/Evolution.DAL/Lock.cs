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
    
    public partial class Lock
    {
        public int Id { get; set; }
        public string TableName { get; set; }
        public int LockedRowId { get; set; }
        public System.DateTimeOffset TimeStamp { get; set; }
        public string LockGuid { get; set; }
    }
}
