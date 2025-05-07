using System;

namespace ConcursModel.domain
{
    [Serializable]
    public class Participant : Entity<int>
    {
        public string Name { get; set; }
        public int Age { get; set; }

        public Participant()
        {
            Name = string.Empty;
        }

        public Participant(string name, int age)
        {
            Name = name;
            Age = age;
        }

        public override string ToString()
        {
            return $"{Name} ({Age} years)";
        }
    }
}