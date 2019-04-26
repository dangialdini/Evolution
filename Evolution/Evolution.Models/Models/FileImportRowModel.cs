using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Evolution.Models.Models {
    public class FileImportRowModel {
        public int Id { set; get; } = 0;
        public int CompanyId { set; get; } = 0;
        public int UserId { set; get; } = 0;
        public int? ProductId { set; get; } = null;
        public int? SupplierId { set; get; } = null;
        public string ErrorMessage { set; get; } = "";

        public List<FileImportFieldModel> Fields { set; get; } = new List<FileImportFieldModel>();
    }
}
