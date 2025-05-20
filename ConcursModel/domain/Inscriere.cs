using System;
using Newtonsoft.Json;

namespace ConcursModel.domain
{
    [Serializable]
    [JsonObject(MemberSerialization.OptIn)]
    public class Inscriere : Entity<int>
    {
        [JsonProperty("eventId")]
        public int EventId { get; set; }
        
        [JsonProperty("participantId")]
        public int ParticipantId { get; set; }

        public Inscriere() { }

        public Inscriere(int eventId, int participantId)
        {
            EventId = eventId;
            ParticipantId = participantId;
        }

        public override string ToString()
        {
            return $"Inscriere{{id={Id}, eventId={EventId}, participantId={ParticipantId}}}";
        }
    }
}