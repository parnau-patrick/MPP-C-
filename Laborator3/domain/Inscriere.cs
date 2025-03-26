

using System;
using System.Collections.Generic;

namespace Laborator3.domain
{
    public class Inscriere : Entity<int>
    {
        public Participant Participant { get; set; }
        public List<Event> Events { get; set; } = new List<Event>();

        public Inscriere(Participant participant)
        {
            Participant = participant;
        }
    }
}