using System;
using Newtonsoft.Json;

namespace ConcursModel.domain
{
    [Serializable]
    [JsonObject(MemberSerialization.OptIn)]
    public class User : Entity<int>
    {
        [JsonProperty("username")]
        public string Username { get; set; }
        
        [JsonProperty("password")]
        public string Password { get; set; }
        
        [JsonProperty("officeName")]
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