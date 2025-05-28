using System;
using Newtonsoft.Json;

namespace ConcursModel.domain
{
    [Serializable]
    [JsonObject(MemberSerialization.OptIn)]
    public class Event : Entity<int>
    {
        [JsonProperty("distance")]
        public string Distance { get; set; }
        
        [JsonProperty("style")]
        public string Style { get; set; }

        public Event() 
        {
            Distance = string.Empty;
            Style = string.Empty;
        }

        public Event(string distance, string style)
        {
            Distance = distance;
            Style = style;
        }

        public override string ToString()
        {
            return $"{Distance}m - {Style}";
        }
    }
}