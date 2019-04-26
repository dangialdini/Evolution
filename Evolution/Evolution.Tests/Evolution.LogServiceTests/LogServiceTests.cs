using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CommonTest;
using Evolution.DAL;
using Evolution.Enumerations;
using Evolution.LogService;

namespace Evolution.LogServiceTests {
    [TestClass]
    public class LogServiceTests : BaseTest {
        [TestMethod]
        public void FindLogTest() {
            // Tested in WriteLogTest below
        }

        [TestMethod]
        public void WriteLogTest() {
            var logSections = Enum.GetValues(typeof(LogSection));
            var severities = Enum.GetValues(typeof(LogSeverity));

            Evolution.LogService.LogService logService = new Evolution.LogService.LogService(db);

            foreach (var logSection in logSections) {
                foreach (LogSeverity severity in severities) {
                    string logMessage = "Log message: Section:" + logSection.ToString() + " Severity:" + severity.ToString();

                    // Overload 1
                    string tempMessage = logMessage + " " + RandomString();

                    logService.WriteLog((LogSection)logSection, (LogSeverity)severity, tempMessage);
                    var logRecord = db.FindLogs()
                                      .Where(l => l.LogSection == (int)logSection &&
                                                  l.Severity == (int)severity &&
                                                  l.Message == tempMessage)
                                      .FirstOrDefault();
                    Assert.IsNotNull(logRecord, "Error: 1 log record was expected but 0 were found");

                    var tempLog = db.FindLog(logRecord.Id);
                    AreEqual(logRecord, tempLog);

                    // Overload 2
                    string tempUrl = RandomString();
                    string tempExceptionMsg = RandomString();
                    Exception e = new Exception(tempExceptionMsg);

                    logService.WriteLog((LogSection)logSection, (LogSeverity)severity, tempUrl, e);
                    logRecord = db.FindLogs()
                                  .Where(l => l.LogSection == (int)logSection &&
                                              l.Severity == (int)severity &&
                                              l.Url == tempUrl &&
                                              l.Message == tempExceptionMsg)
                                  .FirstOrDefault();
                    Assert.IsNotNull(logRecord, "Error: 1 log record was expected but 0 were found");

                    tempLog = db.FindLog(logRecord.Id);
                    AreEqual(logRecord, tempLog);

                    // Overload 3
                    tempUrl = RandomString();
                    tempMessage = logMessage + " " + RandomString();
                    string tempStackTrace = logMessage + " " + RandomString();

                    logService.WriteLog((LogSection)logSection, (LogSeverity)severity, tempUrl, tempMessage, tempStackTrace);
                    logRecord = db.FindLogs()
                                      .Where(l => l.LogSection == (int)logSection &&
                                                  l.Severity == (int)severity &&
                                                  l.Url == tempUrl &&
                                                  l.Message == tempMessage &&
                                                  l.StackTrace == tempStackTrace)
                                      .FirstOrDefault();
                    Assert.IsNotNull(logRecord, "Error: 1 log record was expected but 0 were found");

                    tempLog = db.FindLog(logRecord.Id);
                    AreEqual(logRecord, tempLog);
                }
            }
        }
    }
}
