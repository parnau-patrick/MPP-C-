using System;
using Newtonsoft.Json;

namespace ConcursNetworking.jsonprotocol
{
    [Serializable]
    public class Response
    {
        [JsonProperty("type")]
        public ResponseType Type { get; set; }
        
        [JsonProperty("data")]
        public object? Data { get; set; }

        public Response() { }

        public Response(ResponseType type, object? data)
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