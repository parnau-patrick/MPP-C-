using System.Collections.Generic;
using Laborator3.domain;

namespace Laborator3.repository
{
    public interface IInscriereRepository
    {
        /// <summary>
        /// Găsește toate evenimentele la care participă un participant pe baza ID-ului participantului.
        /// </summary>
        /// <param name="participantId">ID-ul participantului</param>
        /// <returns>Lista de evenimente la care participă participantul</returns>
        IEnumerable<Event> FindAllEventsByParticipant(int participantId);

        /// <summary>
        /// Găsește toți participanți care participă la un eveniment pe baza ID-ului evenimentului.
        /// </summary>
        /// <param name="eventId">ID-ul evenimentului</param>
        /// <returns>Lista de participanți ai evenimentului</returns>
        IEnumerable<Participant> FindAllParticipantsByEvent(int eventId);

        /// <summary>
        /// Salvează relația dintre un eveniment și un participant (adică adaugă participantul la un eveniment).
        /// </summary>
        /// <param name="eventId">ID-ul evenimentului</param>
        /// <param name="participantId">ID-ul participantului</param>
        void SaveEventParticipant(int eventId, int participantId);

        /// <summary>
        /// Șterge relația dintre un participant și un eveniment.
        /// </summary>
        /// <param name="eventId">ID-ul evenimentului</param>
        /// <param name="participantId">ID-ul participantului</param>
        void DeleteEventParticipant(int eventId, int participantId);
    }
}
