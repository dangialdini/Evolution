using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.IO;
using Evolution.DAL;
using Evolution.CommonService;
using Evolution.Models.Models;
using Evolution.Enumerations;
using Evolution.Extensions;
using Evolution.TemplateService;
using Evolution.MediaService;
using Evolution.Resources;

namespace Evolution.DocumentService {
    public partial class DocumentService {

        #region Public methods

        public Error CreateDocumentPdf(Dictionary<string, string> headerProperties,
                                       List<Dictionary<string, string>> records,
                                       string templateFile,
                                       string outputFile,
                                       int maxItems = Int32.MaxValue) {
            var error = new Error();

            try {
                MediaService.MediaService mediaService = new MediaService.MediaService(db);

                string tempFile = MediaService.MediaService.GetTempFile(".html");

                // Perform the substitutions
                TemplateService.TemplateService ts = new TemplateService.TemplateService();

                var templateProperties = new TemplateProperties {
                    DocumentHeaderClass = "DocumentHeader",
                    PageHeaderClass = "PageHeader",
                    ItemClass = "ItemSection",
                    PageFooterClass = "PageFooter",
                    DocumentFooterClass = "DocumentFooter"
                };

                error = ts.LoadTemplateFile(templateFile, templateProperties);

                if (!error.IsError) {
                    // Insert the lines
                    int     maxItemsPerPage = 0,
                            maxItemsBeforeFooter = ts.MaxItemsBeforeFooter,
                            pageNo = 1,
                            itemsOnPage = 0;

                    headerProperties.AddProperty("PAGENO", pageNo);
                    ts.AddContent(templateProperties.DocumentHeaderClass, headerProperties);
                    ts.AddContent(templateProperties.PageHeaderClass, headerProperties);

                    // Add items
                    Dictionary<string, string> dict = new Dictionary<string, string>();

                    int itemCount = 1;
                    foreach (var line in records) {
                        maxItemsPerPage = (pageNo == 1 ? ts.MaxItemsOnFirstPage : ts.MaxItemsOnSecondPage);
                        dict = line;
                        if (itemsOnPage >= maxItemsPerPage) {
                            headerProperties.AddProperty("PAGENO", pageNo);
                            ts.AddContent(templateProperties.PageFooterClass, headerProperties);      // Includes css page-break

                            pageNo++;
                            headerProperties.AddProperty("PAGENO", pageNo);
                            ts.AddContent(templateProperties.PageHeaderClass, headerProperties);
                            itemsOnPage = 0;
                        }

                        ts.AddContent(templateProperties.ItemClass, dict);
                        itemsOnPage++;
                        itemCount++;
                        if (itemCount > maxItems) break;
                    }

                    // End of document
                    if (itemsOnPage > maxItemsBeforeFooter) {
                        // Not enough room left on page, so break
                        ts.AddContent(templateProperties.PageFooterClass, headerProperties);      // Includes css page-break

                        pageNo++;
                        headerProperties.AddProperty("PAGENO", pageNo);
                        ts.AddContent(templateProperties.PageHeaderClass, headerProperties);
                        itemsOnPage = 0;
                    }

                    headerProperties.AddProperty("PAGECOUNT", pageNo);
                    ts.AddContent(templateProperties.DocumentFooterClass, headerProperties, dict);

                    // Write the html file
                    StreamWriter sw = new StreamWriter(tempFile);
                    sw.Write(ts.Render(headerProperties));
                    sw.Close();

                    // Create a folder for it
                    MediaService.MediaService.CreateDirectory(outputFile.FolderName());

                    // Convert it
                    error = convertHtmlFileToPDF(tempFile, outputFile);

                    MediaService.MediaService.DeleteFile(tempFile);
                    if (error.IsError) MediaService.MediaService.DeleteFile(outputFile);
                }

            } catch (Exception e1) {
                error.SetError(e1);
            }

            return error;
        }

        #endregion
    }
}
