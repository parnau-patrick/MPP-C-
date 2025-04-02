using System;
using System.Collections.Generic;
using Laborator3.domain;

namespace Laborator3.repository.Interface   
{
    public interface IRepository<ID, E> where E : Entity<ID>
    {
        E FindOne(ID id);
        IEnumerable<E> FindAll();
        E Save(E entity);
        E Delete(ID id);
        E Update(E entity);
    }
}