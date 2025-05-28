using Newtonsoft.Json;
using System.Collections.Generic;

namespace ConcursNetworking.dto
{
    [JsonObject(MemberSerialization.OptIn)]
    public class RegistrationDTO
    {
        [JsonProperty("participantName")]
        public string ParticipantName { get; set; }
        
        [JsonProperty("participantAge")]
        public int ParticipantAge { get; set; }
        
        [JsonProperty("eventIds")]
        public List<int> EventIds { get; set; }

        public RegistrationDTO() 
        {
            EventIds = new List<int>();
        }

        public RegistrationDTO(string participantName, int participantAge, List<int> eventIds)
        {
            ParticipantName = participantName;
            ParticipantAge = participantAge;
            EventIds = eventIds;
        }
    }
}