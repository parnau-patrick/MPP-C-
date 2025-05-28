using Newtonsoft.Json;

namespace ConcursNetworking.jsonprotocol
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Request
    {
        [JsonProperty("type")]
        public RequestType Type { get; set; }
        
        [JsonProperty("data")]
        public object Data { get; set; }

        // Default constructor for JSON deserialization
        public Request() { }

        public Request(RequestType type, object data)
        {
            Type = type;
            Data = data;
        }

        public override string ToString()
        {
            return $"Request{{type={Type}, data={Data}}}";
        }
    }
}