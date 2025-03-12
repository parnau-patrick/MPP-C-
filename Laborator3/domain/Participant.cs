using System.Collections.Generic;

namespace Laborator3.domain
{
    public class Participant : Entity<long>
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public List<Proba> Probas { get; set; }

        public Participant() { }

        public Participant(long id, string name, int age, List<Proba> probas)
        {
            Id = id;
            Name = name;
            Age = age;
            Probas = probas;
        }
    }
}