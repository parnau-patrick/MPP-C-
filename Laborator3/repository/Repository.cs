
namespace Laborator3.Repository ;

public interface IRepository<ID, E>
{
    E FindOne(ID id);
    IEnumerable<E> FindAll();
    E Save(E entity);
    E Update(E entity);
    E Delete(ID id);
}