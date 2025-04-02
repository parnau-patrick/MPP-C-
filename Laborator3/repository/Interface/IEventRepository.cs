using System;
using System.Collections.Generic;
using Laborator3.domain;

namespace Laborator3.repository.Interface
{
    public interface IEventRepository : IRepository<int, Event>
    {
        // Additional methods specific to events can be added here
    }
}