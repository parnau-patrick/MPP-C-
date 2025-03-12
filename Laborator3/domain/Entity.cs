namespace Laborator3.domain
{
    public class Entity<ID>
    {
        public ID Id { get; set; }

        public Entity() { }

        public Entity(ID id)
        {
            Id = id;
        }

        public override bool Equals(object obj)
        {
            if (obj is Entity<ID> other)
            {
                return Equals(Id, other.Id);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Id == null ? 0 : Id.GetHashCode();
        }
    }
}