using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;
using Evolution.Enumerations;

namespace Evolution.Models.Models {
    public class ProductMediaModel {
        public int Id { set; get; } = 0;
        public int ProductId { set; get; } = 0;
        public int MediaId { set; get; } = 0;
        public bool IsPrimary { set; get; } = false;

        // Additional fields
        public string MediaFile { set; get; } = "";
        public string MediaHtml { set; get; } = "";
    }

    public class ProductMediaListModel : BaseListModel {
        public List<ProductMediaModel> Items { set; get; } = new List<ProductMediaModel>();
    }
}

