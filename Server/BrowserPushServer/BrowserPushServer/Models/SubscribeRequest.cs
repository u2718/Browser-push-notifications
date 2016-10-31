using System.Runtime.Serialization;

namespace BrowserPushServer.Models
{
    [DataContract]
    public class SubscribeRequest
    {
        [DataMember]
        public string Endpoint { get; set; }

        [DataMember]
        public string Key { get; set; }

        [DataMember]
        public string Auth { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string StatusType { get; set; }
    }
}