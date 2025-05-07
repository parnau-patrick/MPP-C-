using System;
using System.Collections.Generic;
using System.Data;
using ConcursModel.domain;
using ConcursPersistence.utils;
using ConcursModel.exception;
using ConcursPersistence.repository.Interface;
using ConcursPersistence.exception;
using log4net;

namespace ConcursPersistence.repository
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
            try
            {
                int id = Convert.ToInt32(dataReader["id"]);
                string distance = dataReader["distance"].ToString();
                string style = dataReader["style"].ToString();

                return new Event(distance, style) { Id = id };
            }
            catch (Exception ex)
            {
                log.Error("Error parsing event from database: " + ex.Message, ex);
                throw new RepositoryException("Error parsing event data: " + ex.Message, ex);
            }
        }

        private Participant? GetParticipantFromResult(IDataReader dataReader)
        {
            try
            {
                int id = Convert.ToInt32(dataReader["id"]);
                string name = dataReader["name"].ToString();
                int age = Convert.ToInt32(dataReader["age"]);

                return new Participant(name, age) { Id = id };
            }
            catch (Exception ex)
            {
                log.Error("Error parsing participant from database: " + ex.Message, ex);
                throw new RepositoryException("Error parsing participant data: " + ex.Message, ex);
            }
        }

        private Inscriere? GetInscriereFromResult(IDataReader dataReader)
        {
            try
            {
                int id = Convert.ToInt32(dataReader["id"]);
                int eventId = Convert.ToInt32(dataReader["event_id"]);
                int participantId = Convert.ToInt32(dataReader["participant_id"]);

                return new Inscriere(eventId, participantId) { Id = id };
            }
            catch (Exception ex)
            {
                log.Error("Error parsing inscriere from database: " + ex.Message, ex);
                throw new RepositoryException("Error parsing inscriere data: " + ex.Message, ex);
            }
        }

        public IEnumerable<Event> FindAllEventsByParticipant(int participantId)
        {
            log.Info("Finding all events by participant: " + participantId);

            try
            {
                List<Event> events = new List<Event>();
                IDbConnection connection = DBUtils.GetConnection(properties);

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

                log.InfoFormat("Found {0} events for participant ID {1}", events.Count, participantId);
                return events;
            }
            catch (RepositoryException)
            {
                // Just rethrow repository exceptions
                throw;
            }
            catch (Exception ex)
            {
                log.Error("Error finding events by participant: " + ex.Message, ex);
                throw new RepositoryException("Error finding events by participant (ID: " + participantId + "): " + ex.Message, ex);
            }
        }

        public IEnumerable<Participant> FindAllParticipantsByEvent(int eventId)
        {
            log.Info("Finding all participants by event: " + eventId);

            try
            {
                List<Participant> participants = new List<Participant>();
                IDbConnection connection = DBUtils.GetConnection(properties);

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

                log.InfoFormat("Found {0} participants for event ID {1}", participants.Count, eventId);
                return participants;
            }
            catch (RepositoryException)
            {
                // Just rethrow repository exceptions
                throw;
            }
            catch (Exception ex)
            {
                log.Error("Error finding participants by event: " + ex.Message, ex);
                throw new RepositoryException("Error finding participants by event (ID: " + eventId + "): " + ex.Message, ex);
            }
        }

        public void SaveEventParticipant(int eventId, int participantId)
        {
            log.Info("Saving Event Participant - Event ID: " + eventId + ", Participant ID: " + participantId);

            try
            {
                IDbConnection connection = DBUtils.GetConnection(properties);

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
                    log.InfoFormat("Event Participant saved successfully with eventId: {0} and participantId: {1}", eventId, participantId);
                }
            }
            catch (RepositoryException)
            {
                // Just rethrow repository exceptions
                throw;
            }
            catch (Exception ex)
            {
                log.Error("Error saving event participant: " + ex.Message, ex);
                throw new RepositoryException(
                    string.Format("Error registering participant ID {0} for event ID {1}: {2}", 
                        participantId, eventId, ex.Message), ex);
            }
        }

        public void DeleteEventParticipant(int eventId, int participantId)
        {
            log.Info("Deleting Event Participant - Event ID: " + eventId + ", Participant ID: " + participantId);

            try
            {
                IDbConnection connection = DBUtils.GetConnection(properties);

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

                    log.InfoFormat("Event Participant deleted successfully with eventId: {0} and participantId: {1}", eventId, participantId);
                }
            }
            catch (RepositoryException)
            {
                // Just rethrow repository exceptions
                throw;
            }
            catch (Exception ex)
            {
                log.Error("Error deleting event participant: " + ex.Message, ex);
                throw new RepositoryException(
                    string.Format("Error removing participant ID {0} from event ID {1}: {2}", 
                        participantId, eventId, ex.Message), ex);
            }
        }

        public Inscriere FindOne(int id)
        {
            log.Info("Finding Inscriere with ID: " + id);

            try
            {
                IDbConnection connection = DBUtils.GetConnection(properties);

                if (id == 0)
                {
                    throw new RepositoryException("Inscriere Id is null or invalid");
                }

                Inscriere? inscriere = null;
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT id, event_id, participant_id FROM event_participants WHERE id = @id";
                    IDbDataParameter parameter = command.CreateParameter();
                    parameter.ParameterName = "@id";
                    parameter.Value = id;
                    command.Parameters.Add(parameter);

                    using (var dataReader = command.ExecuteReader())
                    {
                        if (dataReader.Read())
                        {
                            inscriere = GetInscriereFromResult(dataReader);
                            log.InfoFormat("Found Inscriere with id: {0}, eventId: {1}, participantId: {2}", 
                                inscriere.Id, inscriere.EventId, inscriere.ParticipantId);
                        }
                    }
                }

                if (inscriere == null)
                {
                    throw new RepositoryException("Inscriere not found with ID: " + id);
                }

                return inscriere;
            }
            catch (RepositoryException)
            {
                // Just rethrow repository exceptions
                throw;
            }
            catch (Exception ex)
            {
                log.Error("Error finding inscriere: " + ex.Message, ex);
                throw new RepositoryException("Error finding registration with ID " + id + ": " + ex.Message, ex);
            }
        }

        public IEnumerable<Inscriere> FindAll()
        {
            log.Info("Finding All Inscrieri");

            try
            {
                List<Inscriere> inscrieri = new List<Inscriere>();
                IDbConnection connection = DBUtils.GetConnection(properties);

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT id, event_id, participant_id FROM event_participants";

                    using (var dataReader = command.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            Inscriere? inscriere = GetInscriereFromResult(dataReader);
                            inscrieri.Add(inscriere);
                        }
                    }
                }

                log.InfoFormat("Found {0} registrations", inscrieri.Count);
                return inscrieri;
            }
            catch (Exception ex)
            {
                log.Error("Error finding all registrations: " + ex.Message, ex);
                throw new RepositoryException("Error retrieving all registrations: " + ex.Message, ex);
            }
        }

        public Inscriere Save(Inscriere inscriere)
        {
            log.Info("Saving Inscriere - Event ID: " + inscriere.EventId + ", Participant ID: " + inscriere.ParticipantId);

            try
            {
                SaveEventParticipant(inscriere.EventId, inscriere.ParticipantId);
                
                // Need to retrieve the generated ID
                IDbConnection connection = DBUtils.GetConnection(properties);
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT last_insert_rowid()";
                    inscriere.Id = Convert.ToInt32(command.ExecuteScalar());
                }
                
                log.InfoFormat("Inscriere saved successfully with ID: {0}", inscriere.Id);
                return inscriere;
            }
            catch (RepositoryException)
            {
                // Just rethrow repository exceptions
                throw;
            }
            catch (Exception ex)
            {
                log.Error("Error saving registration: " + ex.Message, ex);
                throw new RepositoryException("Error saving registration: " + ex.Message, ex);
            }
        }

        public Inscriere Delete(int id)
        {
            log.Info("Deleting Inscriere with ID: " + id);

            try
            {
                Inscriere inscriere = FindOne(id);
                
                IDbConnection connection = DBUtils.GetConnection(properties);
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "DELETE FROM event_participants WHERE id = @id";
                    var parameter = command.CreateParameter();
                    parameter.ParameterName = "@id";
                    parameter.Value = id;
                    command.Parameters.Add(parameter);

                    int rowsAffected = command.ExecuteNonQuery();
                    if (rowsAffected == 0)
                    {
                        throw new RepositoryException("Inscriere not found for deletion with ID: " + id);
                    }

                    log.InfoFormat("Inscriere deleted successfully with ID: {0}", id);
                }

                return inscriere;
            }
            catch (RepositoryException)
            {
                // Just rethrow repository exceptions
                throw;
            }
            catch (Exception ex)
            {
                log.Error("Error deleting registration: " + ex.Message, ex);
                throw new RepositoryException("Error deleting registration with ID " + id + ": " + ex.Message, ex);
            }
        }

        public Inscriere Update(Inscriere inscriere)
        {
            log.Info("Updating Inscriere with ID: " + inscriere.Id);

            try
            {
                // First check if the entry exists
                FindOne(inscriere.Id);
                
                IDbConnection connection = DBUtils.GetConnection(properties);
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "UPDATE event_participants SET event_id = @eventId, participant_id = @participantId WHERE id = @id";
                    
                    var eventIdParam = command.CreateParameter();
                    eventIdParam.ParameterName = "@eventId";
                    eventIdParam.Value = inscriere.EventId;
                    command.Parameters.Add(eventIdParam);

                    var participantIdParam = command.CreateParameter();
                    participantIdParam.ParameterName = "@participantId";
                    participantIdParam.Value = inscriere.ParticipantId;
                    command.Parameters.Add(participantIdParam);
                    
                    var idParam = command.CreateParameter();
                    idParam.ParameterName = "@id";
                    idParam.Value = inscriere.Id;
                    command.Parameters.Add(idParam);

                    int rowsAffected = command.ExecuteNonQuery();
                    if (rowsAffected == 0)
                    {
                        throw new RepositoryException("Inscriere not found for update with ID: " + inscriere.Id);
                    }

                    log.InfoFormat("Inscriere updated successfully with ID: {0}", inscriere.Id);
                }

                return inscriere;
            }
            catch (RepositoryException)
            {
                // Just rethrow repository exceptions
                throw;
            }
            catch (Exception ex)
            {
                log.Error("Error updating registration: " + ex.Message, ex);
                throw new RepositoryException("Error updating registration with ID " + inscriere.Id + ": " + ex.Message, ex);
            }
        }
    }
}