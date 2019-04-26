using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CommonTest;
using Evolution.CSVFileService;

namespace Evolution.CSVFileServiceTests {
    [TestClass]
    public class CSVReaderTests : BaseTest {
        [TestMethod]
        public void OpenFileTest() {

            List<CSVField> fields = new List<CSVField>();
            fields.Add(new CSVField { FieldName = "Field1" });
            fields.Add(new CSVField { FieldName = "Field2" });
            fields.Add(new CSVField { FieldName = "Field3" });
            fields.Add(new CSVField { FieldName = "Field4" });

            Dictionary<string, string> fieldValues = new Dictionary<string, string>();
            fieldValues.Add("Field1", "Field1Value");
            fieldValues.Add("Field2", "Field2Value");
            fieldValues.Add("Field3", "Field3Value");
            fieldValues.Add("Field4", "Field4Value");

            CSVReader csvr = new CSVReader();


            // Create an empty file
            StreamWriter sw = new StreamWriter(tempFile);
            sw.Close();

            // Try to read the empty file
            csvr.OpenFile(tempFile, false, fields);
            Dictionary<string, string> fileValues = csvr.ReadLine();
            csvr.Close();
            Assert.IsTrue(fileValues == null, "Error: A non-NULL value was returned when NULL was expected");


            // Create a file with no headers
            sw = new StreamWriter(tempFile);
            string fileLine = "";
            for (int i = 0; i < fields.Count; i++) {
                if (i != 0) fileLine += ",";
                fileLine += fieldValues[fields[i].FieldName];
            }
            sw.WriteLine(fileLine);
            sw.Close();

            // Now read the file
            csvr.OpenFile(tempFile, false, fields);
            fileValues = csvr.ReadLine();
            csvr.Close();
            Assert.IsTrue(fileValues != null, "Error: A NULL value was returned when a non-NULL value was expected");
            Assert.IsTrue(fileValues.Count == 4, $"Error: A value of {fieldValues.Count} was returned when 4 was expected");
            this.AreEqual(fieldValues, fileValues);


            // Create a file with headers
            sw = new StreamWriter(tempFile);
            fileLine = "";
            for (int i = 0; i < fields.Count; i++) {
                if (i != 0) fileLine += ",";
                fileLine += fields[i].FieldName;
            }
            sw.WriteLine(fileLine);
            fileLine = "";
            for (int i = 0; i < fields.Count; i++) {
                if (i != 0) fileLine += ",";
                fileLine += fieldValues[fields[i].FieldName];
            }
            sw.WriteLine(fileLine);
            sw.Close();

            // Now read the file
            csvr.OpenFile(tempFile, true);
            fileValues = csvr.ReadLine();
            csvr.Close();
            Assert.IsTrue(fileValues != null, "Error: A NULL value was returned when a non-NULL value was expected");
            Assert.IsTrue(fileValues.Count == 4, $"Error: A value of {fieldValues.Count} was returned when 4 was expected");
            this.AreEqual(fieldValues, fileValues);


            // Create a file with headers and quotes
            sw = new StreamWriter(tempFile);
            fileLine = "";
            for (int i = 0; i < fields.Count; i++) {
                if (i != 0) fileLine += ",";
                fileLine += "\"" + fields[i].FieldName + "\"";
            }
            sw.WriteLine(fileLine);
            fileLine = "";
            for (int i = 0; i < fields.Count; i++) {
                if (i != 0) fileLine += ",";
                fileLine += "\"" + fieldValues[fields[i].FieldName] + "\"";
            }
            sw.WriteLine(fileLine);
            sw.Close();

            // Now read the file
            csvr.OpenFile(tempFile, true);
            fileValues = csvr.ReadLine();
            csvr.Close();
            Assert.IsTrue(fileValues != null, "Error: A NULL value was returned when a non-NULL value was expected");
            Assert.IsTrue(fileValues.Count == 4, $"Error: A value of {fieldValues.Count} was returned when 4 was expected");
            this.AreEqual(fieldValues, fileValues);


            // Cleanup
            File.Delete(tempFile);
        }

        [TestMethod]
        public void OpenNonExistentFileTest() {
            // try to open a none existing file with no header
            string fileName = Path.GetTempPath() + RandomString() + ".txt";
            LogTestFile(fileName);

            bool error = false;
            CSVReader csvr = new CSVReader();
            try {
                csvr.OpenFile(fileName, false, null);
            } catch {
                error = true;
            }
            Assert.IsTrue(error, "Error: Opening a non-existent file with no header specification should have thrown an exception but it didn't (1)");

            // Try to open a non-existent file where the first line is expected to be a header
            error = false;
            csvr = new CSVReader();
            try {
                csvr.OpenFile(fileName, true);
            } catch {
                error = true;
            }
            Assert.IsTrue(error, "Error: Opening a non-existent file should have thrown an exception but it didn't (2)");

            // try to open a non-existing file when we supply the header
            List<CSVField> fields = new List<CSVField>();
            fields.Add(new CSVField { FieldName = "Field1" });
            fields.Add(new CSVField { FieldName = "Field2" });
            fields.Add(new CSVField { FieldName = "Field3" });
            fields.Add(new CSVField { FieldName = "Field4" });

            error = false;
            csvr = new CSVReader();
            try {
                csvr.OpenFile(fileName, false, fields);
            } catch {
                error = true;
            }
            Assert.IsTrue(error, "Error: Opening a non-existent file should have thrown an exception but it didn't (3)");
        }

        [TestMethod]
        public void OpenEmptyFileTest() {
            string fileName = Path.GetTempPath() + RandomString() + ".txt";
            LogTestFile(fileName);
            File.WriteAllText(fileName, "");

            // Open empty file without specifying headers
            bool error = false;
            CSVReader csvr = new CSVReader();
            try {
                csvr.OpenFile(fileName, false, null);
            } catch {
                error = true;
            }
            Assert.IsTrue(error, "Error: Opening an empty file with no header specification should have thrown an exception but it didn't (1)");

            // Try to open an empty file where the first line is expected to be a header
            error = false;
            csvr = new CSVReader();
            try {
                csvr.OpenFile(fileName, true);
            } catch {
                error = true;
            }
            Assert.IsTrue(error, "Error: Opening an empty file should have thrown an exception but it didn't (2)");

            // try to open an empty file when we supply the header
            List<CSVField> fields = new List<CSVField>();
            fields.Add(new CSVField { FieldName = "Field1" });
            fields.Add(new CSVField { FieldName = "Field2" });
            fields.Add(new CSVField { FieldName = "Field3" });
            fields.Add(new CSVField { FieldName = "Field4" });

            error = false;
            csvr = new CSVReader();
            try {
                csvr.OpenFile(fileName, false, fields);
            } catch {
                error = true;
            }
            Assert.IsTrue(!error, "Error: Opening an empty file when the header is specified should not have thrown an exception (but it did) because this scenario does not actually read a line from the file to cause an exception - it only open the file (3)");

            // Now try to read a line from the empty file
            var data = csvr.ReadLine();
            Assert.IsTrue(data == null, "Error: Reading from an empty file when the header is specified should return NULL data (end of file) but it didn't (4)");
        }

        [TestMethod]
        public void CloseTest() {
            // Tested in OpenFileTest
        }

        [TestMethod]
        public void ReadLineTest() {
            // Tested in OpenFileTest
        }

        string tempFile { get { return TempFileFolder + @"\CSVReadTest.csv"; } }
    }
}
