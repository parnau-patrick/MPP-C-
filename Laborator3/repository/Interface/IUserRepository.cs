using System;
using System.Collections.Generic;
using Laborator3.domain;

namespace Laborator3.repository.Interface
{
    public interface IUserRepository : IRepository<int, User>
    {
        User FindByUsername(string username);
        bool ValidateCredentials(string username, string password);
    }
}