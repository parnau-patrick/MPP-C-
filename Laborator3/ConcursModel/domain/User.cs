using System;

namespace ConcursModel.domain
{
    [Serializable]
    public class User : Entity<int>
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string OfficeName { get; set; }

        public User()
        {
            Username = string.Empty;
            Password = string.Empty;
            OfficeName = string.Empty;
        }

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