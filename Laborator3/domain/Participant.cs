using System.Collections.Generic;

namespace Laborator3.domain
{
    public class Participant : Entity<int>
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public List<Event> Events { get; set; } = new List<Event>();

        public Participant(string name, int age)
        {
            Name = name;
            Age = age;
        }
    }
}