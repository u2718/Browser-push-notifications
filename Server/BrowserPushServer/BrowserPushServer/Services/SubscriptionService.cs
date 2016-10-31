using System.Collections.Generic;
using BrowserPushServer.Models;

namespace BrowserPushServer.Services
{
    public static class SubscriptionService
    {
        private static readonly Dictionary<string, Subscription> Subscriptions = new Dictionary<string, Subscription>();

        public static void AddOrUpdate(SubscribeRequest request)
        {
            if (!Subscriptions.ContainsKey(request.Name))
            {
                Subscriptions.Add(request.Name, new Subscription(request.Endpoint, request.Key, request.Auth));
            }
            else
            {
                Subscriptions[request.Name].Endpoint = request.Endpoint;
                Subscriptions[request.Name].SetPublicKey(request.Key);
                Subscriptions[request.Name].SetSecretKey(request.Auth);
            }
        }

        public static Subscription Get(string userId)
        {
            return Subscriptions[userId];
        }
    }
}