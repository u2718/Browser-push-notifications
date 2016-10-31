using System.Threading.Tasks;
using Microsoft.Owin;

namespace BrowserPushServer.Middleware
{
    public class PreflightRequestHandler : OwinMiddleware
    {
        private readonly PreflightRequestHandlingOptions _options;

        public PreflightRequestHandler(OwinMiddleware next, PreflightRequestHandlingOptions options) : base(next)
        {
            _options = options;
        }

        public override Task Invoke(IOwinContext context)
        {
            if (context.Request.Method.Equals("OPTIONS"))
            {
                context.Response.Headers.Append("Access-Control-Allow-Origin", _options.Origins);
                context.Response.Headers.Append("Access-Control-Allow-Methods", _options.Methods);
                context.Response.Headers.Append("Access-Control-Allow-Headers", _options.Headers);
                return Task.CompletedTask;
            }

            return Next.Invoke(context);
        }
    }
}