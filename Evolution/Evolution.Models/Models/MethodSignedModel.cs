using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Evolution.Models.Models {
    public class MethodSignedModel {
        public int Id { get; set; } = 0;
        public string MethodSigned { get; set; } = "";
        public string Category { get; set; } = "";
        public bool Enabled { get; set; } = false;
    }
    
}
