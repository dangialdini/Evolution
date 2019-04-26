using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Evolution.Extensions;
using System.IO;

namespace Evolution.TemplateService {
    class TemplateSection {
        public string   SectionName { set; get; } = "";
        public string   SectionTemplate { set; get; } = "";
        public string   PreAmble { set; get; } = "";
        public string   PostAmble { set; get; } = "";
        public bool     IsRepeater { set; get; } = false;

        public void SetTemplateText(string templateText) {
            PreAmble = PostAmble = "";
            IsRepeater = false;

            if (templateText.IndexOf("lineitem") != -1) {
                // Template is for repeated items
                int nesting = 0;
                string ts = "",
                       te = "";

                int mode = 0;
                StringReader sr = new StringReader(templateText);
                string templateLine = "";
                while((templateLine = sr.ReadLine()) != null) {
                    switch (mode) {
                    case 0:
                        if (templateLine.IndexOf("lineitem") != -1) {
                            SectionTemplate = templateLine + "\r\n";
                            if(templateLine.ToLower().IndexOf("<table") != -1) {
                                ts = "<table";
                                te = "</table>";
                            } else if (templateLine.ToLower().IndexOf("<tr") != -1) {
                                ts = "<tr";
                                te = "</tr>";
                            } else if(templateLine.ToLower().IndexOf("<table") != -1) {
                                ts = "<td";
                                te = "</td>";
                            }
                            nesting = 1;
                            mode = 1;
                        } else {
                            PreAmble += templateLine + "\r\n";
                        }
                        break;
                    case 1:
                        SectionTemplate += templateLine + "\r\n";
                        if (templateLine.IndexOf(ts) != -1) nesting++;
                        if (templateLine.IndexOf(te) != -1) nesting--;
                        if(nesting == 0) mode = 2;
                        break;
                    case 2:
                        PostAmble += templateLine + "\r\n";
                        break;
                    }
                }
                IsRepeater = true;

            } else {
                // Template is not repeated items
                SectionTemplate += templateText;
                IsRepeater = false;
            }
        }

        public string Render(Dictionary<string, string> dict1,
                             Dictionary<string, string> dict2) {
            return SectionTemplate.DoSubstitutions(dict1).DoSubstitutions(dict2);
        }
    }
}
