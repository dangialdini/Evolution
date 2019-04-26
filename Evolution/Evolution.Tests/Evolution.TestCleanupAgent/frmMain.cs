using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Entity;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Evolution.DAL;

namespace Evolution.TestCleanupAgent {
    public partial class frmMain : Form {

        private string appName = "Test Cleanup Agent";
        private bool bRunning = false;
        private bool bStopping = false;

        EvolutionEntities db = new EvolutionEntities();

        public frmMain() {
            InitializeComponent();
        }

        private void frmMain_Load(object sender, EventArgs e) {
            this.Text = appName;
            EnableButtons(false);
        }

        private void EnableButtons(bool isRunning) {
            bRunning = isRunning;
            btnStart.Enabled = !bRunning;
            btnStop.Enabled = bRunning;
        }

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e) {
            if(bRunning) {
                MessageBox.Show("Error: Cannot close the window while the Agent is running!\n\nPlease stop the Agent first.", appName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                e.Cancel = true;
            }
        }

        delegate void StringArgReturningVoidDelegate(string text);

        private void AddText(string text) {
            if (txtResults.InvokeRequired) {
                StringArgReturningVoidDelegate d = new StringArgReturningVoidDelegate(AddText);
                Invoke(d, new object[] { text });

            } else {
                txtResults.AppendText(text + "\r\n");
                txtResults.SelectionStart = txtResults.Text.Length + 1;
            }
        }

        private void btnQueueAll_Click(object sender, EventArgs e) {
            foreach(var tableLog in db.FindTestTableLogs().ToList()) {
                AddText("Queueing Table Session: " + tableLog.SessionId);
                db.AddTestSessionLog(tableLog.SessionId);
            }
            foreach (var fileLog in db.FindTestFileLogs().ToList()) {
                AddText("Queueing File Session: " + fileLog.SessionId);
                db.AddTestSessionLog(fileLog.SessionId);
            }
        }

        private void btnStart_Click(object sender, EventArgs e) {
            int lastSecond = 0,
                secs = 0;

            EnableButtons(true);
            db.Database.CommandTimeout = 600;

            AddText("Agent Started...");

            List<TestSessionLog> completedSessions = new List<TestSessionLog>();

            bStopping = false;
            while(!bStopping) {
                if (secs == 0) {
                    int count = 1;
                    var sessionList = db.FindTestSessionLogs().ToList();
                    Parallel.ForEach(sessionList, (session) => {
                        if (!bStopping) {
                            AddText("Cleaning up session: " + session.SessionId + $" ({count} of {sessionList.Count()})");
                            db.CleanupFiles(session);
                            if (CleanupTables(session.SessionId) == 0) {
                                //db.DeleteTestSessionLog(session);
                                completedSessions.Add(session);
                            }

                            count++;
                            Application.DoEvents();
                        }
                    });
                    if (db.FindTestSessionLogs().Count() == 0) {
                        lastSecond = -1;
                        secs = 60;
                        if (!bStopping) AddText("Waiting...");
                    }

                } else {
                    if (DateTime.Now.Second != lastSecond) {
                        lastSecond = DateTime.Now.Second;
                        secs--;
                    }
                }
                Application.DoEvents();
            }

            db.DeleteTestSessionLogs(completedSessions);

            EnableButtons(false);
            AddText("Agent Stopped.");
        }

        private void btnStop_Click(object sender, EventArgs e) {
            if(MessageBox.Show("Are you sure you wish to stop the Agent ?", appName, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes) {
                AddText("Agent Stopping...");
                bStopping = true;
            }
        }

        private int CleanupTables(string sessionId) {
            int rc = -1;
            string sql = "";

            for (int i = 0; i < 10 && rc != 0 && !bStopping; i++) {
                var cleanupList = db.FindTestTableLogs()
                                    .Where(ttl => ttl.SessionId == sessionId)
                                    .OrderByDescending(ttl => ttl.Id)
                                    .ToList();
                rc = cleanupList.Count();

                foreach (var item in cleanupList) {
                    try {
                        if (item.TableName.ToLower() == "company") {
                            sql = $"EXEC DeleteCompany {item.RowId}";
                            db.Database.ExecuteSqlCommand(TransactionalBehavior.DoNotEnsureTransaction, sql);

                        } else if (item.TableName.ToLower() == "user") {
                            sql = $"DELETE FROM FileImportField WHERE FileImportRowId IN (SELECT Id FROM FileImportRow WHERE UserId={item.RowId})";
                            db.Database.ExecuteSqlCommand(TransactionalBehavior.DoNotEnsureTransaction, sql);

                            sql = $"DELETE FROM FileImportRow WHERE UserId={item.RowId}";
                            db.Database.ExecuteSqlCommand(TransactionalBehavior.DoNotEnsureTransaction, sql);

                            sql = $"DELETE FROM FileImportFile WHERE UserId={item.RowId}";
                            db.Database.ExecuteSqlCommand(TransactionalBehavior.DoNotEnsureTransaction, sql);

                            sql = $"DELETE FROM Lock WHERE UserId={item.RowId}";
                            db.Database.ExecuteSqlCommand(TransactionalBehavior.DoNotEnsureTransaction, sql);

                        } else {
                            sql = $"DELETE FROM Lock WHERE TableName='{item.TableName}' AND LockedRowId={item.RowId}";
                            db.Database.ExecuteSqlCommand(TransactionalBehavior.DoNotEnsureTransaction, sql);

                            sql = $"DELETE FROM {item.TableName} WHERE Id={item.RowId}";
                            db.Database.ExecuteSqlCommand(TransactionalBehavior.DoNotEnsureTransaction, sql);

                            sql = $"DELETE FROM TestTableLog WHERE Id={item.Id}";
                            db.Database.ExecuteSqlCommand(TransactionalBehavior.DoNotEnsureTransaction, sql);
                        }

                    } catch (Exception e1) {
                        AddText("Error: " + e1.Message);
                        AddText("  " + sql);
                    }
                    Application.DoEvents();
                    if (bStopping) break;
                }
            }
            return rc;
        }
    }
}
