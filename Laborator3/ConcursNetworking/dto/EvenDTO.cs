using System;
using ConcursModel.domain;
using Newtonsoft.Json;

namespace ConcursNetworking.dto
{
    [Serializable]
    public class EventDTO
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        
        [JsonProperty("distance")]
        public string Distance { get; set; }
        
        [JsonProperty("style")]
        public string Style { get; set; }
        
        [JsonProperty("participantsCount")]
        public int ParticipantsCount { get; set; }

        public EventDTO()
        {
            Distance = string.Empty;
            Style = string.Empty;
        }

        public EventDTO(Event evt, int participantsCount)
        {
            Id = evt.Id;
            Distance = evt.Distance;
            Style = evt.Style;
            ParticipantsCount = participantsCount;
        }
    }
}