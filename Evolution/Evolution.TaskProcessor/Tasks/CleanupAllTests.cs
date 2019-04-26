using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Evolution.DAL;
using Evolution.TaskService;
using Evolution.Enumerations;

namespace Evolution.TaskProcessor {
    public class CleanupAllTestsTask : TaskBase {

        public CleanupAllTestsTask(EvolutionEntities dbEntities) : base(dbEntities) { }

        public override string GetTaskName() { return TaskName.CleanupAllTests; }

        public override int DoProcessing(string[] args) {
            _db.CleanupTestFiles();
            return 0;
        }
    }
}
