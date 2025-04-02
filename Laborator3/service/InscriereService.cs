using System;
using System.Collections.Generic;
using Laborator3.domain;
using Laborator3.repository;
using Laborator3.repository.Interface;
using log4net;

namespace Laborator3.service
{
    public class InscriereService
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IInscriereRepository inscriereRepository;
        private readonly EventService eventService;

        public InscriereService(IInscriereRepository inscriereRepository, EventService eventService)
        {
            log.Info("Creating InscriereService");
            this.inscriereRepository = inscriereRepository;
            this.eventService = eventService;
        }

        public IEnumerable<Event> FindAllEventsByParticipant(int participantId)
        {
            log.Info($"InscriereService - FindAllEventsByParticipant with id: {participantId}");
            return inscriereRepository.FindAllEventsByParticipant(participantId);
        }

        public IEnumerable<Participant> FindAllParticipantsByEvent(int eventId)
        {
            log.Info($"InscriereService - FindAllParticipantsByEvent with id: {eventId}");
            return inscriereRepository.FindAllParticipantsByEvent(eventId);
        }

        public void RegisterParticipant(int participantId, int eventId)
        {
            log.Info($"InscriereService - RegisterParticipant with participantId: {participantId} and eventId: {eventId}");
            inscriereRepository.SaveEventParticipant(eventId, participantId);
            
            // Notify observers that a new registration has been made
            eventService.NotifyObservers();
        }

        public int CountEventsForParticipant(int participantId)
        {
            log.Info($"InscriereService - CountEventsForParticipant with id: {participantId}");
            var events = inscriereRepository.FindAllEventsByParticipant(participantId);
            int count = 0;
            
            foreach (var evt in events)
            {
                count++;
            }
            
            return count;
        }
    }
}