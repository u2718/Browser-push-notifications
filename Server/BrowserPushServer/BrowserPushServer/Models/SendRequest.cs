using System.Runtime.Serialization;

namespace BrowserPushServer.Models
{
    [DataContract]
    public class SendRequest
    {
        [DataMember(Name = "message")]
        public string Message { get; set; }
    }
}