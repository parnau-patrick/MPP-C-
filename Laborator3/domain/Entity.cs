using System;

namespace Laborator3.domain
{
    public abstract class Entity<ID>
    {
        public ID Id { get; set; }

        public override string ToString()
        {
            return $"Entity{{id={Id}}}";
        }

        public override bool Equals(object obj)
        {
            if (this == obj) return true;
            if (obj == null || GetType() != obj.GetType()) return false;
            Entity<ID> entity = (Entity<ID>)obj;
            return Id.Equals(entity.Id);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}