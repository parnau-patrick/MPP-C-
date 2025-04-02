using System;
using System.Collections.Generic;
using Laborator3.domain;

namespace Laborator3.repository.Interface
{
    public interface IParticipantRepository : IRepository<int, Participant>
    {
        // Additional methods specific to participants can be added here
    }
}