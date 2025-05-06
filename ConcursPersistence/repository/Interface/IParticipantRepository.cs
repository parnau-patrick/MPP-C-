using System;
using System.Collections.Generic;
using ConcursModel.domain;

namespace ConcursPersistence.repository.Interface
{
    public interface IParticipantRepository : IRepository<int, Participant>
    {
        // Additional methods specific to participants can be added here
    }
}