using System;
using System.Collections.Generic;
using System.Data;
using ConcursModel.domain;
using ConcursModel.domain.validator;
using ConcursModel.exception;
using ConcursPersistence.repository.Interface;
using ConcursPersistence.utils;
using ConcursModel.domain;
using ConcursModel.domain.validator;
using ConcursPersistence.utils;
using log4net;
using ConcursPersistence.exception;
using ConcursPersistence.repository.Interface;

namespace ConcursPersistence.repository.impl
{
    public class EventRepositoryImpl : IEventRepository
    {
        private static readonly ILog log =
            LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private IValidator<Event>? _eventValidator;

        private IDictionary<string, string> properties;

        public EventRepositoryImpl(IDictionary<string, string> properties, IValidator<Event>? eventValidator)
        {
            log.Info("Creating EventDatabaseRepository");

            this.properties = properties;
            this._eventValidator = eventValidator;
        }

        private Event GetEventFromResult(IDataReader dataReader)
        {
            try
            {
                int id = Convert.ToInt32(dataReader["id"]);
                string distance = dataReader["distance"].ToString();
                string style = dataReader["style"].ToString();

                Event? eventObj = new Event(distance, style)
                {
                    Id = id
                };
                return eventObj;
            }
            catch (Exception ex)
            {
                log.Error("Error parsing event from database: " + ex.Message, ex);
                throw new RepositoryException("Error parsing event data: " + ex.Message, ex);
            }
        }

        public Event? FindOne(int id)
        {
            log.Info("Finding Event with id: " + id);

            try
            {
                IDbConnection connection = DBUtils.GetConnection(properties);

                if (id == 0)
                {
                    throw new RepositoryException("Event Id is null");
                }

                Event? eventObj = null;
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "select * from events where id = @id";
                    IDbDataParameter parameter = command.CreateParameter();
                    parameter.ParameterName = "@id";
                    parameter.Value = id;

                    command.Parameters.Add(parameter);

                    using (var dataReader = command.ExecuteReader())
                    {
                        if (dataReader.Read())
                        {
                            eventObj = GetEventFromResult(dataReader);
                            log.InfoFormat("Exiting FindOne Event with value: " + eventObj);
                        }
                    }
                }

                log.InfoFormat("Exiting FindOne Event with value: " + eventObj);
                return eventObj;
            }
            catch (RepositoryException)
            {
                // Just rethrow repository exceptions
                throw;
            }
            catch (Exception ex)
            {
                log.Error("Error finding event: " + ex.Message, ex);
                throw new RepositoryException("Error finding event: " + ex.Message, ex);
            }
        }

        public IEnumerable<Event> FindAll()
        {
            log.Info("Finding All Events");
            
            try
            {
                IDbConnection connection = DBUtils.GetConnection(properties);
                IList<Event> events = new List<Event>();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "select * from events";

                    using (var dataReader = command.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            Event eventObj = GetEventFromResult(dataReader);
                            events.Add(eventObj);
                        }
                    }
                }

                log.InfoFormat("Exiting FindAll events with count: " + events.Count);
                return events;
            }
            catch (Exception ex)
            {
                log.Error("Error finding all events: " + ex.Message, ex);
                throw new RepositoryException("Error finding all events: " + ex.Message, ex);
            }
        }

        public Event? Save(Event eventObj)
        {
            log.Info("Saving Event: " + eventObj);
            
            try
            {
                IDbConnection connection = DBUtils.GetConnection(properties);

                _eventValidator.Validate(eventObj);

                if (eventObj.Id != 0)
                {
                    Event? eventFound = FindOne(eventObj.Id);
                    if (eventFound != null)
                    {
                        log.InfoFormat("Event already exists with value: " + eventFound);
                        throw new RepositoryException("Event already exists");
                    }
                }

                using (var command = connection.CreateCommand())
                {
                    // Insert event without specifying ID (auto-incremented)
                    command.CommandText = "INSERT INTO events (distance, style) VALUES (@distance, @style)";
            
                    var distanceParameter = command.CreateParameter();
                    distanceParameter.ParameterName = "@distance";
                    distanceParameter.Value = eventObj.Distance;
                    command.Parameters.Add(distanceParameter);

                    var styleParameter = command.CreateParameter();
                    styleParameter.ParameterName = "@style";
                    styleParameter.Value = eventObj.Style;
                    command.Parameters.Add(styleParameter);

                    command.ExecuteNonQuery();

                    // Get last inserted ID (correct way for SQLite)
                    command.CommandText = "SELECT last_insert_rowid()";
                    var lastInsertedId = command.ExecuteScalar();

                    // Assign the generated ID to the event object
                    eventObj.Id = Convert.ToInt32(lastInsertedId);

                    log.InfoFormat("Event saved with value: " + eventObj);
                }

                return eventObj;
            }
            catch (ValidationException)
            {
                // Just rethrow validation exceptions
                throw;
            }
            catch (RepositoryException)
            {
                // Just rethrow repository exceptions
                throw;
            }
            catch (Exception ex)
            {
                log.Error("Error saving event: " + ex.Message, ex);
                throw new RepositoryException("Error saving event: " + ex.Message, ex);
            }
        }

        public Event? Delete(int id)
        {
            log.Info("Deleting Event with id: " + id);
            
            try
            {
                IDbConnection connection = DBUtils.GetConnection(properties);

                if (id == 0)
                {
                    throw new RepositoryException("Event Id is null");
                }

                Event? eventFound = FindOne(id);
                if (eventFound == null)
                {
                    log.InfoFormat("Event is not found");
                    throw new RepositoryException("Event is not found");
                }

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "delete from events where id = @id";
                    IDbDataParameter parameter = command.CreateParameter();
                    parameter.ParameterName = "@id";
                    parameter.Value = id;
                    command.Parameters.Add(parameter);

                    command.ExecuteNonQuery();

                    log.InfoFormat("Event deleted with value: " + eventFound);
                }

                return eventFound;
            }
            catch (RepositoryException)
            {
                // Just rethrow repository exceptions
                throw;
            }
            catch (Exception ex)
            {
                log.Error("Error deleting event: " + ex.Message, ex);
                throw new RepositoryException("Error deleting event: " + ex.Message, ex);
            }
        }

        public Event? Update(Event eventObj)
        {
            log.Info("Updating Event: " + eventObj);
            
            try
            {
                IDbConnection connection = DBUtils.GetConnection(properties);

                _eventValidator.Validate(eventObj);

                Event? eventFound = FindOne(eventObj.Id);
                if (eventFound == null)
                {
                    log.InfoFormat("Event is not found");
                    throw new RepositoryException("Event is not found");
                }

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "update events set distance=@distance, style=@style where id = @id";
                    IDbDataParameter distanceParameter = command.CreateParameter();
                    distanceParameter.ParameterName = "@distance";
                    distanceParameter.Value = eventObj.Distance;
                    command.Parameters.Add(distanceParameter);

                    IDbDataParameter styleParameter = command.CreateParameter();
                    styleParameter.ParameterName = "@style";
                    styleParameter.Value = eventObj.Style;
                    command.Parameters.Add(styleParameter);

                    IDbDataParameter parameter = command.CreateParameter();
                    parameter.ParameterName = "@id";
                    parameter.Value = eventObj.Id;
                    command.Parameters.Add(parameter);

                    command.ExecuteNonQuery();

                    log.InfoFormat("Event updated with value: " + eventObj);
                }

                return eventObj;
            }
            catch (ValidationException)
            {
                // Just rethrow validation exceptions
                throw;
            }
            catch (RepositoryException)
            {
                // Just rethrow repository exceptions
                throw;
            }
            catch (Exception ex)
            {
                log.Error("Error updating event: " + ex.Message, ex);
                throw new RepositoryException("Error updating event: " + ex.Message, ex);
            }
        }
    }
}