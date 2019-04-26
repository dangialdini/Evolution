using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Evolution.Models.Models;
using Evolution.Extensions;

namespace Evolution.TemplateService
{
    public class TemplateService {

        #region Private members

        private TemplateProperties TemplateProperties = new TemplateProperties();

        private string preAmble = "",
                        postAmble = "",
                        output = "";

        #endregion

        #region Public members

        List<TemplateSection> templateSections = new List<TemplateSection>();

        public int MaxItemsOnFirstPage {
            get {
                int rc = 0;
                var docSections = TemplateProperties.GetTemplateSections();
                if (docSections.Count() > 0) {
                    var section = FindTemplateSection(docSections[0]);
                    if (section != null) rc = section.SectionTemplate.ReadProperty("MaxItemsOnFirstPage").ParseInt();
                }
                return rc;
            }
        }

        public int MaxItemsOnSecondPage {
            get {
                int rc = 0;
                var docSections = TemplateProperties.GetTemplateSections();
                if (docSections.Count() > 0) {
                    var section = FindTemplateSection(docSections[0]);
                    if (section != null) rc = section.SectionTemplate.ReadProperty("MaxItemsOnSecondPage").ParseInt();
                }
                return rc;
            }
        }

        public int MaxItemsBeforeFooter {
            get {
                int rc = 0;
                var docSections = TemplateProperties.GetTemplateSections();
                if (docSections.Count() > 0) {
                    var section = FindTemplateSection(docSections[0]);
                    if (section != null) rc = section.SectionTemplate.ReadProperty("MaxItemsBeforeFooter").ParseInt();
                }
                return rc;
            }
        }

        #endregion

        #region Construction

        public TemplateService() {
            TemplateProperties = new TemplateProperties();
        }

        public TemplateService(TemplateProperties properties) {
            TemplateProperties = properties;
        }

        #endregion

        #region Template loading

        public Error LoadTemplateFile(string fileName, TemplateProperties properties = null) {
            var error = new Error();

            try {
                string template = File.ReadAllText(fileName);
                error = LoadTemplate(template, properties);

            } catch (Exception e1) {
                error.SetError(e1);
            }

            return error;
        }

        public Error LoadTemplate(string templateText, TemplateProperties properties = null) {
            var error = new Error();

            int nestCount = 0;

            bool bAddToPreamble = true;            
            string sectionTemplate = "";

            TemplateSection section = null;

            if(properties != null) TemplateProperties = properties;

            List<string> sections = TemplateProperties.GetTemplateSections();

            string[] templateLines = templateText.Replace("\r\n", "\n").Trim().Split('\n');

            // When we read a template, we assume that all templates are consecutive and that there
            // is no html between sections.
            // There can be 'pre-amble' html before the first section and post-amble after the last section.
            foreach (string templateLine in templateLines) {
                if (!string.IsNullOrEmpty(templateLine.Trim())) {
                    if (section != null) {
                        // Inside a section, so check for end of section
                        sectionTemplate += templateLine + "\r\n";
                        if (templateLine.IndexOf("<tr") != -1) nestCount++;
                        if (templateLine.IndexOf("</tr>") != -1) {
                            nestCount--;
                            if (nestCount == 0) {
                                section.SetTemplateText(sectionTemplate);
                                sectionTemplate = "";
                                section = null;
                            }
                        }

                    } else {
                        // Not in a section, so look for a start of section
                        int foundIdx = -1;
                        for (int i = 0; i < sections.Count() && foundIdx == -1; i++) {
                            if (templateLine.IndexOf(sections[i]) != -1) foundIdx = i;
                        }

                        if (foundIdx != -1) {
                            // Found a section start
                            nestCount = 1;
                            string sectionName = sections[foundIdx];

                            section = AddTemplateSection(sectionName);
                            sectionTemplate = templateLine + "\r\n";
                            bAddToPreamble = false;

                        } else {
                            // Section start not found, so add to pre/post amble content
                            if (bAddToPreamble) {
                                preAmble += templateLine + "\r\n";

                            } else {
                                postAmble += templateLine + "\r\n";
                            }
                        }
                    }
                }
            }
            return error;
        }

        TemplateSection FindTemplateSection(string sectionName) {
            TemplateSection rc = null;

            int i = 0;
            while (rc == null && i < templateSections.Count()) {
                if (templateSections[i].SectionName == sectionName) rc = templateSections[i];
                i++;
            }

            return rc;
        }

        TemplateSection AddTemplateSection(string sectionName) {

            // Check to see if the section already exists - if it does, we add to it
            TemplateSection section = FindTemplateSection(sectionName);

            if (section == null) {
                section = new TemplateSection {
                    SectionName = sectionName,
                    SectionTemplate = ""
                };

                templateSections.Add(section);
            }

            return section;
        }

        #endregion

        #region Content adding

        bool bInRepeater = false;
        private string lastSectionName = "";

        public void AddContent(string sectionName, 
                               Dictionary<string, string> dict1 = null,
                               Dictionary<string, string> dict2 = null) {
            TemplateSection section;

            if (!string.IsNullOrEmpty(lastSectionName) && lastSectionName != sectionName) {
                section = FindTemplateSection(lastSectionName);
                if (section != null && bInRepeater) {
                    // Finish off the repeater
                    output += section.PostAmble;
                    bInRepeater = false;
                }
            }
            lastSectionName = sectionName;

            section = FindTemplateSection(sectionName);
            if(section != null) {
                if (section.IsRepeater && !bInRepeater) {
                    output += section.PreAmble;
                    bInRepeater = true;
                }
                output += section.Render(dict1, dict2);
            }
        }

        #endregion

        #region Rendering

        public string Render(Dictionary<string,string> dict = null) {
            // Renders the template.
            // Supplying the optional parameter dictionary enables the pre/post amble to be
            // substituted which is used by email templates which generally do not have sections.
            string rc = preAmble + output + postAmble;
            if (dict != null) rc = rc.DoSubstitutions(dict);
            return rc;
        }

        #endregion
    }
}
