using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Evolution.Models.Models {
    public class DataMappingModel {
        public List<string> Headings { set; get; } = new List<string>();
        public List<FileImportRowModel> Lines { set; get; } = new List<FileImportRowModel>();
    }
}
