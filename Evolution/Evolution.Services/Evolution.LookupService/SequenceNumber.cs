using System;
using System.Linq;
using Evolution.Models.Models;
using Evolution.Enumerations;

namespace Evolution.LookupService {
    public partial class LookupService {

        #region Public members    

        public decimal GetNextSequenceNumber(CompanyModel company, SequenceNumberType sequenceNumberType,
                                             decimal existingNumber = 0, bool sameSequence = false) {
            decimal rc = 0;
            if (existingNumber != 0) {
                // Supplying an existing number specifies that we want to create a sub-number from it
                // ie   Supply  SameSeq     Result
                //      50      false       50.1
                //      50.1    false       50.11
                //      50.11   false       50.111

                //      50      true        50.1
                //      50.1    true        50.2
                //      50.2    true        50.3
                //      50.11   true        50.12
                //      50.12   true        50.13

                var temp = existingNumber.ToString();
                var pos = temp.IndexOf(".");
                if (pos == -1) {
                    temp += ".1";
                    rc = Convert.ToDecimal(temp);

                } else {
                    if (sameSequence) {
                        int dp = temp.Substring(pos + 1).Length;
                        int multiplier = (int)Math.Pow(10, dp);
                        decimal addValue = 1.0m / multiplier;
                        rc = existingNumber + addValue;

                    } else {
                        temp += "1";
                        rc = Convert.ToDecimal(temp);
                    }
                }

            } else { 
                // No existing number means we want the next integral number
                var result = db.GetNextSequenceNumber(company.Id, (int)sequenceNumberType);
                rc = result.First().Value;
            }
            return rc;
        }

        #endregion
    }
}
