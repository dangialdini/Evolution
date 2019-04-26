using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Reflection;
using CommonTest;
using Evolution.Extensions;

namespace CodeQualityTests {
    [TestClass]
    public class CodeCoverageTest : BaseTest {

        string      sourceAssemblyName,
                    testAssemblyName;

        Assembly    assembly,
                    assemblyTests;

        Type[]      assemblyTypes,
                    assemblyTestTypes;

        [TestMethod]
        public void CheckForCodeCoverageTest() {
            // Cleanup all old test data
            //CleanupAllTests = true;

            // Check that all public methods have tests

            // All assemblies requiring test coverage should be listed here
            var assemblyNames = new List<string>();
            assemblyNames.Add(ApplicationName + ".AllocationService");
            assemblyNames.Add(ApplicationName + ".AuditService");
            assemblyNames.Add(ApplicationName + ".CompanyService");
            assemblyNames.Add(ApplicationName + ".CSVFileService");
            assemblyNames.Add(ApplicationName + ".CustomerService");
            assemblyNames.Add(ApplicationName + ".DataTransferService");
            assemblyNames.Add(ApplicationName + ".DocumentService");
            assemblyNames.Add(ApplicationName + ".EMailService");
            assemblyNames.Add(ApplicationName + ".EncryptionService");
            assemblyNames.Add(ApplicationName + ".FileCompressionService");
            assemblyNames.Add(ApplicationName + ".FileImportService");
            assemblyNames.Add(ApplicationName + ".FileManagerService");
            assemblyNames.Add(ApplicationName + ".FilePackagerService");
            assemblyNames.Add(ApplicationName + ".FTPService");
            assemblyNames.Add(ApplicationName + ".LogService");
            assemblyNames.Add(ApplicationName + ".LookupService");
            assemblyNames.Add(ApplicationName + ".MediaService");
            assemblyNames.Add(ApplicationName + ".MembershipManagementService");
            //assemblyNames.Add(ApplicationName + ".MenuService");
            assemblyNames.Add(ApplicationName + ".NoteService");
            assemblyNames.Add(ApplicationName + ".PDFService");
            assemblyNames.Add(ApplicationName + ".PepperiImportService");
            assemblyNames.Add(ApplicationName + ".PickService");
            assemblyNames.Add(ApplicationName + ".ProductService");
            assemblyNames.Add(ApplicationName + ".PurchasingService");
            assemblyNames.Add(ApplicationName + ".SalesService");
            assemblyNames.Add(ApplicationName + ".ShipmentService");
            assemblyNames.Add(ApplicationName + ".SupplierService");
            assemblyNames.Add(ApplicationName + ".SystemService");
            assemblyNames.Add(ApplicationName + ".TaskManagerService");
            assemblyNames.Add(ApplicationName + ".TaskProcessor");
            assemblyNames.Add(ApplicationName + ".TaskService");
            assemblyNames.Add(ApplicationName + ".TemplateService");

            foreach (var assemblyName in assemblyNames) {
                switch (assemblyName) {
                case "Evolution.TaskProcessor":
                    sourceAssemblyName = SourceRoot + @"\" + ApplicationName + @".TaskProcessor\bin\debug\" + assemblyName + ".exe";
                    testAssemblyName = SourceRoot + @"\" + ApplicationName + @".Tests\" + assemblyName + @"Tests\bin\debug\" + assemblyName + "Tests.dll";
                    break;

                default:
                    sourceAssemblyName = SourceRoot + @"\" + ApplicationName + @".Services\" + assemblyName + @"\bin\debug\" + assemblyName + ".dll";
                    testAssemblyName = SourceRoot + @"\" + ApplicationName + @".Tests\" + assemblyName + @"Tests\bin\debug\" + assemblyName + "Tests.dll";
                    break;
                }

                try {
                    assembly = Assembly.LoadFrom(sourceAssemblyName);
                } catch (Exception e1) {
                    Assert.Fail($"Error: Failed to load Assembly '{sourceAssemblyName}' : {e1.Message}");
                }
                assemblyTypes = assembly.GetExportedTypes();

                try {
                    assemblyTests = Assembly.LoadFrom(testAssemblyName);
                } catch (Exception e1) {
                    Assert.Fail($"Error: Tests of Assembly '{sourceAssemblyName}' require Assembly '{testAssemblyName}' which could not be found of loaded: {e1.Message}");
                }
                assemblyTestTypes = assemblyTests.GetExportedTypes();

                // Look for public types with public methods in the corresponding test assembly
                string result = checkTestAssemblyForTests();
                Assert.IsTrue(string.IsNullOrEmpty(result), result);
            }
        }

        private string checkTestAssemblyForTests() {
            string result = "";

            foreach (var exportedType in assemblyTypes) {

                string typeName = ApplicationName + "." + exportedType.Name,
                       testTypeName = exportedType.Namespace + "Tests." + exportedType.Name + "Tests";

                var publicMethods = exportedType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Static);
                foreach (var publicMethod in publicMethods) {
                    // Now look for a corresponding type and method in the test assembly with the same names
                    string methodName = publicMethod.Name;

                    if (methodName.Left(4) != "get_" && methodName.Left(4) != "set_") {
                        // We don't check set/get properties as these are all tested in specific tests
                        var testType = assemblyTestTypes.Where(att => att.FullName == testTypeName).FirstOrDefault();
                        if (testType == null) {
                            result += $"Error: Test class '{testTypeName}' could not be found in assembly  '{testAssemblyName}' !\r\n";

                        } else {
                            // Now look for a test method on the test class
                            var publicTestMethods = testType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

                            var testMethod = publicTestMethods.Where(atm => atm.Name == methodName + "Test").FirstOrDefault();
                            if (testMethod == null) {
                                result += $"Error: Test method '{methodName}Test' could not be found in assembly '{testAssemblyName}' to test '{typeName}.{methodName}' !\r\n";
                            }
                        }
                    }
                }
            }

            return result;
        }
    }
}
