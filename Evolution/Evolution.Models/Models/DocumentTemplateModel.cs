using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Evolution.Enumerations;

namespace Evolution.Models.Models {
    public class DocumentTemplateModel {
        public int Id { set; get; } = 0;
        public string Name { set; get; } = "";
        public string Description { set; get; } = "";
        public string BusinessArea { set; get; } = "";
        public string TemplateFile { set; get; } = "";
        public DocumentTemplateCategory TemplateCategory { set; get; }
        public DocumentTemplateType TemplateType { set; get; }
        public bool Enabled { set; get; } = false;

        // Additional properties
        public string QualTemplateFile { set; get; } = "";
    }

    public class DocumentTemplateListModel : BaseListModel {
        public List<DocumentTemplateModel> Items { set; get; } = new List<DocumentTemplateModel>();
    }
}
