using System;
using Newtonsoft.Json;

namespace ConcursModel.domain
{
    [Serializable]
    [JsonObject(MemberSerialization.OptIn)]
    public class Participant : Entity<int>
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        
        [JsonProperty("age")]
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