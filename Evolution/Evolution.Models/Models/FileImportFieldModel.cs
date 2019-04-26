using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Evolution.Models.Models {
    public class FileImportFieldModel {
        public int Id { set; get; } = 0;
        public int CompanyId { set; get; } = 0;
        public int FileImportRowId { set; get; } = 0;
        public int FieldNo { set; get; } = 0;
        public string Value { set; get; } = "";
    }
}
