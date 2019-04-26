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
        public void GetDeliveryWindowTest() {
            TimeSpan offset = new TimeSpan(0, 0, 0);
            string[] testValues = { 
                                 // Input value   Expected result
                                    "2018-01-01", "2018-06-30",
                                    "2018-01-31", "2018-06-30",

                                    "2018-02-01", "2018-06-30",
                                    "2018-02-28", "2018-06-30",

                                    "2018-03-01", "2018-06-30",
                                    "2018-03-31", "2018-06-30",

                                    "2018-04-01", "2018-06-30",
                                    "2018-04-30", "2018-06-30",

                                    "2018-05-01", "2018-07-01",
                                    "2018-05-30", "2018-07-30",

                                    "2018-06-01", "2018-08-01",
                                    "2018-06-30", "2018-08-30",

                                    "2018-07-01", "2018-12-31",
                                    "2018-07-31", "2018-12-31",

                                    "2018-08-01", "2018-12-31",
                                    "2018-08-31", "2018-12-31",
                                    
                                    "2018-09-01", "2018-12-31",
                                    "2018-09-30", "2018-12-31",
                                    
                                    "2018-10-01", "2018-12-31",
                                    "2018-10-31", "2018-12-31",

                                    "2018-11-01", "2019-01-01",
                                    "2018-11-30", "2019-01-30",

                                    "2018-12-01", "2019-02-01",
                                    "2018-12-31", "2019-02-28",
                                    
            };

            for(int i = 0; i < testValues.Length; i += 2) {
                // The following extensions use TimeZoneInfo because DateTimeOffset.Parse
                // gets the wrong UTC offset.
                var input = testValues[i].ParseDateTime();
                var expected = testValues[i + 1].ParseDateTime();

                var actual = LookupService.GetDeliveryWindow(input.Value);

                Assert.IsTrue(actual == expected, $"Error: {actual} was returned when {expected} was expected for {input}");
            }
        }
    }
}
