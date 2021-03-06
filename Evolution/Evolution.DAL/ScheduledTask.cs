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
    
    public partial class ScheduledTask
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public ScheduledTask()
        {
            this.ScheduledTaskLogs = new HashSet<ScheduledTaskLog>();
        }
    
        public int Id { get; set; }
        public string TaskName { get; set; }
        public string CurrentState { get; set; }
        public Nullable<System.DateTimeOffset> LastRun { get; set; }
        public string Parameters { get; set; }
        public string CmdParameter { get; set; }
        public bool Enabled { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ScheduledTaskLog> ScheduledTaskLogs { get; set; }
    }
}
