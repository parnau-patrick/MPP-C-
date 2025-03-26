using System.Collections.Generic;

namespace Laborator3.domain
{
    public class Event : Entity<int>
    {
        public string Distance { get; set; }
        public string Style { get; set; }
        public List<Participant> Participants { get; set; } = new List<Participant>();

        public Event(string distance, string style)
        {
            Distance = distance;
            Style = style;
        }
    }
}