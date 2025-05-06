using System;
using System.Collections.Generic;
using ConcursModel.domain;

namespace ConcursPersistence.repository.Interface
{
    public interface IEventRepository : IRepository<int, Event>
    {
        // Additional methods specific to events can be added here
    }
}