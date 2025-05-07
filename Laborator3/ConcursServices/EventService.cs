using System;
using System.Collections.Generic;
using ConcursModel.domain;
using ConcursModel.domain.observer;
using ConcursPersistence.repository.Interface;
using log4net;

namespace ConcursServices
{
    public class EventService : IObservable
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(EventService));
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
            return events;
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

        public Event SaveEvent(Event eventObj)
        {
            log.Info($"EventService - SaveEvent: {eventObj}");
            Event result = eventRepository.Save(eventObj);
            NotifyObservers();
            return result;
        }

        public Event UpdateEvent(Event eventObj)
        {
            log.Info($"EventService - UpdateEvent: {eventObj}");
            Event result = eventRepository.Update(eventObj);
            NotifyObservers();
            return result;
        }

        public Event DeleteEvent(int id)
        {
            log.Info($"EventService - DeleteEvent with id: {id}");
            Event result = eventRepository.Delete(id);
            NotifyObservers();
            return result;
        }

        public void AddObserver(IObserver observer)
        {
            log.Info($"EventService - Adding observer");
            observers.Add(observer);
        }

        public void RemoveObserver(IObserver observer)
        {
            log.Info($"EventService - Removing observer");
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