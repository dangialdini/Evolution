using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Evolution.Extensions {
    public static class DateTimeExtensions {
        public static DateTime StartOfDay(this DateTime dt) {
            return new DateTime(dt.Year, dt.Month, dt.Day, 0, 0, 0);
        }

        public static DateTime StartOfDay(this DateTime dt, TimeSpan startOfWorkingDay) {
            DateTime startOfDay = new DateTime(dt.Year, dt.Month, dt.Day, 0, 0, 0) + startOfWorkingDay;
            return startOfDay;
        }

        public static DateTime EndOfDay(this DateTime dt) {
            return new DateTime(dt.Year, dt.Month, dt.Day, 23, 59, 59);
        }

        public static DateTime EndOfDay(this DateTime dt, TimeSpan endOfWorkingDay) {
            DateTime endOfDay = new DateTime(dt.Year, dt.Month, dt.Day, 0, 0, 0) + endOfWorkingDay;
            return endOfDay;
        }
    }
}
