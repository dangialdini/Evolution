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
    
    public partial class Task
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public System.DateTimeOffset CreatedDate { get; set; }
        public int TaskTypeId { get; set; }
        public int BusinessUnitId { get; set; }
        public int UserId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public Nullable<System.DateTimeOffset> DueDate { get; set; }
        public Nullable<System.DateTimeOffset> CompletedDate { get; set; }
        public Nullable<int> CompletedById { get; set; }
        public int StatusId { get; set; }
        public Nullable<int> CustomerId { get; set; }
        public bool Enabled { get; set; }
    
        public virtual Company Company { get; set; }
        public virtual LOVItem LOVItem_Status { get; set; }
        public virtual User User_Assignee { get; set; }
        public virtual User User_CompletedBy { get; set; }
        public virtual LOVItem LOVItem_TaskType { get; set; }
        public virtual Customer Customer { get; set; }
        public virtual LOVItem LOVItem_BusinessUnit { get; set; }
    }
}
