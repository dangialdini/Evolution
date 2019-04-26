using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Evolution.Extensions;

namespace Evolution.CSVFileService {
    public class CSVReader {
        StreamReader sr = null;

        public List<CSVField> Fields { set; get; } = null;

        public CSVReader() {
        }

        public void OpenFile(string fileName, bool bHasHeaderLine = false, List<CSVField> fieldList = null) {
            if (fieldList != null) {
                Fields = fieldList;
                sr = new StreamReader(fileName);

            } else if (bHasHeaderLine) {
                sr = new StreamReader(fileName);

                // Read the header line
                Fields = new List<CSVField>();

                string fileLine = sr.ReadLine().Trim();
                while(!string.IsNullOrEmpty(fileLine)) {
                    Fields.Add(new CSVField { FieldName = readFieldFromLine(ref fileLine)});
                }

            } else {
                throw new Exception("Error: A CSV file must be opened with a header line or a field list!");
            }
        }

        public void Close() {
            if (sr != null) sr.Close();
            sr = null;
        }

        public Dictionary<string, string> ReadLine() {
            Dictionary<string, string> fieldData = null;
            string fileLine;

            if ((fileLine = sr.ReadLine()) != null) {
                fieldData = new Dictionary<string, string>();

                fileLine = fileLine.Trim().Replace("\t", ",");

                for (int i = 0; i < Fields.Count(); i++) {
                    fieldData.Add(Fields[i].FieldName, readFieldFromLine(ref fileLine));
                }
            }

            return fieldData;
        }

        private string readFieldFromLine(ref string fileLine) {
            string rc = "";
            int pos;

            if (fileLine.Length > 0) {
                if (fileLine[0] == '\"') {
                    // Data is quote delimited
                    pos = fileLine.IndexOf("\"", 1);
                    if (pos == -1) {
                        // Malformed data with no training quote, so assume rest of line
                        rc = fileLine.Substring(1);
                        fileLine = "";
                    } else {
                        rc = fileLine.MidTo(1, pos - 1);
                        fileLine = fileLine.Substring(pos + 1).TrimStart();
                        if (fileLine.Length > 0 && fileLine[0] == ',') fileLine = fileLine.Substring(1).TrimStart();
                    }

                } else {
                    // Data is not quote delimited
                    pos = fileLine.IndexOf(",", 0);
                    if (pos == -1) {
                        // Last field on line
                        rc = fileLine.Trim();
                        fileLine = "";
                    } else {
                        // Not last field on line
                        rc = fileLine.Substring(0, pos).Trim();
                        fileLine = fileLine.Substring(pos + 1).TrimStart();
                    }
                }
            }

            return rc;
        }
    }
}
