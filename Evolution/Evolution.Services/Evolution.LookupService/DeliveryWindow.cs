using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Evolution.DAL;
using Evolution.Models.Models;
using Evolution.CommonService;
using Evolution.Extensions;
using Evolution.Resources;
using AutoMapper;

namespace Evolution.LookupService {
    public partial class LookupService {

        public DateTimeOffset GetDeliveryWindow(DateTimeOffset inputDate) {
            // Give a date, works out its window close date

            // MMDDStart   MMDDEnd OffsetValue OffsetType  OffsetBase
            // ------------------------------------------------------
            // 0101        0430    0630        Exact       MMDd
            // 0501        0630    2           Month       After Date
            // 0701        1031    1231        Exact       MMDD
            // 1101        1231    0630        Month       After Date

            DateTimeOffset result = inputDate;

            var ddmm = inputDate.Month * 100 + inputDate.Day;
            var delWindow = db.FindDeliveryWindow(ddmm);

            if (delWindow != null) {
                switch (delWindow.OffsetBase.ToUpper()) {
                case "MMDD":
                    string offsetValue = delWindow.OffsetValue.ToString("D4");

                    if (string.Compare(offsetValue, delWindow.MMDDEnd.ToString("D4")) > 0) {
                        result = new DateTimeOffset(inputDate.Year,
                                              Convert.ToInt32(offsetValue.Left(2)),
                                              Convert.ToInt32(offsetValue.Right(2)), 0, 0, 0, inputDate.Offset);
                    } else {
                        result = new DateTimeOffset(inputDate.Year + 1,
                                              Convert.ToInt32(offsetValue.Left(2)),
                                              Convert.ToInt32(offsetValue.Right(2)), 0, 0, 0, inputDate.Offset);
                    }
                    break;

                case "AFTER DATE":
                case "AFTER EOM":
                    if (delWindow.OffsetBase.ToUpper() == "AFTER EOM")
                        result = new DateTimeOffset(inputDate.Year, inputDate.Month + 1, 0, 0, 0, 0, inputDate.Offset);

                    switch (delWindow.OffsetType.ToUpper()) {
                    case "DAY":
                        result = result.AddDays(delWindow.OffsetValue);
                        break;
                    case "WEEK":
                        result = result.AddDays(7 * delWindow.OffsetValue);
                        break;
                    case "MONTH":
                        result = result.AddMonths(delWindow.OffsetValue);
                        break;
                    default:    // YEAR
                        result = result.AddYears(delWindow.OffsetValue);
                        break;
                    }
                    break;
                }
            }

            return result;
        }
    }
}
