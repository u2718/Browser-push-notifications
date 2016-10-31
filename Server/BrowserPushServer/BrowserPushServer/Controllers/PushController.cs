using System;
using System.Configuration;
using System.Threading.Tasks;
using System.Web.Http;
using BrowserPushServer.Models;
using BrowserPushServer.Services;

namespace BrowserPushServer.Controllers
{
    public class PushController : ApiController
    {
        private static readonly WebPushService PushService = new WebPushService(ConfigurationManager.AppSettings["firebaseServerKey"]);

        [HttpPost]
        public void Subscribe(SubscribeRequest request)
        {
            if (!request.StatusType.Equals("subscribe", StringComparison.OrdinalIgnoreCase))
                return;

            SubscriptionService.AddOrUpdate(request);
        }

        [HttpGet]
        public async Task<string> SendNotification(string user, string message)
        {
            var sendRequest = new SendRequest { Message = message };
            try
            {
                var subscription = SubscriptionService.Get(user);
                var result = await PushService.SendNotification(subscription, sendRequest);
                return result.ToString();
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }
    }
}
