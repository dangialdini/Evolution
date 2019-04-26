using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonTest {
    public class ExcludeList {

        List<string> excludes = new List<string>();

        public void Add(string str) { excludes.Add(str); }

        public bool IsExcluded(string str) {
            bool bFound = false;

            for(int i = 0; !bFound && i < excludes.Count(); i++) {
                if (str.IndexOf(excludes[i]) != -1) bFound = true;
            }
            return bFound;
        }
    }
}
