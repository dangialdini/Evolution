using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;

namespace Evolution.LookupService {
    public partial class LookupService {
        public string GetExecutableName(Assembly assembly = null) {
            var ass = getAssembly(assembly);
            return ass.GetName().Name;
        }

        public string GetSoftwareVersionInfo(Assembly assembly = null) {
            var ass = getAssembly(assembly);
            string softwareVersion = ass.GetName().Version.ToString();
            return $"v{softwareVersion}";
        }

        public DateTime GetExecutableDate(Assembly assembly = null) {
            var ass = getAssembly(assembly);
            return File.GetLastWriteTime(ass.Location);
        }

        public string GetSoftwareCopyrightInfo(Assembly assembly = null) {
            var ass = getAssembly(assembly);
            object[] attribs = ass.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), true);
            if (attribs.Length > 0) {
                return ((AssemblyCopyrightAttribute)attribs[0]).Copyright;
            } else {
                return "";
            }
        }

        private Assembly getAssembly(Assembly assembly) {
            if (assembly != null) {
                return assembly;
            } else {
                return Assembly.GetExecutingAssembly();
            }
        }
    }
}
