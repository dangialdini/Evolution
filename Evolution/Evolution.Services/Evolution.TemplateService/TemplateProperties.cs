using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Evolution.TemplateService {
    public class TemplateProperties {
        public string DocumentHeaderClass { set; get; } = "";
        public string PageHeaderClass { set; get; } = "";
        public string ItemClass { set; get; } = "";
        public string PageFooterClass { set; get; } = "";
        public string DocumentFooterClass { set; get; } = "";

        public List<string> GetTemplateSections() {
            List<string> sections = new List<string>();

            if (!string.IsNullOrEmpty(DocumentHeaderClass)) sections.Add(DocumentHeaderClass);
            if (!string.IsNullOrEmpty(PageHeaderClass)) sections.Add(PageHeaderClass);
            if (!string.IsNullOrEmpty(ItemClass)) sections.Add(ItemClass);
            if (!string.IsNullOrEmpty(PageFooterClass)) sections.Add(PageFooterClass);
            if (!string.IsNullOrEmpty(DocumentFooterClass)) sections.Add(DocumentFooterClass);

            return sections;
        }
    }
}
