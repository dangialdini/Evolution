using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Evolution.DAL;

namespace Evolution.TaskProcessor {
    class Program {
        static void Main(string[] args) {

            TaskProcessorApplication app = new TaskProcessorApplication();
            app.Run(args);
        }
    }
}
