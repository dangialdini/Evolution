using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Evolution.Models.Models;

namespace Evolution.Models.ViewModels {
    public class EditNoteViewModel : ViewModelBase {
        public NoteModel Note { set; get; }
        public int MaxUploadFileSize { set; get; } = 0;
        public string ValidFileTypes { set; get; } = "";
    }
}
