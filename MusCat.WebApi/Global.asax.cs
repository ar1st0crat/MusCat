using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using MusCat.Infrastructure.Services;

namespace MusCat.WebApi
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            DiConfig.Register(GlobalConfiguration.Configuration);
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            FileLocator.LoadConfiguration();
        }
    }
}
