using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CommonTest;
using Evolution.MapperService;

namespace Evolution.MapperServiceTests {
    [TestClass]
    public class MappingsTests : BaseTest {
        // Tests Mappings class which is used by the Mapper to
        // remap (point members at members with a different name) member names

        [TestMethod]
        public void set_FieldMappingsTest() {
            MapperFieldMapping mapping = new MapperFieldMapping("Field1Source", "Field1Target");
            List<MapperFieldMapping> fieldMappings = new List<MapperFieldMapping>();
            fieldMappings.Add(mapping);

            Mappings mappings = new Mappings();
            int expectedValue = 0;
            Assert.IsTrue(mappings.FieldMappings.Count() == expectedValue, $"Error: {mappings.FieldMappings.Count()} mappings were found when {expectedValue} were expected");

            mappings.FieldMappings = fieldMappings;
            expectedValue = 1;
            Assert.IsTrue(mappings.FieldMappings.Count() == expectedValue, $"Error: {mappings.FieldMappings.Count()} mappings were found when {expectedValue} were expected");
        }

        [TestMethod]
        public void get_FieldMappingsTest() {
            // Tested in set_FieldMappingsTest
        }

        [TestMethod]
        public void AddTest() {
            Mappings mappings = new Mappings();
            int expectedValue = 0;
            Assert.IsTrue(mappings.FieldMappings.Count() == expectedValue, $"Error: {mappings.FieldMappings.Count()} mappings were found when {expectedValue} were expected");

            // Test for mapping fields onto themselves (isn't allowed)
            mappings.Add("Field1Source", "Field1Source");
            mappings.Add("Field2Source", "Field2Source");

            expectedValue = 0;
            Assert.IsTrue(mappings.FieldMappings.Count() == expectedValue, $"Error: {mappings.FieldMappings.Count()} mappings were found when {expectedValue} were expected");

            // Test for mapping to other fields
            mappings.Clear();
            mappings.Add("Field1Source", "Field1Target");
            mappings.Add("Field2Source", "Field2Target");

            // There should be 4 mappings found - two for the above and two to cancel
            // Field1Target and Field2Target so that they don't received their own data which
            // could be overwritten by the mapping.
            expectedValue = 4;
            Assert.IsTrue(mappings.FieldMappings.Count() == expectedValue, $"Error: {mappings.FieldMappings.Count()} mappings were found when {expectedValue} were expected");
        }

        [TestMethod]
        public void ClearTest() {
            Mappings mappings = new Mappings();

            int expectedValue = 0;
            Assert.IsTrue(mappings.FieldMappings.Count() == expectedValue, $"Error: {mappings.FieldMappings.Count()} mappings were found when {expectedValue} were expected");

            mappings.Add("Field1Source", "Field1Target");
            mappings.Add("Field2Source", "Field2Target");

            expectedValue = 4;
            Assert.IsTrue(mappings.FieldMappings.Count() == expectedValue, $"Error: {mappings.FieldMappings.Count()} mappings were found when {expectedValue} were expected");

            mappings.Clear();

            expectedValue = 0;
            Assert.IsTrue(mappings.FieldMappings.Count() == expectedValue, $"Error: {mappings.FieldMappings.Count()} mappings were found when {expectedValue} were expected");
        }

        [TestMethod]
        public void MapperTest() {
            MapperTest sourceData = new MapperServiceTests.MapperTest {
                BitValue = 1,
                IntValue = 2,
                LongValue = 3,
                FloatValue = (float)4.4,
                DecimalValue = (decimal)5.5,
                DateTimeValue = DateTime.Now
            };

            MapperTest targetData = new MapperServiceTests.MapperTest();

            Mapper.Map(sourceData, targetData);
            AreEqual(sourceData, targetData);

            sourceData.BitNValue = 1;
            sourceData.IntNValue = 6;
            sourceData.LongNValue = 7;
            sourceData.FloatNValue = (float)8.8;
            sourceData.DecimalNValue = (decimal)9.9;
            sourceData.DateTimeNValue = DateTime.Now;

            Mapper.Map(sourceData, targetData);
            AreEqual(sourceData, targetData);

            // Now map a field
            sourceData.IntValue2 = 255;
            sourceData.IntValue3 = null;

            Mappings mappings = new Mappings();
            mappings.Add("IntValue2", "IntValue3");

            Assert.IsTrue(sourceData.IntValue2 == 255);
            Assert.IsTrue(sourceData.IntValue3 == null);

            Mapper.Map(sourceData, targetData, mappings);

            Assert.IsTrue(targetData.IntValue2 == null);
            Assert.IsTrue(targetData.IntValue3 == sourceData.IntValue2);
        }
    }

    class MapperTest {
        public int BitValue { set; get; }
        public int IntValue { set; get; }
        public long LongValue { set; get; }
        public float FloatValue { set; get; }
        public decimal DecimalValue { set; get; }
        public DateTime DateTimeValue { set; get; }

        public int? BitNValue { set; get; }
        public int? IntNValue { set; get; }
        public long? LongNValue { set; get; }
        public float? FloatNValue { set; get; }
        public decimal? DecimalNValue { set; get; }
        public DateTime? DateTimeNValue { set; get; }

        public int? IntValue2 { set; get; }
        public int? IntValue3 { set; get; }
    }
}
