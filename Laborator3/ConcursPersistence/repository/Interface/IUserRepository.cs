using System;
using System.Collections.Generic;
using ConcursModel.domain;

namespace ConcursPersistence.repository.Interface
{
    public interface IUserRepository : IRepository<int, User>
    {
        User FindByUsername(string username);
        bool ValidateCredentials(string username, string password);
    }
}