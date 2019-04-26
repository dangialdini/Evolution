using System;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Evolution.DAL;
using Evolution.LookupService;
using Evolution.CommonService;
using Evolution.Enumerations;
using Evolution.Models.Models;
using Evolution.Extensions;

namespace Evolution.LookupServiceTests {
    public partial class LookupServiceTests {
        [TestMethod]
        public void GetExecutableNameTest() {
            Assembly assembly = Assembly.GetExecutingAssembly();
            string test = LookupService.GetExecutableName(assembly);
            Assert.IsTrue(!string.IsNullOrEmpty(test), "Error: GetExecutableName returned an empty string when a non-empty string was expected");
        }

        [TestMethod]
        public void GetSoftwareVersionInfoTest() {
            Assembly assembly = Assembly.GetExecutingAssembly();
            string test = LookupService.GetSoftwareVersionInfo(assembly);
            Assert.IsTrue(!string.IsNullOrEmpty(test), "Error: GetSoftwareVersionInfo returned an empty string when a non-empty string was expected");
            Assert.IsTrue(test.CountOf(".") == 3, $"Error: GetSoftwareVersionInfo returned {test} when a value in the format x.x.x.x was expected");
        }

        [TestMethod]
        public void GetExecutableDateTest() {
            Assembly assembly = Assembly.GetExecutingAssembly();
            var test = LookupService.GetExecutableDate(assembly);
            Assert.IsTrue(test != null, "Error: GetExecutableDate returned an empty date when a non-empty date was expected");
        }

        [TestMethod]
        public void GetSoftwareCopyrightInfoTest() {
            Assembly assembly = Assembly.GetExecutingAssembly();
            string test = LookupService.GetSoftwareCopyrightInfo(assembly);
            Assert.IsTrue(!string.IsNullOrEmpty(test), "Error: GetSoftwareCopyrightInfo returned an empty string when a non-empty string was expected");
        }
    }
}
