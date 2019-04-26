using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Evolution.Models.Models {
    public class LOVModel {
        public int Id { set; get; } = 0;
        public string LOVName { set; get; } = "";
        public bool MultiTenanted { set; get; } = false;
    }
}
