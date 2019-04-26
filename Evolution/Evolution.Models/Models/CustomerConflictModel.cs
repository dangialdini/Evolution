using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Evolution.Models.Models {
    public class CustomerConflictModel {
        public int Id { set; get; } = 0;
        public int CompanyId { set; get; } = 0;
        public int? CustomerId { set; get; } = null;
        [UIHint("CustomerSearch")]
        public int? SensitiveWithId { set; get; } = null;
        public string SensitiveWithCompanyName { set; get; } = "";
    }

    public class CustomerConflictListModel : BaseListModel {
        public List<CustomerConflictModel> Items { set; get; } = new List<CustomerConflictModel>();
    }
}
