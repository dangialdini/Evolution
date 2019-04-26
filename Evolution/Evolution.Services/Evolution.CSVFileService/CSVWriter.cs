using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Evolution.CSVFileService {
    public class CSVWriter {

        #region Private members

        StreamWriter sw = null;
        int _currentFormat = -1;

        #endregion

        #region Construction

        public CSVWriter() {
        }

        ~CSVWriter() {
            Close();
        }

        #endregion

        #region Public properties

        public List<CSVFormat> Formats { set; get; } = new List<CSVFormat>();
        public int CurrentFormat {
            set { if (value >= 0 && value < Formats.Count()) _currentFormat = value; }
            get { return _currentFormat; }
        }

        #endregion

        #region Public members

        public void CreateFile(string fileName,
                               CSVFormat format = null,
                               bool bHasHeaderLine = false) {
            List<CSVFormat> formats = new List<CSVFormat>();
            formats.Add(format);

            CreateFile(fileName, formats, bHasHeaderLine);
        }

        public void CreateFile(string fileName, 
                               List<CSVFormat> formats, 
                               bool bHasHeaderLine = false) {

            Formats = formats;
            _currentFormat = (Formats.Count() > 0 ? 0 : -1);

            sw = new StreamWriter(fileName);

            if (bHasHeaderLine) WriteHeaderLine();
        }

        public void Close() {
            if (sw != null) sw.Close();
            sw = null;
        }

        public void WriteLine(Dictionary<string, string> fieldValues) {
            if (_currentFormat >= 0) {
                CSVFormat format = Formats[_currentFormat];

                string recordLine = "";
                for (int i = 0; i < format.Fields.Count(); i++) {
                    if (i != 0) recordLine += format.DataFieldSeparator;

                    CSVField field = format.Fields[i];

                    string fieldValue = fieldValues[field.FieldName];
                    string delim = "";

                    switch(format.DataFieldDelimiterUsage) {
                    case CSVDelimiterUsage.All:
                        delim = format.DataFieldDelimiter;
                        break;
                    case CSVDelimiterUsage.StringsOnly:
                        if(field.FieldType == CSVFieldType.String) delim = format.DataFieldDelimiter;
                        break;
                    case CSVDelimiterUsage.StringsAndDates:
                        if (field.FieldType == CSVFieldType.String ||
                            field.FieldType == CSVFieldType.DateTime) delim = format.DataFieldDelimiter;
                        break;
                    }
                    recordLine += delim + fieldValues[format.Fields[i].FieldName] + delim;
                }
                sw.WriteLine(recordLine);
            }
        }

        public void WriteHeaderLine() {
            if (_currentFormat >= 0) {
                CSVFormat format = Formats[_currentFormat];

                string headerLine = "";
                for (int i = 0; i < format.Fields.Count(); i++) {
                    if (i != 0) headerLine += format.HeaderFieldSeparator;

                    headerLine += format.HeaderFieldDelimiter + format.Fields[i].FieldName + format.HeaderFieldDelimiter;
                }
                sw.WriteLine(headerLine);
            }
        }

        public void AddFormat(CSVFormat format,
                              bool bHasHeaderLine = false) {
            SetFormat(Formats.Count(), format, bHasHeaderLine);
        }

        public void SetFormat(int index,
                              CSVFormat format,
                              bool bHasHeaderLine = false) {
            while(Formats.Count() < index + 1) {
                Formats.Add(new CSVFormat());
            }

            Formats[index] = format;
        }

        #endregion
    }
}
