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
using Evolution.SupplierService;
using Evolution.LookupService;
using Evolution.MediaService;
using Evolution.Resources;

namespace Evolution.SalesService {
    public partial class SalesService {

        #region Public methods

        public Error CreateSalesDocumentPdf(SalesOrderHeaderModel soh,
                                            int templateId,
                                            string pdfFile,
                                            bool showCancelledItems,
                                            ref string outputFile,
                                            int maxItems = Int32.MaxValue) {
            // This method is used by th Sale/Print option where the user chooses
            // the template they wish to print with.
            var error = new Error();

            var template = LookupService.FindDocumentTemplateModel(templateId);
            if (template == null) {
                error.SetRecordError("DocumentTemplate", templateId);

            } else {
                switch (template.TemplateType) {
                case DocumentTemplateType.SaleCancellation:
                case DocumentTemplateType.ProFormaInvoice:
                case DocumentTemplateType.ConfirmedOrder:
                    error = createSalesOrderPdf(soh, template,
                                                pdfFile,
                                                true,
                                                ref outputFile,
                                                maxItems);
                    break;

                case DocumentTemplateType.OrderConfirmation:
                    error = CreateOrderConfirmationPdf(soh, template, 
                                                       pdfFile, 
                                                       showCancelledItems, 
                                                       ref outputFile, 
                                                       maxItems);
                    break;
                }
            }
            return error;
        }

        #endregion
    }
}
