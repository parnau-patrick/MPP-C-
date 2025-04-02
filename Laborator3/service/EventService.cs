using System;
using System.Collections.Generic;
using Laborator3.domain;
using Laborator3.repository;
using Laborator3.observer;
using Laborator3.repository.Interface;
using log4net;

namespace Laborator3.service
{
    public class EventService : IObservable
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IEventRepository eventRepository;
        private readonly IInscriereRepository inscriereRepository;
        private readonly List<IObserver> observers = new List<IObserver>();

        public EventService(IEventRepository eventRepository, IInscriereRepository inscriereRepository)
        {
            log.Info("Creating EventService");
            this.eventRepository = eventRepository;
            this.inscriereRepository = inscriereRepository;
        }

        public IEnumerable<Event> FindAllEvents()
        {
            log.Info("EventService - FindAllEvents");
            return eventRepository.FindAll();
        }

        public IEnumerable<Event> FindAllEventsWithParticipantCounts()
        {
            log.Info("EventService - FindAllEventsWithParticipantCounts");
            var events = eventRepository.FindAll();
            
            // Create a new list to avoid modifying the original events
            var eventsWithCounts = new List<Event>();
            
            foreach (var evt in events)
            {
                // Create a copy of the event
                Event eventWithCount = new Event(evt.Distance, evt.Style)
                {
                    Id = evt.Id
                };
                
                eventsWithCounts.Add(eventWithCount);
            }
            
            return eventsWithCounts;
        }

        public int GetParticipantCountForEvent(int eventId)
        {
            log.Info($"EventService - GetParticipantCountForEvent with id: {eventId}");
            var participants = inscriereRepository.FindAllParticipantsByEvent(eventId);
            int count = 0;
            
            foreach (var participant in participants)
            {
                count++;
            }
            
            return count;
        }

        public Event FindEventById(int id)
        {
            log.Info($"EventService - FindEventById with id: {id}");
            return eventRepository.FindOne(id);
        }

        public void AddObserver(IObserver observer)
        {
            log.Info($"EventService - Adding observer: {observer}");
            observers.Add(observer);
        }

        public void RemoveObserver(IObserver observer)
        {
            log.Info($"EventService - Removing observer: {observer}");
            observers.Remove(observer);
        }

        public void NotifyObservers()
        {
            log.Info("EventService - Notifying observers");
            foreach (var observer in observers)
            {
                observer.Update();
            }
        }
    }
}