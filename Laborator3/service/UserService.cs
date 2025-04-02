using System;
using System.Collections.Generic;
using Laborator3.domain;
using Laborator3.repository;
using log4net;
using Laborator3.exception;
using Laborator3.repository.Interface;

namespace Laborator3.service
{
    public class UserService
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IUserRepository userRepository;

        public UserService(IUserRepository userRepository)
        {
            log.Info("Creating UserService");
            this.userRepository = userRepository;
        }

        public User Login(string username, string password)
        {
            log.Info($"UserService - Login attempt for user: {username}");
            
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                throw new ValidationException("Username and password cannot be empty");
            }
            
            User user = userRepository.FindByUsername(username);
            
            if (user == null)
            {
                log.Warn($"UserService - User not found: {username}");
                throw new ValidationException("Invalid username or password");
            }
            
            // In a real application, you would use proper password hashing
            // For simplicity in this example, we'll do a direct comparison
            if (password != user.Password)
            {
                log.Warn($"UserService - Invalid password for user: {username}");
                throw new ValidationException("Invalid username or password");
            }
            
            log.Info($"UserService - Login successful for user: {username}");
            return user;
        }
        
        public IEnumerable<User> FindAllUsers()
        {
            log.Info("UserService - FindAllUsers");
            return userRepository.FindAll();
        }

        public User FindUserById(int id)
        {
            log.Info($"UserService - FindUserById with id: {id}");
            return userRepository.FindOne(id);
        }

        public User SaveUser(User user)
        {
            log.Info($"UserService - SaveUser: {user}");
            return userRepository.Save(user);
        }

        public User UpdateUser(User user)
        {
            log.Info($"UserService - UpdateUser: {user}");
            return userRepository.Update(user);
        }

        public User DeleteUser(int id)
        {
            log.Info($"UserService - DeleteUser with id: {id}");
            return userRepository.Delete(id);
        }
    }
}