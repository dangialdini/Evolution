using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Evolution.CSVFileService {
    public enum CSVDelimiterUsage {
        None = 0,
        All = 1,
        StringsOnly = 2,
        StringsAndDates = 3
    }

    public class CSVFormat {

        public string DataFieldDelimiter { set; get; } = "";            // The string to enclose fields with eg "
        public CSVDelimiterUsage DataFieldDelimiterUsage { set; get; } = CSVDelimiterUsage.None;
        public string DataFieldSeparator { set; get; } = ",";           // The string to separate fields with eg ,
        public string HeaderFieldDelimiter { set; get; } = "";          // The string to enclose header fields with eg "
        public string HeaderFieldSeparator { set; get; } = ",";         // The string to separate header fields with eg ,
        public List<CSVField> Fields { set; get; } = new List<CSVField>();
    }
}
