using System;
using System.Collections.Generic;
using System.Data;
using Laborator3.domain;
using Laborator3.domain.validator;
using Laborator3.repository_utils;
using Laborator3.exception;
using Laborator3.repository.Interface;
using log4net;

namespace Laborator3.repository
{
    public class ParticipantRepositoryImpl : IParticipantRepository
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IDictionary<string, string> properties;
        private readonly IValidator<Participant> _validator;

        public ParticipantRepositoryImpl(IDictionary<string, string> properties, IValidator<Participant> validator)
        {
            log.Info("Creating ParticipantRepository");
            this.properties = properties;
            this._validator = validator;
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

        public Participant? FindOne(int id)
        {
            log.Info("Finding Participant with ID: " + id);

            try
            {
                IDbConnection connection = DBUtils.getConnection(properties);

                if (id == 0)
                {
                    throw new RepositoryException("Participant Id is null or invalid");
                }

                Participant? participant = null;
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT id, name, age FROM participants WHERE id = @id";
                    IDbDataParameter parameter = command.CreateParameter();
                    parameter.ParameterName = "@id";
                    parameter.Value = id;

                    command.Parameters.Add(parameter);

                    using (var dataReader = command.ExecuteReader())
                    {
                        if (dataReader.Read())
                        {
                            participant = GetParticipantFromResult(dataReader);
                            log.InfoFormat("Found participant with ID {0}: {1}, {2} years", 
                                participant.Id, participant.Name, participant.Age);
                        }
                    }
                }

                if (participant == null)
                {
                    log.InfoFormat("No participant found with ID: " + id);
                }
                
                return participant;
            }
            catch (RepositoryException)
            {
                // Just rethrow repository exceptions
                throw;
            }
            catch (Exception ex)
            {
                log.Error("Error finding participant: " + ex.Message, ex);
                throw new RepositoryException("Error finding participant with ID " + id + ": " + ex.Message, ex);
            }
        }

        public IEnumerable<Participant> FindAll()
        {
            log.Info("Finding All Participants");

            try
            {
                List<Participant> participants = new List<Participant>();
                IDbConnection connection = DBUtils.getConnection(properties);

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT id, name, age FROM participants";

                    using (var dataReader = command.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            Participant participant = GetParticipantFromResult(dataReader);
                            participants.Add(participant);
                        }
                    }
                }

                log.InfoFormat("Found {0} participants", participants.Count);
                return participants;
            }
            catch (Exception ex)
            {
                log.Error("Error finding all participants: " + ex.Message, ex);
                throw new RepositoryException("Error retrieving all participants: " + ex.Message, ex);
            }
        }

        public Participant Save(Participant participant)
        {
            log.Info("Saving Participant: " + participant.Name);

            try
            {
                IDbConnection connection = DBUtils.getConnection(properties);
                
                _validator.Validate(participant);

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "INSERT INTO participants (name, age) VALUES (@name, @age)";
                    var nameParam = command.CreateParameter();
                    nameParam.ParameterName = "@name";
                    nameParam.Value = participant.Name;
                    command.Parameters.Add(nameParam);

                    var ageParam = command.CreateParameter();
                    ageParam.ParameterName = "@age";
                    ageParam.Value = participant.Age;
                    command.Parameters.Add(ageParam);

                    command.ExecuteNonQuery();

                    // After saving, get the auto-generated ID for the participant
                    command.CommandText = "SELECT last_insert_rowid()";
                    participant.Id = Convert.ToInt32(command.ExecuteScalar());

                    log.InfoFormat("Participant saved successfully with ID {0}: {1}, {2} years", 
                        participant.Id, participant.Name, participant.Age);
                    return participant;
                }
            }
            catch (ValidationException)
            {
                // Just rethrow validation exceptions
                throw;
            }
            catch (Exception ex)
            {
                log.Error("Error saving participant: " + ex.Message, ex);
                throw new RepositoryException("Error saving participant '" + participant.Name + "': " + ex.Message, ex);
            }
        }

        public Participant Update(Participant participant)
        {
            log.Info("Updating Participant with ID: " + participant.Id);

            try
            {
                IDbConnection connection = DBUtils.getConnection(properties);
                
                _validator.Validate(participant);

                Participant? participantFound = FindOne(participant.Id);
                if (participantFound == null)
                {
                    log.InfoFormat("Participant is not found with ID: " + participant.Id);
                    throw new RepositoryException("Participant not found with ID: " + participant.Id);
                }

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "UPDATE participants SET name = @name, age = @age WHERE id = @id";
                    var nameParam = command.CreateParameter();
                    nameParam.ParameterName = "@name";
                    nameParam.Value = participant.Name;
                    command.Parameters.Add(nameParam);

                    var ageParam = command.CreateParameter();
                    ageParam.ParameterName = "@age";
                    ageParam.Value = participant.Age;
                    command.Parameters.Add(ageParam);

                    var idParam = command.CreateParameter();
                    idParam.ParameterName = "@id";
                    idParam.Value = participant.Id;
                    command.Parameters.Add(idParam);

                    int rowsAffected = command.ExecuteNonQuery();
                    if (rowsAffected == 0)
                    {
                        throw new RepositoryException("Participant not found for update with ID: " + participant.Id);
                    }

                    log.InfoFormat("Participant updated successfully with ID {0}: {1}, {2} years", 
                        participant.Id, participant.Name, participant.Age);
                    return participant;
                }
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
                log.Error("Error updating participant: " + ex.Message, ex);
                throw new RepositoryException("Error updating participant with ID " + participant.Id + ": " + ex.Message, ex);
            }
        }

        public Participant Delete(int id)
        {
            log.Info("Deleting Participant with ID: " + id);

            try
            {
                // Find the participant before deleting
                Participant participant = FindOne(id); 
                if (participant == null)
                {
                    throw new RepositoryException("Participant not found for deletion with ID: " + id);
                }

                IDbConnection connection = DBUtils.getConnection(properties);

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "DELETE FROM participants WHERE id = @id";
                    var parameter = command.CreateParameter();
                    parameter.ParameterName = "@id";
                    parameter.Value = id;
                    command.Parameters.Add(parameter);

                    int rowsAffected = command.ExecuteNonQuery();
                    if (rowsAffected == 0)
                    {
                        throw new RepositoryException("Participant not found for deletion with ID: " + id);
                    }

                    log.InfoFormat("Participant deleted successfully with ID {0}: {1}, {2} years", 
                        participant.Id, participant.Name, participant.Age);
                }

                return participant; // Return the deleted participant
            }
            catch (RepositoryException)
            {
                // Just rethrow repository exceptions
                throw;
            }
            catch (Exception ex)
            {
                log.Error("Error deleting participant: " + ex.Message, ex);
                throw new RepositoryException("Error deleting participant with ID " + id + ": " + ex.Message, ex);
            }
        }
    }
}