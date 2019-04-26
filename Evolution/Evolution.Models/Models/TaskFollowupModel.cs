using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Evolution.Models.Models {
    public class TaskFollowupModel {
        public bool AddFollowup { set; get; }
        public DateTimeOffset? FollowupDate { set; get; }
        public string FollowupNote { set; get; }
    }
}
