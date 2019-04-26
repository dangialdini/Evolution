using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Evolution.CommonService;
using Evolution.DAL;
using Evolution.Models.Models;
using Evolution.Enumerations;
using AutoMapper;

namespace Evolution.SystemService {
    public partial class SystemService {

        #region Public members

        public static Error WinExec(string command, string parameters) {
            var error = new Error();
            return error;
        }

        #endregion
    }
}
