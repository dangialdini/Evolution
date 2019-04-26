using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Evolution.Models.Models {
    public class NoteAttachmentModel {
        public int Id { set; get; } = 0;
        public int CompanyId { set; get; } = 0;
        public int? NoteId { set; get; } = null;
        public int MediaId { set; get; } = 0;

        // Additional fields
        public MediaModel Media { set; get; } = null;
    }
}
