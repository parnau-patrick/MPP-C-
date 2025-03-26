using System;
using System.Collections.Generic;
using System.Data;
using Laborator3.domain;
using Laborator3.repository_utils;
using SwimmingCompetition.exception;
using log4net;

namespace Laborator3.repository
{
    public class ParticipantRepositoryImpl : IRepository<int, Participant> // Implementare corectă a interfeței IRepository<int, Participant>
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IDictionary<string, string> properties;

        public ParticipantRepositoryImpl(IDictionary<string, string> properties)
        {
            log.Info("Creating ParticipantRepository");
            this.properties = properties; // Aici folosim `this.properties` pentru a accesa variabila membru.
        }

        private Participant? GetParticipantFromResult(IDataReader dataReader)
        {
            int id = Convert.ToInt32(dataReader["id"]);
            string name = dataReader["name"].ToString();
            int age = Convert.ToInt32(dataReader["age"]);

            return new Participant(name, age) { Id = id };
        }

        public Participant? FindOne(int id)
        {
            log.Info("Finding Participant");

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
                        log.InfoFormat("Exiting FindOne Participant with value: " + participant);
                    }
                }
            }

            log.InfoFormat("Exiting FindOne Participant with value: " + participant);
            return participant;
        }

        public IEnumerable<Participant> FindAll()
        {
            log.Info("Finding All Participants");

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

            log.InfoFormat("Exiting FindAll Participants with value: " + participants);
            return participants;
        }

        public Participant Save(Participant participant)
        {
            log.Info("Saving Participant");

            IDbConnection connection = DBUtils.getConnection(properties);

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
                command.CommandText = "SELECT LAST_INSERT_ID()";
                participant.Id = Convert.ToInt32(command.ExecuteScalar());

                log.InfoFormat("Participant saved with value: " + participant);
                return participant;
            }
        }

        public Participant Update(Participant participant)
        {
            log.Info("Updating Participant");

            IDbConnection connection = DBUtils.getConnection(properties);

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
                    throw new RepositoryException("Participant not found for update.");
                }

                log.InfoFormat("Participant updated with value: " + participant);
                return participant;
            }
        }

        public Participant Delete(int id)
        {
            log.Info("Deleting Participant");

            Participant participant = FindOne(id); // Find the participant before deleting

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
                    throw new RepositoryException("Participant not found for deletion.");
                }

                log.InfoFormat("Participant deleted with value: " + participant);
            }

            return participant; // Return the deleted participant
        }
    }
}
