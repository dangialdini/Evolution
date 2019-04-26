using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Evolution.DAL;
using Evolution.Enumerations;
using System.Globalization;
using System.Threading;

namespace Evolution
{
    public class MvcApplication : System.Web.HttpApplication {
        protected void Application_Start() {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            AutoMapperConfig.RegisterMappings();
            ModelBinders.Binders.Add(typeof(DateTimeOffset), new DateTimeOffsetBinder());
            ModelBinders.Binders.Add(typeof(DateTimeOffset?), new DateTimeOffsetBinder());
        }

        protected void Application_Error() {
            HttpContext httpContext = HttpContext.Current;
            if (httpContext != null) {
                EvolutionEntities db = new EvolutionEntities();

                var request = httpContext.Request;
                var requestLength = request.ContentLength;

                var ex = Server.GetLastError();
                int id = db.WriteLog(ex, httpContext.Request.RawUrl);

                if (ex.Message.IndexOf("The view ") != -1 &&
                    ex.Message.IndexOf(" or its master was not found or no view engine supports the searched locations") != -1 &&
                    ex.Message.IndexOf("Error.cshtml") != -1) {
                    // Stops an infinite loop if Error.cshtml can't be found
                    httpContext.Response.Write(httpContext.Request.RawUrl + "<br/<br/>");
                    httpContext.Response.Write(ex.Message + "<br/<br/>");

                } else {
                    // The error page replaces /r/n with /r/n<br/>  It doesn't touch ||
                    httpContext.Response.Redirect("~/Error?id=" + id.ToString());
                }
            }
        }

        public override void Init() {
            base.Init();
            this.AcquireRequestState += showRouteValues;
        }

        protected void showRouteValues(object sender, EventArgs e) {
            var context = HttpContext.Current;
            if (context == null)
                return;
            var routeData = RouteTable.Routes.GetRouteData(new HttpContextWrapper(context));
        }
    }
}
