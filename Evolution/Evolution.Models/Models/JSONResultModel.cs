﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Evolution.Models.Models {
    public class JSONResultModel {
        public Error Error { set; get; } = new Error();
        public Object Data { set; get; } = null;
    }
}
