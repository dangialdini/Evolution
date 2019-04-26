using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Evolution.Extensions {
    public enum BinaryUnit {
        B = 0,
        kB = 1,
        KiB = -1,
        MB = 2,
        MiB = -2,
        GB = 3,
        GiB = -3,
        TB = 4,
        TiB = -4,
        PB = 5,
        PiB = -5,
        EB = 6,
        EiB = -6,
        ZB = 7,
        ZiB = -7,
        YB = 8,
        YiB = -8
    }

    public static class IntExtensions {
        public static string ToBinaryUnit(this Int32 value, BinaryUnit unit, int decimalPlaces = 0) {
            string format = "N" + decimalPlaces.ToString();
            int mult = getMultiplier(unit);
            string rc = (value / (double)Math.Pow(mult, Math.Abs((Int32)unit))).ToString(format);
            rc += unit.ToString();
            return rc;
        }

        public static string ToBinaryUnit(this Int64 value, BinaryUnit unit, int decimalPlaces = 0) {
            string format = "N" + decimalPlaces.ToString();
            int mult = getMultiplier(unit);
            string rc = (value / (double)Math.Pow(mult, Math.Abs((Int64)unit))).ToString(format);
            rc += unit.ToString();
            return rc;
        }

        private static int getMultiplier(BinaryUnit unit) {
            // 1K can be 1024 (IEC/JEDEC) bytes or 1000 bytes (metric)
            // See: https://en.wikipedia.org/wiki/Kilobyte
            int rc = 0;

            if(unit == BinaryUnit.KiB ||
               unit == BinaryUnit.MiB ||
               unit == BinaryUnit.GiB ||
               unit == BinaryUnit.TiB ||
               unit == BinaryUnit.PiB ||
               unit == BinaryUnit.EiB ||
               unit == BinaryUnit.ZiB ||
               unit == BinaryUnit.YiB) {
                rc = 1024;
            } else {
                rc = 1000;
            }            
            return rc;
        }
    }
}