using System;
using System.Collections.Generic;
using System.Data;
using Laborator3.domain;
using Laborator3.repository_utils;
using SwimmingCompetition.exception;
using log4net;

namespace Laborator3.repository
{
    public class InscriereRepositoryImpl : IInscriereRepository
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IDictionary<string, string> properties;

        public InscriereRepositoryImpl(IDictionary<string, string> properties)
        {
            log.Info("Creating InscriereRepository");
            this.properties = properties;
        }

        private Event? GetEventFromResult(IDataReader dataReader)
        {
            int id = Convert.ToInt32(dataReader["id"]);
            string distance = dataReader["distance"].ToString();
            string style = dataReader["style"].ToString();

            return new Event(distance, style) { Id = id };
        }

        private Participant? GetParticipantFromResult(IDataReader dataReader)
        {
            int id = Convert.ToInt32(dataReader["id"]);
            string name = dataReader["name"].ToString();
            int age = Convert.ToInt32(dataReader["age"]);

            return new Participant(name, age) { Id = id };
        }

        public IEnumerable<Event> FindAllEventsByParticipant(int participantId)
        {
            log.Info("Finding all events by participant");

            List<Event> events = new List<Event>();
            IDbConnection connection = DBUtils.getConnection(properties);

            if (participantId == 0)
            {
                throw new RepositoryException("Participant Id is null or invalid");
            }

            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT e.id, e.distance, e.style FROM events e " +
                                      "JOIN event_participants ep ON e.id = ep.event_id " +
                                      "WHERE ep.participant_id = @participantId";
                IDbDataParameter parameter = command.CreateParameter();
                parameter.ParameterName = "@participantId";
                parameter.Value = participantId;
                command.Parameters.Add(parameter);

                using (var dataReader = command.ExecuteReader())
                {
                    while (dataReader.Read())
                    {
                        Event? eventObj = GetEventFromResult(dataReader);
                        events.Add(eventObj);
                    }
                }
            }

            log.InfoFormat("Exiting FindAllEventsByParticipant with value: " + events);
            return events;
        }

        public IEnumerable<Participant> FindAllParticipantsByEvent(int eventId)
        {
            log.Info("Finding all participants by event");

            List<Participant> participants = new List<Participant>();
            IDbConnection connection = DBUtils.getConnection(properties);

            if (eventId == 0)
            {
                throw new RepositoryException("Event Id is null or invalid");
            }

            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT p.id, p.name, p.age FROM participants p " +
                                      "JOIN event_participants ep ON p.id = ep.participant_id " +
                                      "WHERE ep.event_id = @eventId";
                IDbDataParameter parameter = command.CreateParameter();
                parameter.ParameterName = "@eventId";
                parameter.Value = eventId;
                command.Parameters.Add(parameter);

                using (var dataReader = command.ExecuteReader())
                {
                    while (dataReader.Read())
                    {
                        Participant? participant = GetParticipantFromResult(dataReader);
                        participants.Add(participant);
                    }
                }
            }

            log.InfoFormat("Exiting FindAllParticipantsByEvent with value: " + participants);
            return participants;
        }

        public void SaveEventParticipant(int eventId, int participantId)
        {
            log.Info("Saving Event Participant");

            IDbConnection connection = DBUtils.getConnection(properties);

            if (eventId == 0 || participantId == 0)
            {
                throw new RepositoryException("Event Id or Participant Id is null or invalid");
            }

            using (var command = connection.CreateCommand())
            {
                command.CommandText = "INSERT INTO event_participants (event_id, participant_id) " +
                                      "VALUES (@eventId, @participantId)";
                var eventIdParam = command.CreateParameter();
                eventIdParam.ParameterName = "@eventId";
                eventIdParam.Value = eventId;
                command.Parameters.Add(eventIdParam);

                var participantIdParam = command.CreateParameter();
                participantIdParam.ParameterName = "@participantId";
                participantIdParam.Value = participantId;
                command.Parameters.Add(participantIdParam);

                command.ExecuteNonQuery();
                log.InfoFormat("Event Participant saved with eventId: {0} and participantId: {1}", eventId, participantId);
            }
        }

        public void DeleteEventParticipant(int eventId, int participantId)
        {
            log.Info("Deleting Event Participant");

            IDbConnection connection = DBUtils.getConnection(properties);

            if (eventId == 0 || participantId == 0)
            {
                throw new RepositoryException("Event Id or Participant Id is null or invalid");
            }

            using (var command = connection.CreateCommand())
            {
                command.CommandText = "DELETE FROM event_participants WHERE event_id = @eventId AND participant_id = @participantId";
                var eventIdParam = command.CreateParameter();
                eventIdParam.ParameterName = "@eventId";
                eventIdParam.Value = eventId;
                command.Parameters.Add(eventIdParam);

                var participantIdParam = command.CreateParameter();
                participantIdParam.ParameterName = "@participantId";
                participantIdParam.Value = participantId;
                command.Parameters.Add(participantIdParam);

                int rowsAffected = command.ExecuteNonQuery();
                if (rowsAffected == 0)
                {
                    throw new RepositoryException("Event Participant not found for deletion.");
                }

                log.InfoFormat("Event Participant deleted with eventId: {0} and participantId: {1}", eventId, participantId);
            }
        }
    }
}
