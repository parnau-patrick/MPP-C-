using System;
using Newtonsoft.Json;

namespace ConcursNetworking.jsonprotocol
{
    [Serializable]
    public class Request
    {
        [JsonProperty("type")]
        public RequestType Type
        {
            get; set;
        }
        [JsonProperty("data")]
        public object? Data { get; set; }

        public Request() { }

        public Request(RequestType type, object? data)
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
        