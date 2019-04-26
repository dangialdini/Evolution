using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Evolution.Models.Models {
    public class FreightForwarderModel {
        public int Id { set; get; } = 0;
        public int CompanyId { set; get; } = 0;
        public string Name { set; get; } = "";
        public string Address { set; get; } = "";
        public string Phone { set; get; } = "";
        public string Email { set; get; } = "";
        public bool Enabled { set; get; } = false;
    }

    public class FreightForwarderListModel : BaseListModel {
        public List<FreightForwarderModel> Items { set; get; } = new List<FreightForwarderModel>();
    }
}
