using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Evolution.CSVFileService {
    public enum CSVFieldType {
        String = 1,
        Numeric = 2,
        DateTime = 3
    }

    public class CSVField {
        public string FieldName { set; get; } = "";
        public CSVFieldType FieldType { set; get; } = CSVFieldType.String;
    }
}
