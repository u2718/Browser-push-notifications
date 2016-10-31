namespace BrowserPushServer.Middleware
{
    public class PreflightRequestHandlingOptions
    {
        public PreflightRequestHandlingOptions()
        {
        }

        public PreflightRequestHandlingOptions(string origins, string headers, string methods)
        {
            Origins = origins;
            Headers = headers;
            Methods = methods;
        }

        public string Origins { get; set; }

        public string Headers { get; set; }

        public string Methods { get; set; }
    }
}