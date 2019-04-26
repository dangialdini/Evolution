using System;
using System.Linq;
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
        public void FindFileTransferTypeListItemModelTest() {
            var transferTypeList = LookupService.FindFileTransferTypeListItemModel();

            int actual = transferTypeList.Count(),
                expected = 2;
            Assert.IsTrue(actual >= expected, $"Error: {actual} items were returned when {expected} or more were expected");

            string expcted = "Send";
            Assert.IsTrue(transferTypeList[0].Text == expcted, $"Error: {transferTypeList[0].Text} was returned when '{expcted}' was expected");
            expcted = "Receive";
            Assert.IsTrue(transferTypeList[1].Text == expcted, $"Error: {transferTypeList[0].Text} was returned when '{expcted}' was expected");
        }
    }
}
