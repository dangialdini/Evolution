using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Evolution.DAL;
using Evolution.TaskService;
using Evolution.Enumerations;

namespace Evolution.TaskProcessor {
    public class HouseKeepingTask : TaskBase {

        public HouseKeepingTask(EvolutionEntities dbEntities) : base(dbEntities) { }

        public override string GetTaskName() { return TaskName.HouseKeepingTask; }

        public override int DoProcessing(string[] args) {
            int keepDays = GetTaskParameter("KeepLogDays", 30);

            // Clean the system log
            int delCount = _db.DeleteLogsOlderThan(keepDays);
            WriteTaskLog($"{delCount} System Log record(s) deleted");

            // Clean the task logs
            delCount = _db.DeleteScheduledTaskLogsOlderThan(keepDays);
            WriteTaskLog($"{delCount} Task Log record(s) deleted");

            // Clean up all logged temporary files and folders
            int folderDeletes = 0,
                fileDeletes = 0;

            _db.CleanupFileLogs(ref folderDeletes, ref fileDeletes, false);
            WriteTaskLog($"{folderDeletes} Temporary Folder(s) deleted");
            WriteTaskLog($"{fileDeletes} Temporary File(s) deleted");

            return 0;
        }
    }
}
