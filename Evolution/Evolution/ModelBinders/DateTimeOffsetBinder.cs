using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.ModelBinding;
using System.Web.Mvc;
using Evolution.DAL;

namespace Evolution {
    public class DateTimeOffsetBinder : System.Web.Mvc.DefaultModelBinder {

        string dateFormat = null;

        public override object BindModel(ControllerContext controllerContext, 
                                         System.Web.Mvc.ModelBindingContext bindingContext) {
            object result = null;

            var name = bindingContext.ModelName;
            var value = bindingContext.ValueProvider.GetValue(name);

            if (dateFormat == null) {
                EvolutionEntities db = new EvolutionEntities();
                var mms = new MembershipManagementService.MembershipManagementService(db);

                var user = mms.User;
                dateFormat = user.DateFormat;
            }

            if (value != null && !string.IsNullOrEmpty(value.AttemptedValue)) {
                DateTimeOffset date;
                if (DateTimeOffset.TryParseExact(value.AttemptedValue, dateFormat + " HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out date)) {
                    result = date;
                } else if (DateTimeOffset.TryParseExact(value.AttemptedValue, dateFormat, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out date)) {
                    result = date;
                } else {
                    result = base.BindModel(controllerContext, bindingContext);
                }
            }
            return result;
        }
    }
}
