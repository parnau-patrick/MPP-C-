using System;
using System.Collections.Generic;
using ConcursModel.domain;

namespace ConcursPersistence.repository.Interface
{
    public interface IInscriereRepository : IRepository<int, Inscriere>
    {
        IEnumerable<Event> FindAllEventsByParticipant(int participantId);
        IEnumerable<Participant> FindAllParticipantsByEvent(int eventId);
        void SaveEventParticipant(int eventId, int participantId);
        void DeleteEventParticipant(int eventId, int participantId);
    }
}