using System;
using ConcursModel.domain;
using Newtonsoft.Json;

namespace ConcursNetworking.dto
{
    [Serializable]
    public class EventDTO
    {
        public int Id { get; set; }
        public string Distance { get; set; }
        public string Style { get; set; }
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