using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Evolution.Extensions {
    public static class DictionaryExtensions {
        public static void AddProperty(this Dictionary<string, string> dictionary, string key, string value) {

            if (!string.IsNullOrEmpty(key)) {
                string tempValue = (string.IsNullOrEmpty(value) ? "" : value);
                try {
                    dictionary.Add(key, tempValue);
                } catch {
                    try {
                        dictionary[key] = tempValue;
                    } catch { }
                }
            }
        }

        public static void AddProperty(this Dictionary<string, string> dictionary, string key, int? value) {
            if(value != null) dictionary.AddProperty(key, value.Value.ToString());
        }

        public static void AddProperty(this Dictionary<string, string> dictionary, string key, float? value) {
            if (value != null) dictionary.AddProperty(key, value.Value.ToString());
        }

        public static void AddProperty(this Dictionary<string, string> dictionary, string key, double? value) {
            if (value != null) dictionary.AddProperty(key, value.Value.ToString());
        }

        public static void AddProperty(this Dictionary<string, string> dictionary, string key, decimal? value) {
            if (value != null) dictionary.AddProperty(key, value.Value.ToString());
        }
    }
}
