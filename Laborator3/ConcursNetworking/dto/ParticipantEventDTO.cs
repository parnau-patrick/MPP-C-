using System;
using ConcursModel.domain;
using Newtonsoft.Json;

namespace ConcursNetworking.dto
{
    [Serializable]
    public class ParticipantEventDTO
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        
        [JsonProperty("name")]
        public string Name { get; set; }
        
        [JsonProperty("age")]
        public int Age { get; set; }
        
        [JsonProperty("eventsCount")]
        public int EventsCount { get; set; }

        public ParticipantEventDTO()
        {
            Name = string.Empty;
        }

        public ParticipantEventDTO(int id, string name, int age, int eventsCount)
        {
            Id = id;
            Name = name;
            Age = age;
            EventsCount = eventsCount;
        }

        public ParticipantEventDTO(Participant participant, int eventsCount)
        {
            Id = participant.Id;
            Name = participant.Name;
            Age = participant.Age;
            EventsCount = eventsCount;
        }
    }
}