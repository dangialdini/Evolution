using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Evolution.Models.Models {
    public class UserModel {
        public int Id { set; get; } = 0;
        public string Name { set; get; } = "";
        public string LastName { set; get; } = "";
        public string FirstName { set; get; } = "";
        public string EMail { set; get; } = "";
        public int? DefaultBrandCategoryId { set; get; } = null;
        public int? DefaultCompanyId { set; get; } = null;
        public string DateFormat { set; get; } = "dd/MM/yyyy";
        public bool Enabled { set; get; } = false;

        // Construction
        public UserModel() { }
        public UserModel(string firstName, string lastName, string email) {
            FirstName = firstName;
            LastName = lastName;
            EMail = email;
        }

        // Additional properties
        public string FullName { get {
                return (FirstName + " " + LastName).Trim();
            }
        }

        public string LoginId {
            get {
                return (FirstName + "." + LastName).Trim();
            }
        }
    }
}
