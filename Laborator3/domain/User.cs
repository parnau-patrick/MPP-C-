using System;

namespace Laborator3.domain
{
    public class User : Entity<int>
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string OfficeName { get; set; }

        public User(string username, string password, string officeName)
        {
            Username = username;
            Password = password;
            OfficeName = officeName;
        }

        public override string ToString()
        {
            return $"User{{username={Username}, officeName={OfficeName}}}";
        }
    }
}