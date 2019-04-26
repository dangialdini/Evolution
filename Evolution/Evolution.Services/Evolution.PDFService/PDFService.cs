using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Winnovative;
using Evolution.Models.Models;

namespace Evolution.PDFService
{
    public class PDFService {

        public static Error ConvertHtmlFileToPDF(string sourceFile, string targetFile, bool bLandscape = false, 
                                                      HtmlToPdfPageProperties pageProps = null) {
            if (pageProps == null) pageProps = new HtmlToPdfPageProperties();

            // To convert to PDF we use the Winnovative HTML-PDF library.
            var error = new Error();

            // Create the PDF converter. Optionally the HTML viewer width can
            // be specified as parameter
            // The default HTML viewer width is 1024 pixels.
            var pdfConverter = new PdfConverter();

            // set the license key - required
            pdfConverter.LicenseKey = "R8nYyNnI2MjRxtjI29nG2drG0dHR0Q==";     // Eval license
            //pdfConverter.LicenseKey = "Mb+vvq++r6amvqewrr6tr7CvrLCnp6en";       // Paid for license

            // set the converter options - optional
            pdfConverter.PdfDocumentOptions.PdfPageSize = ConvertPageSize(pageProps.PageSize);
            pdfConverter.PdfDocumentOptions.PdfCompressionLevel = PdfCompressionLevel.Normal;
            pdfConverter.PdfDocumentOptions.PdfPageOrientation = (bLandscape ? PdfPageOrientation.Landscape : PdfPageOrientation.Portrait);

            // Html viewer
            if (pageProps.ViewerWidth != 0) pdfConverter.HtmlViewerWidth = pageProps.ViewerWidth;

            // set if header and footer are shown in the PDF - optional - default is false 
            pdfConverter.PdfDocumentOptions.ShowHeader = false;  // cbAddHeader.Checked;
            pdfConverter.PdfDocumentOptions.ShowFooter = false;  // cbAddFooter.Checked;
            // set if the HTML content is resized if necessary to fit the PDF
            // page width - default is true
            pdfConverter.PdfDocumentOptions.FitWidth = true;    // cbFitWidth.Checked;

            // set the embedded fonts option - optional - default is false
            pdfConverter.PdfDocumentOptions.EmbedFonts = true;  // cbEmbedFonts.Checked;
            // set the live HTTP links option - optional - default is true
            pdfConverter.PdfDocumentOptions.LiveUrlsEnabled = true; // cbLiveLinks.Checked;

            // Set PDF page margins in points or leave them not set to have a PDF page without pageProps
            if (pageProps.Left != 0) pdfConverter.PdfDocumentOptions.LeftMargin = pageProps.Left;
            if (pageProps.Right != 0) pdfConverter.PdfDocumentOptions.RightMargin = pageProps.Right;
            if (pageProps.Top != 0) pdfConverter.PdfDocumentOptions.TopMargin = pageProps.Top;
            if (pageProps.Bottom != 0) pdfConverter.PdfDocumentOptions.BottomMargin = pageProps.Bottom;

            // set if the JavaScript is enabled during conversion to a PDF - default
            // is true
            pdfConverter.JavaScriptEnabled = false;  // cbClientScripts.Checked;

            // set if the images in PDF are compressed with JPEG to reduce the
            // PDF document size - default is true
            pdfConverter.PdfDocumentOptions.JpegCompressionEnabled = true;  // cbJpegCompression.Checked;

            // Performs the conversion and get the pdf document bytes that can
            try {
                pdfConverter.SavePdfFromHtmlFileToFile(sourceFile, targetFile);
            } catch (Exception e1) {
                error.SetError(e1);
            }
            return error;
        }

        public static string GetPageSizes() {
            return "A4|A4|A5|A5|A6|A6|A7|A7";
        }

        static PdfPageSize ConvertPageSize(string pageSize) {
            switch (pageSize.ToUpper()) {
            case "A7":
                return PdfPageSize.A7;
            case "A6":
                return PdfPageSize.A6;
            case "A5":
                return PdfPageSize.A5;
            case "A4":
            default:
                return PdfPageSize.A4;
            }
        }
    }

    public class HtmlToPdfPageProperties {

        public HtmlToPdfPageProperties() {
            SetProperties(20, 20, 20, 20, 0, "A4");
        }

        public HtmlToPdfPageProperties(int leftMargin = 0, int rightMargin = 0, int topMargin = 0, int bottomMargin = 0, int viewerWidth = 0, string pageSize = "A4") {
            SetProperties(leftMargin, rightMargin, topMargin, bottomMargin, viewerWidth, pageSize);
        }

        void SetProperties(int leftMargin = 0, int rightMargin = 0, int topMargin = 0, int bottomMargin = 0, int viewerWidth = 0, string pageSize = "A4") {
            Left = leftMargin;
            Right = rightMargin;
            Top = topMargin;
            Bottom = bottomMargin;
            ViewerWidth = viewerWidth;
            PageSize = pageSize;
        }

        public int Top { set; get; }
        public int Bottom { set; get; }
        public int Left { set; get; }
        public int Right { set; get; }
        public int ViewerWidth { set; get; }
        public string PageSize { set; get; }
    }
}
