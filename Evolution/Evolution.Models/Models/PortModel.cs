using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Evolution.Models.Models {
    public class PortModel {
        public int Id { set; get; } = 0;
        public string PortName { set; get; } = "";
        public bool Enabled { set; get; } = false;
    }

    public class PortlListModel : BaseListModel {
        public List<PortModel> Items { set; get; } = new List<PortModel>();
    }
}
