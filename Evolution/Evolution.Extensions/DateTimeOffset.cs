using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Evolution.Extensions {
    public enum OffsetType {
        Minutes = 1,
        Hours = 60
    }

    public static class DateTimeOffsetExtensions {
        public static DateTimeOffset ParseDate(this DateTimeOffset dto, string offset, OffsetType offsetType = OffsetType.Minutes) {
            return dto.ParseDate(Convert.ToDecimal(offset), offsetType);
        }

        public static DateTimeOffset ParseDate(this DateTimeOffset dto, decimal offset, OffsetType offsetType = OffsetType.Minutes) {
            // This extension should be used to convert a date entered on a screen by
            // a user to a DateTimeOffset value.

            // Note:    The resulting value is UTC with a local offset as there is no time component.
            //          For example, if the user enters 20/11/2017, they are specifying that date in their
            //          local time. We therefore return 2017-11-20 00:00:00 + 11:00 for Australia
            //          where the + 11:00 is supplied as the offset parameter in minutes ie (11 * 60).
            //          The offset is decimal because it can be expressed in fractions of an hour or
            //          as integer minutes.
            var ts = getOffset(offset, offsetType);
            DateTimeOffset rc = new DateTimeOffset(dto.Year, dto.Month, dto.Day, 0, 0, 0, ts);
            return rc;
        }

        public static DateTimeOffset Now(string offset, OffsetType offsetType = OffsetType.Minutes) {
            return Now(Convert.ToDecimal(offset), offsetType);
        }

        public static DateTimeOffset Now(decimal offset, OffsetType offsetType = OffsetType.Minutes) {
            var ts = getOffset(offset, offsetType);
            DateTimeOffset dto = DateTimeOffset.UtcNow;
            DateTimeOffset now = new DateTimeOffset(dto.Year, dto.Month, dto.Day, dto.Hour, dto.Minute, dto.Second, ts);
            return now;
        }

        public static DateTimeOffset StartOfDay(this DateTimeOffset dt, decimal offset, OffsetType offsetType = OffsetType.Minutes) {
            var ts = getOffset(offset, offsetType);
            return new DateTimeOffset(dt.Year, dt.Month, dt.Day, 0, 0, 0, ts);
        }

        public static DateTimeOffset StartOfDay(this DateTimeOffset dt, TimeSpan startOfWorkingDay, decimal offset, OffsetType offsetType = OffsetType.Minutes) {
            var ts = getOffset(offset, offsetType);
            DateTimeOffset startOfDay = new DateTimeOffset(dt.Year, dt.Month, dt.Day, 0, 0, 0, ts) + startOfWorkingDay;
            return startOfDay;
        }

        public static DateTimeOffset EndOfDay(this DateTimeOffset dt, decimal offset, OffsetType offsetType = OffsetType.Minutes) {
            var ts = getOffset(offset, offsetType);
            return new DateTimeOffset(dt.Year, dt.Month, dt.Day, 23, 59, 59, ts);
        }

        public static DateTimeOffset EndOfDay(this DateTimeOffset dt, TimeSpan endOfWorkingDay, decimal offset, OffsetType offsetType = OffsetType.Minutes) {
            var ts = getOffset(offset, offsetType);
            DateTimeOffset endOfDay = new DateTimeOffset(dt.Year, dt.Month, dt.Day, 0, 0, 0, ts) + endOfWorkingDay;
            return endOfDay;
        }

        private static TimeSpan getOffset(decimal offset, OffsetType offsetType) {
            decimal offsetValue = offset * (decimal)offsetType;
            TimeSpan ts = new TimeSpan(0, Convert.ToInt32(offsetValue), 0);
            return ts;
        }

        public static string ISODate(this DateTimeOffset dt) {
            if (dt == null) {
                return "";
            } else {
                return dt.ToString("o");
            }
        }

        public static string ISODate(this DateTimeOffset? dt) {
            if (dt == null) {
                return "";
            } else {
                return dt.Value.ToString("o");
            }
        }
    }
}
