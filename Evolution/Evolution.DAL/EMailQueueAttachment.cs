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
    
    public partial class EMailQueueAttachment
    {
        public int Id { get; set; }
        public int EMailQueueId { get; set; }
        public string FileName { get; set; }
        public int OrderNo { get; set; }
    
        public virtual EMailQueue EMailQueue { get; set; }
    }
}