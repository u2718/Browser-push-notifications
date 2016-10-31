using Owin;

namespace BrowserPushServer.Middleware
{
    public static class PreflightRequestHandlerExtension
    {
        public static IAppBuilder HandlePreflightRequests(this IAppBuilder app, PreflightRequestHandlingOptions options)
        {
            app.Use(typeof(PreflightRequestHandler), options);
            return app;
        }
    }
}