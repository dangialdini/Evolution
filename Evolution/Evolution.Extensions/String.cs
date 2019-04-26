using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.IO;
using System.Threading;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Globalization;

namespace Evolution.Extensions {
    public static class StringExtensions {

        #region String parts

        public static string Left(this string str, int numChars) {
            if(str.Length < numChars) {
                return str;
            } else {
                return str.Substring(0, numChars);
            }
        }

        public static string Right(this string str, int numChars) {
            if (str.Length < numChars) {
                return str;
            } else {
                return str.Substring(str.Length - numChars, numChars);
            }
        }

        public static string MidTo(this string str, int fromPos, int toPos) {
            if (fromPos >= str.Length || toPos >= str.Length) {
                return "";
            } else {
                return str.Substring(fromPos, toPos - (fromPos - 1));
            }
        }

        public static string TrimStart(this string str, string strToRemove) {
            string rc = "";
            while(rc.Left(strToRemove.Length) == strToRemove) {
                rc = rc.Substring(strToRemove.Length);
            }
            return rc;
        }

        public static string TrimEnd(this string str, string strToRemove) {
            string rc = "";
            while (rc.Right(strToRemove.Length) == strToRemove) {
                rc = rc.Substring(0, rc.Length - strToRemove.Length);
            }
            return rc;
        }

        public static string ShrinkString(this string str, int maxLength) {
            string rc = str;
            if (rc.Length > maxLength && rc.Length > 3) rc = rc.Left(rc.Length - 3) + "...";
            return rc;
        }

        public static string ObscureString(this string str, bool bObscure) {
            string rc = "";
            if (!string.IsNullOrEmpty(str)) {
                if (bObscure) {
                    rc = str;
                    if (rc.Length > 8) rc = rc.Left(4) + "...." + rc.Right(4);
                } else {
                    rc = str;
                }
            }
            return rc;
        }

        public static string[] SplitCSV(this string str) {
            string[] rc = null;
            try {
                rc = str.Split(',');
            } catch { }
            return rc;
        }

        public static string AddString(this string str, string addString) {
            // Adds a string to the end of a string if is doesn't already end with that string
            if(str.Right(addString.Length) != addString) {
                return str + addString;
            } else {
                return str;
            }
        }

        public static string NullToEmpty(this string str) {
            if(str == null) {
                return "";
            } else {
                return str.Trim();
            }
        }

        public static string GetNthValueFromString(this string optionList, int itemNo, char separatorChar = '|') {
            // Given a string: A|B|C|D
            // Returns the item at the itemNo poistion
            string[] itemList = optionList.Split(separatorChar);
            if (itemNo >= 0 && itemNo < itemList.Length) {
                return itemList[itemNo];
            } else {
                return "";
            }
        }

        public static string GetNValueFromString(this string optionList, int itemValue, char separatorChar = '|') {
            // Given a string: A|1|B|2|C|3|D|4
            // Looks for the item value number and returns its corresponding text
            string rc = "";
            string[] itemList = optionList.Split(separatorChar);
            int i = 1;
            while (i < itemList.Length) {
                if (itemList[i] == itemValue.ToString()) {
                    rc = itemList[i - 1];
                    i = itemList.Length;
                }
                i += 2;
            }
            return rc;
        }

        public static int GetStringPosition(this string valueList, string findValue, char separatorChar = '|') {
            int rc = -1,
                pos = 0;
            string[] tempList = valueList.Split(separatorChar);

            while (rc == -1 && pos < tempList.Length) {
                if (tempList[pos] == findValue) rc = pos + 1;
                pos++;
            }

            return rc;
        }

        public static string ReadProperty(this string str, string propName) {
            // Read a property from a string:
            //      <table style="width:1024px" maxitemsperpage="37" maxitemsbeforefooter="32">
            string result = "";

            int pos1 = str.ToLower().IndexOf(" " + propName.ToLower() + "=\"");
            if(pos1 != -1) {
                pos1 += propName.Length + 3;
                int pos2 = str.IndexOf("\"", pos1);
                if(pos2 != -1) {
                    result = str.MidTo(pos1, pos2 - 1);
                }
            }
            return result;
        }

        public static bool IsHtml(this string text) {
            string temp = text.ToLower();
            if (temp.IndexOf("<html>") != -1 ||
               temp.IndexOf("<br") != -1 ||
               temp.IndexOf("<br/") != -1 ||
               temp.IndexOf("<br /") != -1 ||
               temp.IndexOf("<b>") != -1 ||
               temp.IndexOf("<i>") != -1 ||
               temp.IndexOf("<font") != -1 ||
               temp.IndexOf("<td") != -1 ||
               temp.IndexOf("<table") != -1 ||
               temp.IndexOf("<span") != -1 ||
               temp.IndexOf("</a>") != -1 ||
               temp.IndexOf("</p>") != -1 ||
               temp.IndexOf("<img ") != -1 ||
               temp.IndexOf("<strong>") != -1) {
                return true;
            } else {
                return false;
            }
        }

        public static bool IsItemShared(this string str1, string str2) {
            // Compares to commas separated string to see if any of the items are shared
            bool bRc = false;
            if (!string.IsNullOrEmpty(str1) && !string.IsNullOrEmpty(str2)) {
                string str1List = "," + str1 + ",";
                string[] str2List = str2.Split(',');
                foreach (var item in str2List) {
                    if (str1.IndexOf("," + item + ",") != -1) {
                        bRc = true;
                        break;
                    }
                }
            }
            return bRc;
        }

        public static int CountOf(this string str, string strToFind) {
            int count = 0;
            if(!string.IsNullOrEmpty(str)) {
                int idx = str.IndexOf(strToFind);
                while(idx != -1) {
                    count++;
                    idx = str.IndexOf(strToFind, idx + strToFind.Length);
                }
            }
            return count;
        }

        #endregion

        #region Conversions

        public static List<string> ToStringList(this string str) {
            var strList = new List<string>();
            strList.Add(str);
            return strList;
        }

        #endregion

        #region Numeric parsing

        public static int ParseInt(this string str) {
            int rc = 0;
            try {
                rc = Convert.ToInt32(str);
            } catch { }
            return rc;
        }

        public static decimal ParseDecimal(this string str) {
            decimal rc = 0;
            try {
                rc = Convert.ToDecimal(str);
            } catch { }
            return rc;
        }

        public static double ParseDouble(this string str) {
            double rc = 0;
            try {
                rc = Convert.ToDouble(str);
            } catch { }
            return rc;
        }

        public static int ParseHex(this string str) {
            int rc = 0;
            string temp = str;

            if (temp.Left(1) == "#") {
                try {
                    rc = Convert.ToInt32(temp.Substring(1), 16);
                } catch { }

            } else if(temp.Left(2).ToLower() == "0x") {
                try {
                    rc = Convert.ToInt32(temp.Substring(2), 16);
                } catch { }
            }

            return rc;
        }

        public static List<int> ToIntList(this string str) {
            List<int> intValues = new List<int>();
            if(!string.IsNullOrEmpty(str)) {

                string[] splitStr = str.Split(',');
                foreach(var numValue in splitStr) {
                    intValues.Add(Convert.ToInt32(numValue));
                }
            }
            return intValues;
        }

        #endregion

        #region Boolean parsing

        public static bool ParseBool(this string str) {
            string temp = str.ToLower();
            if(temp == "yes" || temp == "y" || temp == "1" || temp == "true") {
                return true;
            } else {
                return false;
            }
        }

        #endregion

        #region Date/Time parsing

        public static DateTimeOffset? ParseDateTime(this string str) {
            // The following extension uses TimeZoneInfo because DateTimeOffset.Parse()
            // gets the wrong UTC offset for AEST with no daylight saving. TimeZoneInfo gets it right.
            return str.ParseDateTime((int)TimeZoneInfo.Local.BaseUtcOffset.TotalMinutes);
        }

        public static DateTimeOffset? ParseDateTime(this string str, int tzMinutes) {
            DateTimeOffset? dto = null;
            DateTimeOffset dt;

            if (DateTimeOffset.TryParse(str, out dt)) {
                TimeSpan ts = new TimeSpan(0, 0, tzMinutes, 0);

                dto = new DateTimeOffset(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second, ts);
            }

            return dto;
        }

        public static DateTimeOffset? ParseDateTime(this string str, string dateFormat, int tzMinutes = 0) {
            DateTimeOffset? dto = null;
            DateTimeOffset dt;

            try {
                dt = DateTimeOffset.ParseExact(str, dateFormat, CultureInfo.InstalledUICulture);
                TimeSpan ts = new TimeSpan(0, 0, tzMinutes, 0);

                dto = new DateTimeOffset(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second, ts);
            } catch { }

            return dto;
        }

        #endregion

        #region Substitutions

        public static string DoSubstitutions(this string str, Dictionary<string, string> substituteParams, bool highlightReplacements = false) {

            string rc = (str == null ? "" : str),
                   prefix = (highlightReplacements ? "<i><strong>" : ""),
                   postfix = (highlightReplacements ? "</strong></i>" : "");

            if (substituteParams != null) {
                foreach (KeyValuePair<string, string> item in substituteParams) {
                    rc = rc.ReplaceInsensitive("{" + item.Key.ToLower() + "}", prefix + item.Value + postfix);
                }
            }
            return rc;
        }

        public static string DoSubstitutions(this string str, string p1 = null, string p2 = null, string p3 = null, string p4 = null) {
            string rc = str;

            if (p1 != null) rc = rc.Replace("%1", p1);
            if (p2 != null) rc = rc.Replace("%2", p2);
            if (p3 != null) rc = rc.Replace("%3", p3);
            if (p4 != null) rc = rc.Replace("%4", p4);

            return rc;
        }

        #endregion

        #region Filename methods

        public static string FileName(this string str) {
            string rc = str;

            int pos = rc.LastIndexOf("\\");
            if (pos != -1) {
                rc = rc.Substring(pos + 1);

            } else {
                pos = rc.LastIndexOf("/");
                if (pos != -1) {
                    rc = rc.Substring(pos + 1);
                }
            }

            return rc;
        }

        public static string FileExtension(this string str) {
            string rc = str;

            int pos1 = rc.LastIndexOf(".");
            int pos2 = rc.LastIndexOf("\\");
            if(pos2 == -1) pos2 = rc.LastIndexOf("/");

            if(pos1 != -1) {
                // Found a point, but is it after the last slash >
                if (pos1 > pos2) {
                    // Point after last slash
                    rc = rc.Substring(pos1 + 1);
                } else {
                    // Point before last slash, so it isn't an extension marker
                    rc = "";
                }

            } else {
                // No extension found
                rc = "";
            }

            return rc;
        }

        public static string ChangeExtension(this string str, string extn) {
            // Extension must contain leading . ie .txt
            string rc = str;

            int pos1 = rc.LastIndexOf(".");
            if (pos1 == -1) {
                // No . so just add extension
                rc += extn;

            } else {
                // Found a .
                int pos2 = rc.LastIndexOf("\\");
                if(pos2 == -1) {
                    // No slash so . can be assumed to be the extension
                    rc = rc.Substring(0, pos1) + extn;

                } else {
                    // Got a slash
                    if(pos1 < pos2) {
                        // . is before slash, so path contains a . but filename has no extension, so just add it
                        rc += extn;

                    } else {
                        // . is after the slash so can be assumed to be the extension
                        rc = rc.Substring(0, pos1) + extn;
                    }
                }
            }

            return rc;
        }

        public static bool IsFileSpec(this string str) {
            // Parameter examples:      Return

            //      filename.*          true
            //      /Path/filename.*    true

            //      filename.ext        false
            //      /Path/filename.ext  false

            //      /Path               true

            bool bRc = false;

            if(str.IndexOf("*") != -1 || str.IndexOf("?") != -1) {
                bRc = true;

            } else {
                int pos1 = str.LastIndexOf(".");
                int pos2 = str.LastIndexOf("\\");
                if(pos2 == -1) pos2 = str.LastIndexOf("/");

                if (pos1 == -1 || (pos1 != -1 && pos1 < pos2)) bRc = true;
            }

            return bRc;
        }

        public static string FolderName(this string str) {
            string rc = str;
            int pos = rc.LastIndexOf("\\");
            if(pos == -1) pos = rc.LastIndexOf("/");
            if (pos != -1) rc = rc.Substring(0, pos);
            return rc;
        }

        public static bool IsWebUrl(this string str) {
            if(str.ToLower().IndexOf("http://") != -1 ||
               str.ToLower().IndexOf("https://") != -1) {
                return true;
            } else {
                return false;
            }
        }

        public static bool IsYouTubeUrl(this string str) {
            if (str.ToLower().IndexOf("http://www.youtube.com") != -1 ||
               str.ToLower().IndexOf("https://www.youtube.com") != -1) {
                return true;
            } else {
                return false;
            }
        }

        #endregion

        #region Validation methods

        public static bool IsValidInt(this string str, bool bRequired = false) {
            bool bRc = true;

            if (string.IsNullOrEmpty(str.Trim()) && bRequired) {
                bRc = false;
            } else {
                try {
                    int temp = Convert.ToInt32(str);
                } catch {
                    bRc = false;
                }
            }
            return bRc;
        }

        public static bool IsValidDec(this string str, bool bRequired = false) {
            bool bRc = true;

            if (string.IsNullOrEmpty(str.Trim()) && bRequired) {
                bRc = false;
            } else {
                try {
                    decimal temp = Convert.ToDecimal(str);
                } catch {
                    bRc = false;
                }
            }
            return bRc;
        }

        public static bool IsValidDate(this string str, string dateFormat, bool bRequired = false) {
            bool bRc = true;

            if (string.IsNullOrEmpty(str.Trim()) && bRequired) {
                bRc = false;
            } else {
                try {
                    var temp = DateTime.ParseExact(str.PadLeft(10, '0'), dateFormat, CultureInfo.InvariantCulture);
                } catch {
                    bRc = false;
                }
            }
            return bRc;
        }

        public static bool IsValidEMail(this string str) {

            bool    bRc = true;
            int     pos1,
                    pos2,
                    i = 0;
            string  eMail = str.Trim().ToLower(),
                    chars = "abcdefghijklmnopqrstuvwxyz0123456789.@-_";

            pos1 = eMail.IndexOf("@");

            if (eMail == "" || pos1 < 1 || eMail.Length < 7) {
                // Empty address or not enough chars for a@b.com
                bRc = false;

            } else {
                // Get position of . after @
                pos2 = eMail.IndexOf(".", pos1 + 1);
                if (pos2 == -1 || pos2 < pos1 + 2) {
                    // No . after @
                    bRc = false;

                } else {
                    // Check for valid characters
                    while (bRc && i < eMail.Length) {
                        if (chars.IndexOf(eMail[i]) == -1) bRc = false;
                        i++;
                    }

                    if (bRc) {
                        // Check that email doesn't start or end with .
                        if (eMail[0] == '.' || eMail[eMail.Length - 1] == '.') {
                            bRc = false;
                        }
                    }
                }
            }

            return bRc;
        }

        #endregion

        #region Word Capitalisation

        public static string WordCapitalise(this string str) {
            string rc = "";
            if (str != null) {
                var split = str.Split(' ');
                for (int i = 0; i < split.Length; i++) {
                    var word = split[i].Trim();
                    if(word.Length > 1) {
                        // Only word capitalise words > 1 chars long
                        word = char.ToUpper(word[0]) + word.Substring(1).ToLower();
                    }
                    if (i > 0) rc += " ";
                    rc += word;
                }
            }
            return rc;
        }

        #endregion

        #region Replacement

        static public string ReplaceInsensitive(this string str, string searchStr, string replaceStr) {
            str = Regex.Replace(str, searchStr, replaceStr, RegexOptions.IgnoreCase);
            return str;
        }

        #endregion

        #region Encryption and Decryption

        private static readonly string PasswordHash = "3ncrtpt10nP@@Sw0rd";
        private static readonly string SaltKey = "3ncrtpt10nS@lt&Key";
        private static readonly string VIKey = "@1BAF2c3D7D4e513F6g7H8";

        public static string Encrypt(this string plainText) {
            byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);

            byte[] keyBytes = new Rfc2898DeriveBytes(PasswordHash, Encoding.ASCII.GetBytes(SaltKey)).GetBytes(256 / 8);
            var symmetricKey = new RijndaelManaged() { Mode = CipherMode.CBC, Padding = PaddingMode.Zeros };
            var encryptor = symmetricKey.CreateEncryptor(keyBytes, Encoding.ASCII.GetBytes(VIKey));

            byte[] cipherTextBytes;

            using (var memoryStream = new MemoryStream()) {
                using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write)) {
                    cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
                    cryptoStream.FlushFinalBlock();
                    cipherTextBytes = memoryStream.ToArray();
                    cryptoStream.Close();
                }
                memoryStream.Close();
            }
            return Convert.ToBase64String(cipherTextBytes);
        }

        public static string Decrypt(this string encryptedText) {
            byte[] cipherTextBytes = Convert.FromBase64String(encryptedText);
            byte[] keyBytes = new Rfc2898DeriveBytes(PasswordHash, Encoding.ASCII.GetBytes(SaltKey)).GetBytes(256 / 8);
            var symmetricKey = new RijndaelManaged() { Mode = CipherMode.CBC, Padding = PaddingMode.None };

            var decryptor = symmetricKey.CreateDecryptor(keyBytes, Encoding.ASCII.GetBytes(VIKey));
            var memoryStream = new MemoryStream(cipherTextBytes);
            var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
            byte[] plainTextBytes = new byte[cipherTextBytes.Length];

            int decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
            memoryStream.Close();
            cryptoStream.Close();
            return Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount).TrimEnd("\0".ToCharArray());
        }

        #endregion
    }
}
