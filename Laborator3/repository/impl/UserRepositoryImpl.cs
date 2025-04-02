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
    public class UserRepositoryImpl : IUserRepository
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IDictionary<string, string> properties;
        private readonly IValidator<User> validator;

        public UserRepositoryImpl(IDictionary<string, string> properties, IValidator<User> validator)
        {
            log.Info("Creating UserRepositoryImpl");
            this.properties = properties;
            this.validator = validator;
        }

        private User GetUserFromResult(IDataReader dataReader)
        {
            try
            {
                int id = Convert.ToInt32(dataReader["id"]);
                string username = dataReader["username"].ToString();
                string password = dataReader["password"].ToString();
                string officeName = dataReader["office_name"].ToString();

                User user = new User(username, password, officeName)
                {
                    Id = id
                };
                return user;
            }
            catch (Exception ex)
            {
                log.Error("Error parsing user from database: " + ex.Message, ex);
                throw new RepositoryException("Error parsing user data: " + ex.Message, ex);
            }
        }

        public User FindOne(int id)
        {
            log.Info($"UserRepository - FindOne with id: {id}");
            
            try
            {
                IDbConnection connection = DBUtils.getConnection(properties);

                if (id == 0)
                {
                    throw new RepositoryException("User Id is null or invalid");
                }

                User user = null;
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT * FROM users WHERE id = @id";
                    IDbDataParameter parameter = command.CreateParameter();
                    parameter.ParameterName = "@id";
                    parameter.Value = id;

                    command.Parameters.Add(parameter);

                    using (var dataReader = command.ExecuteReader())
                    {
                        if (dataReader.Read())
                        {
                            user = GetUserFromResult(dataReader);
                            log.InfoFormat("Found user with ID {0}: {1} ({2})", user.Id, user.Username, user.OfficeName);
                        }
                    }
                }

                if (user == null)
                {
                    log.Info($"No user found with ID: {id}");
                }
                
                return user;
            }
            catch (RepositoryException)
            {
                // Just rethrow repository exceptions
                throw;
            }
            catch (Exception ex)
            {
                log.Error("Error finding user: " + ex.Message, ex);
                throw new RepositoryException("Error finding user with ID " + id + ": " + ex.Message, ex);
            }
        }

        public User FindByUsername(string username)
        {
            log.Info($"UserRepository - FindByUsername with username: {username}");
            
            try
            {
                IDbConnection connection = DBUtils.getConnection(properties);

                if (string.IsNullOrEmpty(username))
                {
                    throw new RepositoryException("Username is null or empty");
                }

                User user = null;
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT * FROM users WHERE username = @username";
                    IDbDataParameter parameter = command.CreateParameter();
                    parameter.ParameterName = "@username";
                    parameter.Value = username;

                    command.Parameters.Add(parameter);

                    using (var dataReader = command.ExecuteReader())
                    {
                        if (dataReader.Read())
                        {
                            user = GetUserFromResult(dataReader);
                            log.InfoFormat("Found user with username {0}: ID {1}, office {2}", 
                                user.Username, user.Id, user.OfficeName);
                        }
                    }
                }

                if (user == null)
                {
                    log.Info($"No user found with username: {username}");
                }
                
                return user;
            }
            catch (RepositoryException)
            {
                // Just rethrow repository exceptions
                throw;
            }
            catch (Exception ex)
            {
                log.Error("Error finding user by username: " + ex.Message, ex);
                throw new RepositoryException("Error finding user with username '" + username + "': " + ex.Message, ex);
            }
        }

        public bool ValidateCredentials(string username, string password)
        {
            log.Info($"UserRepository - ValidateCredentials for username: {username}");
            
            try
            {
                User user = FindByUsername(username);
                if (user == null)
                {
                    log.Info($"Validation failed: User not found with username: {username}");
                    return false;
                }

                // In a real application, you would use proper password hashing
                // For simplicity, we're doing a direct comparison
                bool isValid = user.Password == password;
                
                if (isValid)
                {
                    log.Info($"Credentials validated successfully for user: {username}");
                }
                else
                {
                    log.Info($"Validation failed: Incorrect password for user: {username}");
                }
                
                return isValid;
            }
            catch (Exception ex)
            {
                log.Error("Error validating credentials: " + ex.Message, ex);
                // We return false rather than throw an exception for security reasons
                // (not revealing if the user exists or not)
                return false;
            }
        }

        public IEnumerable<User> FindAll()
        {
            log.Info("UserRepository - FindAll");
            
            try
            {
                IDbConnection connection = DBUtils.getConnection(properties);
                IList<User> users = new List<User>();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT * FROM users";

                    using (var dataReader = command.ExecuteReader())
                    {
                        while (dataReader.Read())
                        {
                            User user = GetUserFromResult(dataReader);
                            users.Add(user);
                        }
                    }
                }

                log.InfoFormat("Found {0} users", users.Count);
                return users;
            }
            catch (Exception ex)
            {
                log.Error("Error finding all users: " + ex.Message, ex);
                throw new RepositoryException("Error retrieving all users: " + ex.Message, ex);
            }
        }

        public User Save(User user)
        {
            log.Info($"UserRepository - Save user: {user.Username}");
            
            try
            {
                IDbConnection connection = DBUtils.getConnection(properties);

                validator.Validate(user);

                // Check if user with same username already exists
                User existingUser = FindByUsername(user.Username);
                if (existingUser != null)
                {
                    log.Info($"User with username {user.Username} already exists");
                    throw new RepositoryException($"User with username {user.Username} already exists");
                }

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "INSERT INTO users (username, password, office_name) VALUES (@username, @password, @officeName)";
                    
                    var usernameParam = command.CreateParameter();
                    usernameParam.ParameterName = "@username";
                    usernameParam.Value = user.Username;
                    command.Parameters.Add(usernameParam);
                    
                    var passwordParam = command.CreateParameter();
                    passwordParam.ParameterName = "@password";
                    passwordParam.Value = user.Password;
                    command.Parameters.Add(passwordParam);
                    
                    var officeNameParam = command.CreateParameter();
                    officeNameParam.ParameterName = "@officeName";
                    officeNameParam.Value = user.OfficeName;
                    command.Parameters.Add(officeNameParam);

                    command.ExecuteNonQuery();

                    // Get last inserted ID
                    command.CommandText = "SELECT last_insert_rowid()";
                    user.Id = Convert.ToInt32(command.ExecuteScalar());

                    log.InfoFormat("User saved successfully with ID {0}: {1} ({2})", 
                        user.Id, user.Username, user.OfficeName);
                }

                return user;
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
                log.Error("Error saving user: " + ex.Message, ex);
                throw new RepositoryException("Error saving user '" + user.Username + "': " + ex.Message, ex);
            }
        }

        public User Update(User user)
        {
            log.Info($"UserRepository - Update user with ID: {user.Id}");
            
            try
            {
                IDbConnection connection = DBUtils.getConnection(properties);

                validator.Validate(user);

                User existingUser = FindOne(user.Id);
                if (existingUser == null)
                {
                    log.Info($"User not found for update with ID: {user.Id}");
                    throw new RepositoryException($"User not found for update with ID: {user.Id}");
                }

                // Check if we're trying to update to a username that already exists for another user
                if (existingUser.Username != user.Username)
                {
                    User userWithSameUsername = FindByUsername(user.Username);
                    if (userWithSameUsername != null && userWithSameUsername.Id != user.Id)
                    {
                        log.Info($"Cannot update user: username {user.Username} already exists for another user");
                        throw new RepositoryException($"Username {user.Username} already exists for another user");
                    }
                }

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "UPDATE users SET username = @username, password = @password, office_name = @officeName WHERE id = @id";
                    
                    var usernameParam = command.CreateParameter();
                    usernameParam.ParameterName = "@username";
                    usernameParam.Value = user.Username;
                    command.Parameters.Add(usernameParam);
                    
                    var passwordParam = command.CreateParameter();
                    passwordParam.ParameterName = "@password";
                    passwordParam.Value = user.Password;
                    command.Parameters.Add(passwordParam);
                    
                    var officeNameParam = command.CreateParameter();
                    officeNameParam.ParameterName = "@officeName";
                    officeNameParam.Value = user.OfficeName;
                    command.Parameters.Add(officeNameParam);
                    
                    var idParam = command.CreateParameter();
                    idParam.ParameterName = "@id";
                    idParam.Value = user.Id;
                    command.Parameters.Add(idParam);

                    int rowsAffected = command.ExecuteNonQuery();
                    if (rowsAffected == 0)
                    {
                        throw new RepositoryException($"User not found for update with ID: {user.Id}");
                    }

                    log.InfoFormat("User updated successfully with ID {0}: {1} ({2})", 
                        user.Id, user.Username, user.OfficeName);
                }

                return user;
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
                log.Error("Error updating user: " + ex.Message, ex);
                throw new RepositoryException("Error updating user with ID " + user.Id + ": " + ex.Message, ex);
            }
        }

        public User Delete(int id)
        {
            log.Info($"UserRepository - Delete user with id: {id}");
            
            try
            {
                User user = FindOne(id);
                if (user == null)
                {
                    log.Info($"User not found for deletion with ID: {id}");
                    throw new RepositoryException($"User not found for deletion with ID: {id}");
                }

                IDbConnection connection = DBUtils.getConnection(properties);

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "DELETE FROM users WHERE id = @id";
                    var parameter = command.CreateParameter();
                    parameter.ParameterName = "@id";
                    parameter.Value = id;
                    command.Parameters.Add(parameter);

                    int rowsAffected = command.ExecuteNonQuery();
                    if (rowsAffected == 0)
                    {
                        throw new RepositoryException($"User not found for deletion with ID: {id}");
                    }

                    log.InfoFormat("User deleted successfully with ID {0}: {1} ({2})", 
                        user.Id, user.Username, user.OfficeName);
                }

                return user;
            }
            catch (RepositoryException)
            {
                // Just rethrow repository exceptions
                throw;
            }
            catch (Exception ex)
            {
                log.Error("Error deleting user: " + ex.Message, ex);
                throw new RepositoryException("Error deleting user with ID " + id + ": " + ex.Message, ex);
            }
        }
    }
}