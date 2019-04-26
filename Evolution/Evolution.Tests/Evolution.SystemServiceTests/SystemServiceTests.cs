using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CommonTest;
using Evolution.DAL;
using Evolution.SystemService;
using Evolution.Models.Models;
using Evolution.Enumerations;

namespace Evolution.SystemServiceTests {
    [TestClass]
    public partial class SystemServiceTests : BaseTest {
        [TestMethod]
        public void FindLogListModelTest() {
            // Write to each log section/severity combination and check that the message
            // gets writtern to the combination it was written to
            int s1 = Enum.GetValues(typeof(LogSection)).Cast<int>().Min();
            int e1 = Enum.GetValues(typeof(LogSection)).Cast<int>().Max();

            int s2 = Enum.GetValues(typeof(LogSeverity)).Cast<int>().Min();
            int e2 = Enum.GetValues(typeof(LogSeverity)).Cast<int>().Max();

            for (int section = s1; section <= e1; section++) {
                for (int severity = s2; severity <= e2; severity++) {

                    LogResult result1 = getLogCount((LogSection)section, (LogSeverity)severity);
                    int expected = result1.Count;

                    // Write an item to the log
                    string message = RandomString(),
                            stackTrace = RandomString();
                    db.WriteLog((LogSection)section, (LogSeverity)severity, "", message, stackTrace);

                    // Check it was written
                    LogResult result2 = getLogCount((LogSection)section, (LogSeverity)severity);
                    expected++;
                    int actual = result2.Count;
                    Assert.IsTrue(actual == expected, "Error: {actual} was returned when {expected} was expected");

                    var logList = getLogList((LogSection)section, (LogSeverity)severity);
                    var tempLog = logList.Items.Where(ll => ll.Message.Contains(message) &&
                                                            ll.Message.Contains(stackTrace))
                                               .FirstOrDefault();
                    Assert.IsTrue(tempLog != null, "Error: Message was not found");
                }
            }
        }

        private LogListModel getLogList(LogSection section, LogSeverity severity) {
            return SystemService.FindLogListModel(0, 1, 100000, "",
                                                  (int)section,
                                                  (int)severity,
                                                  null,
                                                  null);
        }

        private List<LogResult> getLogCounts() {
            List<LogResult> result = new List<LogResult>();

            int s1 = Enum.GetValues(typeof(LogSection)).Cast<int>().Min();
            int e1 = Enum.GetValues(typeof(LogSection)).Cast<int>().Max();

            int s2 = Enum.GetValues(typeof(LogSeverity)).Cast<int>().Min();
            int e2 = Enum.GetValues(typeof(LogSeverity)).Cast<int>().Max();

            for (int i = s1; i <= e1; i++) {
                for (int j = s2; j <= e2; j++) {
                    LogResult res = getLogCount((LogSection)i, (LogSeverity)j);
                    result.Add(res);
                }
            }
            return result;
        }

        LogResult getLogCount(LogSection section, LogSeverity severity) {

            LogListModel logList = getLogList(section, severity);
            LogResult res = new LogResult();
            res.Section = section;
            res.Severity = severity;
            res.Count = logList.Items.Count;

            return res;
        }

        private string AreEqual(LogResult res1, LogResult res2) {
            string rc = "";

            if (res1.Section != res2.Section) rc = "Error: Section {(int)res1.Section} was returned when {(int)res2.Section} was expected";
            if (res1.Severity != res2.Severity) rc = "Error: Severity {(int)res1.Severity} was returned when {(int)res2.Severity} was expected";
            if (res1.Count != res2.Count) rc = "Error: Section {res1.Count} was returned when {res2.Count} was expected";

            return rc;
        }
    }

    class LogResult {
        public LogSection Section;
        public LogSeverity Severity;
        public int Count;
    }
}
