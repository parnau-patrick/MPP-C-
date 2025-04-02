using System;

namespace Laborator3.domain
{
    public class Inscriere : Entity<int>
    {
        public int EventId { get; set; }
        public int ParticipantId { get; set; }

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