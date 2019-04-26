using Evolution.DAL;
using Evolution.Models.Models;

namespace Evolution.DocumentService {
    // This service is used for creating pdf documents using templates.
    // The documents can be emailed to suppliers/clients etc

    public partial class DocumentService : CommonService.CommonService {

        #region Construction

        public DocumentService(EvolutionEntities dbEntities) : base(dbEntities) { }

        #endregion

        #region Private methods

        private string doReplacements(string str) {
            string rc = str.Replace("\\t", "\t");
            rc = rc.Replace("&quot;", "\"");
            return rc;
        }

        private string nullToString(string str) {
            return (string.IsNullOrEmpty(str) ? "" : str).Replace("\r\n", "<br/>");
        }

        private Error convertHtmlFileToPDF(string sourceHtmlFile, string targetPdfFile) {
            return PDFService.PDFService.ConvertHtmlFileToPDF(sourceHtmlFile, targetPdfFile);
        }

        #endregion
    }
}
