using Microsoft.AspNetCore.WebUtilities;

namespace BrowserPushServer.Models
{
    public class Subscription
    {
        public string Endpoint { get; set; }
        public byte[] PublicKey { get; private set; }
        public byte[] SecretKey { get; private set; }

        public Subscription(string endpoint, string publicKey, string secretKey)
        {
            Endpoint = endpoint;
            SetPublicKey(publicKey);
            SetSecretKey(secretKey);
        }

        public void SetPublicKey(string publicKey)
        {
            PublicKey = WebEncoders.Base64UrlDecode(publicKey);
        }

        public void SetSecretKey(string secretKey)
        {
            SecretKey = WebEncoders.Base64UrlDecode(secretKey);
        }
    }
}