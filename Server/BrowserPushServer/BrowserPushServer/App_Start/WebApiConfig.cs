using System.Web.Http;
using System.Web.Http.Cors;
using Owin;

namespace BrowserPushServer
{
    public static class WebApiConfig
    {
        public static void UseWebApi(this IAppBuilder app, HttpConfiguration config)
        {
            WebApiAppBuilderExtensions.UseWebApi(app, config);

            EnableCors(config);
            // Web API routes
            config.MapHttpAttributeRoutes();
            
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { controller = "Push", id = RouteParameter.Optional }
            );
        }

        private static void EnableCors(HttpConfiguration config)
        {
            var cors = new EnableCorsAttribute("*", "Content-Type, Authorization", "POST,GET,PUT,DELETE");
            config.EnableCors(cors);
        }
    }
}
