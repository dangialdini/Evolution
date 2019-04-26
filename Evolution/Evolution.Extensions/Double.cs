using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Evolution.Extensions {
    public static class DoubleExtensions {
        public static Double? InchesToCm(this Double? inches) {
            if (inches == null) {
                return null;
            } else {
                return Math.Round(inches.Value * (Double)2.54, 1);
            }
        }

        public static Double? CmToInches(this Double? cm) {
            if (cm == null) {
                return null;
            } else {
                return cm * (Double)0.393701;
            }
        }

        public static Double? FeetToM(this Double? feet) {
            if (feet == null) {
                return null;
            } else {
                return Math.Round(feet.Value * (Double).3048, 4);
            }
        }

        public static Double? MToFeet(this Double? m) {
            if (m == null) {
                return null;
            } else {
                return m * (Double)3.28084;
            }
        }

        public static Double? LbToKg(this Double? pounds) {
            if (pounds == null) {
                return null;
            } else {
                return Math.Round(pounds.Value * (Double)0.453592);
            }
        }

        public static Double? KgToLb(this Double? kg) {
            if (kg == null) {
                return null;
            } else {
                return kg * (Double)2.20462;
            }
        }
    }
}
