using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Evolution.Models.Models {
    public class SupplierTermModel {
        public int Id { set; get; } = 0;
        public int CompanyId { set; get; } = 0;
        public string SupplierTermName { set; get; } = "";
        public decimal? ValueBeforeProduction { set; get; } = 0;
        public decimal? ValueOnBOL { set; get; } = 0;
    	public decimal? ValueBeforeLoading { set; get; } = 0;
        public decimal? ValueAfterInvoice { set; get; } = 0;
        public int? DaysAfterInvoice { set; get; } = 0;
	    public decimal? ValueAfterProduction { set; get; } = 0;
        public bool Enabled { set; get; } = false;
    }

    public class SupplierTermListModel : BaseListModel {
        public List<SupplierTermModel> Items { set; get; } = new List<SupplierTermModel>();
    }
}
