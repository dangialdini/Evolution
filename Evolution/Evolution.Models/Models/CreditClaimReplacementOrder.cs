using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Evolution.Models.Models {
    public class CreditClaimReplacementOrderModel {
        public int Id { get; set; } = 0;
        public int CompanyId { get; set; } = 0;
        public int CreditClaimHeaderId { get; set; } = 0;
        public int SalesOrderHeaderId { get; set; } = 0;
    }
}
