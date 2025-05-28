using Newtonsoft.Json;

namespace ConcursNetworking.jsonprotocol
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Response
    {
        [JsonProperty("type")]
        public ResponseType Type { get; set; }
        
        [JsonProperty("data")]
        public object Data { get; set; }

        // Default constructor for JSON deserialization
        public Response() { }

        public Response(ResponseType type, object data)
        {
            Type = type;
            Data = data;
        }

        public override string ToString()
        {
            return $"Response{{type={Type}, data={Data}}}";
        }
    }
}