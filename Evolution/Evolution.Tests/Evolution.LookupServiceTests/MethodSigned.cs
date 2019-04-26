using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Evolution.DAL;
using Evolution.LookupService;
using Evolution.CommonService;
using Evolution.Enumerations;
using Evolution.Models.Models;

namespace Evolution.LookupServiceTests {
    public partial class LookupServiceTests {
        [TestMethod]
        public void FindMethodSignedListItemModelTest() {
            var methodList = LookupService.FindMethodSignedListItemModel();
            Assert.IsTrue(methodList.Count() > 0, "Error: No items were returned when some were expected");
        }

        [TestMethod]
        public void FindMethodSignedModelTest() {
            var methodList = LookupService.FindMethodSignedListItemModel();
            Assert.IsTrue(methodList.Count() > 0, "Error: No items were returned when some were expected");

            foreach(var item in methodList) {
                var method = LookupService.FindMethodSignedModel(item.Text);
                Assert.IsTrue(method != null, "Error: A NULL value was returned when an object was expected");
                Assert.IsTrue(method.Id.ToString() == item.Id, $"Error: MethodId:{method.Id} was returned when {item.Id} was expected");
                Assert.IsTrue(method.MethodSigned == item.Text, $"Error: {method.MethodSigned} was returned when {item.Text} was expected");
            }
        }
    }
}
