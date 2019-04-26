using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Evolution.Extensions;

namespace Evolution.Models.Models {
    public class ScheduledTaskModel {
        public int Id { set; get; } = 0;
        public string TaskName { set; get; } = "";
        public string CurrentState { set; get; } = "";
        public DateTimeOffset? LastRun { set; get; } = null;
        public string LastRunISO { get { return LastRun.ISODate(); } }
        public string Parameters { set; get; } = "";
        public string CmdParameter { set; get; } = "";
        public bool Enabled { set; get; } = false;
    }

    public class ScheduledTaskListModel : BaseListModel {
        public List<ScheduledTaskModel> Items { set; get; } = new List<ScheduledTaskModel>();
    }
}
