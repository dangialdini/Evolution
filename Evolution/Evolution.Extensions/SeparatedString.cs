using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Evolution.Extensions {
    public class SeparatedString {
        public string String { set; get; } = "";
        public string Separator { set; get; } = ",";

        public SeparatedString(string str) {
            String = str;
        }

        public SeparatedString(string str, string separator) {
            String = str;
            Separator = separator;
        }

        public string NextString() {
            string result = null;
            if (!string.IsNullOrEmpty(String)) {
                var pos = String.IndexOf(Separator);
                if(pos == -1) {
                    result = String;
                    String = "";
                } else {
                    result = String.Substring(0, pos);
                    String = String.Substring(pos + Separator.Length);
                }
            }
            return result;
        }
    }
}
