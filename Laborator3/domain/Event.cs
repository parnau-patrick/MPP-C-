using System;

namespace Laborator3.domain
{
    public class Event : Entity<int>
    {
        public string Distance { get; set; }
        public string Style { get; set; }

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