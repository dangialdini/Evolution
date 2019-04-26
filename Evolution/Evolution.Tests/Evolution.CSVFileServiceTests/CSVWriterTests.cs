using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CommonTest;
using Evolution.CSVFileService;

namespace Evolution.CSVFileServiceTests {
    [TestClass]
    public class CSVWriterTests : BaseTest {
        [TestMethod]
        public void CreateFileTest() {
            CSVFormat format = createFormat();

            Dictionary<string, string> fieldValues = new Dictionary<string, string>();
            fieldValues.Add("Field1", "Field1Value");
            fieldValues.Add("Field2", "Field2Value");
            fieldValues.Add("Field3", "Field3Value");
            fieldValues.Add("Field4", "Field4Value");

            CSVWriter csvw = new CSVWriter();

            // Create the file without headers
            csvw.CreateFile(tempFile, format);
            csvw.WriteLine(fieldValues);
            csvw.Close();

            // Check the file written
            using (StreamReader sr = new StreamReader(tempFile)) {

                string fileLine = sr.ReadLine();
                string expectedValue = "Field1Value,Field2Value,Field3Value,Field4Value";
                Assert.IsTrue(fileLine == expectedValue, $"Error: First line incorrect: {fileLine} was found when {expectedValue} was expected");
            }

            // Create the file with headers
            csvw.CreateFile(tempFile, format, true);
            csvw.WriteLine(fieldValues);
            csvw.Close();

            using (StreamReader sr = new StreamReader(tempFile)) {

                string fileLine = sr.ReadLine();
                string expectedValue = "Field1,Field2,Field3,Field4";
                Assert.IsTrue(fileLine == expectedValue, $"Error: First line incorrect: {fileLine} was found when {expectedValue} was expected");

                fileLine = sr.ReadLine();
                expectedValue = "Field1Value,Field2Value,Field3Value,Field4Value";
                Assert.IsTrue(fileLine == expectedValue, $"Error: First line incorrect: {fileLine} was found when {expectedValue} was expected");
            }

            // Create the file with headers and quotes
            format.HeaderFieldDelimiter = "\"";
            format.DataFieldDelimiter = "\"";
            format.DataFieldDelimiterUsage = CSVDelimiterUsage.All;

            csvw.CreateFile(tempFile, format, true);
            csvw.WriteLine(fieldValues);
            csvw.Close();

            using (StreamReader sr = new StreamReader(tempFile)) {

                string fileLine = sr.ReadLine();
                string expectedValue = "\"Field1\",\"Field2\",\"Field3\",\"Field4\"";
                Assert.IsTrue(fileLine == expectedValue, $"Error: First line incorrect: {fileLine} was found when {expectedValue} was expected");

                fileLine = sr.ReadLine();
                expectedValue = "\"Field1Value\",\"Field2Value\",\"Field3Value\",\"Field4Value\"";
                Assert.IsTrue(fileLine == expectedValue, $"Error: First line incorrect: {fileLine} was found when {expectedValue} was expected");
            }

            // Cleanup
            File.Delete(tempFile);
        }

        [TestMethod]
        public void CloseTest() {
            // Tested in CreateFileTest
        }

        [TestMethod]
        public void WriteLineTest() {
            // Tested in CreateFileTest
        }

        [TestMethod]
        public void WriteHeaderLineTest() {
            // Tested in CSVWriter tests
        }

        [TestMethod]
        public void AddFormatTest() {
            CSVFormat format = createFormat();

            CSVWriter csvw = new CSVWriter();

            // Make sure there are no formats to start with
            int actual = csvw.Formats.Count();
            Assert.IsTrue(actual == 0, $"Error: {actual} was returned when 0 were expected");

            for (int expected = 1; expected <= 10; expected++) {
                // Now add a format
                csvw.AddFormat(format);

                actual = csvw.Formats.Count();
                Assert.IsTrue(actual == expected, $"Error: {actual} was returned when {expected} were expected");
            }
        }

        [TestMethod]
        public void SetFormatTest() {
            CSVFormat format = createFormat();

            CSVWriter csvw = new CSVWriter();

            // Make sure there are no formats to start with
            int actual = csvw.Formats.Count();
            Assert.IsTrue(actual == 0, $"Error: {actual} was returned when 0 were expected");

            // Add some formats
            for (int expected = 1; expected <= 10; expected++) {
                csvw.SetFormat(expected - 1, format);

                actual = csvw.Formats.Count();
                Assert.IsTrue(actual == expected, $"Error: {actual} was returned when {expected} were expected");
            }

            // Now set a format
            int rnd = RandomInt(0, 9);
            csvw.SetFormat(rnd, null);

            Assert.IsTrue(csvw.Formats[rnd] == null, $"Error: Format [{rnd}] returned a non-null value when NULL was expected");
        }

        string tempFile { get { return TempFileFolder + @"\CSVWriterTest.csv"; } }

        CSVFormat createFormat() {
            CSVFormat format = new CSVFormat();

            format.Fields.Add(new CSVField { FieldName = "Field1" });
            format.Fields.Add(new CSVField { FieldName = "Field2" });
            format.Fields.Add(new CSVField { FieldName = "Field3" });
            format.Fields.Add(new CSVField { FieldName = "Field4" });

            return format;
        }
    }
}
