using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Evolution.LookupService;

namespace Evolution.LookupServiceTests {
    public partial class LookupServiceTests {
        [TestMethod]
        public void FindDateFormatListItemModelTest() {
            var list = LookupService.FindDateFormatListItemModel();
            int expected = 2,
                actual = list.Count();
            Assert.IsTrue(actual == expected, $"Error: {actual} items were returned when {expected} were expected");

            string  strExpected = "dd/mm/yyyy",
                    strActual = list[0].Text;
            Assert.IsTrue(strActual == strExpected, $"Error: {strActual} was found when {strExpected} was expected");

            strExpected = "mm/dd/yyyy";
            strActual = list[1].Text;
            Assert.IsTrue(strActual == strExpected, $"Error: {strActual} was found when {strExpected} was expected");
        }
    }
}
