using System;
using ConcursModel.domain;
using Newtonsoft.Json;

namespace ConcursNetworking.dto
{
    [Serializable]
    public class ParticipantDTO
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        
        [JsonProperty("name")]
        public string Name { get; set; }
        
        [JsonProperty("age")]
        public int Age { get; set; }

        public ParticipantDTO()
        {
            Name = string.Empty;
        }

        public ParticipantDTO(Participant participant)
        {
            Id = participant.Id;
            Name = participant.Name;
            Age = participant.Age;
        }
    }
}