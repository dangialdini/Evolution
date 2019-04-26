using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Evolution.Models.Models {
    public class TaxCodeModel {
        public int Id { set; get; } = 0;
        public int CompanyId { set; get; } = 0;
        public string TaxCode { set; get; } = "";
        public string TaxCodeDescription { set; get; } = "";
        public decimal? TaxPercentageRate { set; get; } = 0;
        public string TaxCodeTypeId { set; get; } = "";
        public bool Enabled { set; get; } = false;
    }

    public class TaxCodeListModel : BaseListModel {
        public List<TaxCodeModel> Items { set; get; } = new List<TaxCodeModel>();
    }
}
