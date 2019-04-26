using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Evolution.Enumerations;

namespace Evolution.Models.Models {
    public class BaseListModel {

        #region Public Members

        public Error Error { set; get; } = new Error();
        public int TotalRecords { set; get; } = 0;
        public int GridIndex { set; get; }

        #endregion
    }
}
