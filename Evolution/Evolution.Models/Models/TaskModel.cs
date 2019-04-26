using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Evolution.Extensions;
using Evolution.Enumerations;
using Evolution.Resources;

namespace Evolution.Models.Models {
    public class TaskModel {
        public int Id { set; get; } = 0;
        public int CompanyId { set; get; } = 0;
        public DateTimeOffset CreatedDate { set; get; } = DateTimeOffset.Now;
        public int TaskTypeId { set; get; }
        public int BusinessUnitId { set; get; }
        public int UserId { set; get; }
        [Required]
        public string Title { set; get; } = "";
        public string Description { set; get; } = "";
        [Required]
        [Display(Name = "lblDueDate", ResourceType = typeof(EvolutionResources))]
        public DateTimeOffset? DueDate { set; get; }
        [Display(Name = "lblCompletionDate", ResourceType = typeof(EvolutionResources))]
        public DateTimeOffset? CompletionDate { set; get; }
        public int? CompletedById { set; get; }
        public int StatusId { set; get; }
        public int? CustomerId { set; get; }
        public bool Enabled { set; get; } = true;

        // Additional fields
        public String CreatedDateISO { get { return CreatedDate.ISODate(); } }
        public String DueDateISO { get { return DueDate.ISODate(); } }
        public String CompletionDateISO { get { return CompletionDate.ISODate(); } }
        public String BusinessUnit { set; get; } = "";
        public String AssigneeName { set; get; } = "";
        public String TaskTypeText { set; get; } = "";
        public String StatusText { set; get; } = "";
        public String CompletedByName { set; get; } = "";
        public String StatusColour { set; get; } = "";
        public String CustomerName { set; get; } = "";
        public String CustomerUrl { set; get; } = "";
    }

    public class TaskListModel : BaseListModel {
        public List<TaskModel> Items { set; get; } = new List<TaskModel>();
    }
}
