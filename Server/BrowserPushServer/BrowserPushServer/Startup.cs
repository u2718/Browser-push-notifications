using System.Web.Http;
using BrowserPushServer.Middleware;
using BrowserPushServer;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(Startup))]

namespace BrowserPushServer
{

    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.HandlePreflightRequests(
                new PreflightRequestHandlingOptions
                {
                    Headers = "Content-Type, Authorization",
                    Methods = "POST,GET,PUT,DELETE",
                    Origins = "*"
                });
            HttpConfiguration config = new HttpConfiguration();

            app.UseWebApi(config);
        }
    }
}