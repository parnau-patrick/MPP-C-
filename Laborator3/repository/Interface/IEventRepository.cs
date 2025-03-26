using Laborator3.domain;
using Laborator3.repository;

namespace Laborator3.repository
{


    public interface IEventRepository : IRepository<int, Event>
    {
    }
}