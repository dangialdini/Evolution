using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.IO;
using Evolution.DAL;
using CommonTest;
using Evolution.Extensions;

namespace CodeQualityTests {
    [TestClass]
    public class DatabaseTests : BaseTest {

        [TestMethod]
        public void CheckForDatabaseTablesNotInEF() {
            // Checks for tables defined in the database which are not defined in EF
            string errorMsg = "";

            // Create an exclude list
            ExcludeList excludes = new ExcludeList();
            excludes.Add("sysdiagrams");

            // Query the server for database tables
            var tableList = db.Database
                              .SqlQuery<string>("SELECT name FROM sysobjects WHERE xtype = 'U'")
                              .ToList();
            foreach (var tableName in tableList) {
                // Look for a code file for this table

                if (!excludes.IsExcluded(tableName)) {
                    string fileName = SourceRoot + @"\Evolution.DAL\" + getEFTableFileName(tableName);
                    if (!File.Exists(fileName)) errorMsg += "  " + tableName + "\r\n";
                }
            }

            Assert.IsTrue(string.IsNullOrEmpty(errorMsg), $"Error: The following tables are defined in the database but haven't been imported to EF:\r\n{errorMsg}");
        }

        private string getEFTableFileName(string tableName) {
            string rc = tableName;

            // EF adjusts table names to create file names
            if (rc == "Media") {
                rc = "Medium";

            } else if (rc == "UserAlias") {
                // Keep the name

            } else {
                // If the table name ends with an 's', the 's' is removed from the end of the file name in EF.
                // However, if the table name ends in 'ss', it is left untouched.
                int len = rc.Length;
                if (rc[len - 1] == 's' && rc[len - 2] != 's') len--;
                rc = rc.Substring(0, len);
            }
            return rc + ".cs";
        }
    }
}

