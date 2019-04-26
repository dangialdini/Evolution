using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Evolution.Models.Models;
using System.Web;

namespace Evolution.Models.ViewModels {
    public class FileUploadViewModel : ViewModelBase {
        public IEnumerable<HttpPostedFileBase> Files { get; set; }
        public IEnumerable<HttpPostedFileBase> Images { get; set; }
        public bool FirstLineContainsHeader { set; get; } = false;
        public int MaxUploadFileSize { set; get; } = 0;
        public string ValidFileTypes { set; get; } = "";
        public string ColumnHeadings = "";
    }
}
