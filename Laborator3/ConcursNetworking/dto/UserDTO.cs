using System;
using ConcursModel.domain;
using Newtonsoft.Json;

namespace ConcursNetworking.dto
{
    [Serializable]
    public class UserDTO
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        
        [JsonProperty("username")]
        public string Username { get; set; }
        
        [JsonProperty("password")]
        public string Password { get; set; }
        
        [JsonProperty("officeName")]
        public string OfficeName { get; set; }

        public UserDTO()
        {
            Username = string.Empty;
            Password = string.Empty;
            OfficeName = string.Empty;
        }

        public UserDTO(string username, string password)
        {
            Username = username;
            Password = password;
            OfficeName = string.Empty;
        }

        public UserDTO(string username, string password, string officeName)
        {
            Username = username;
            Password = password;
            OfficeName = officeName;
        }

        public UserDTO(User user)
        {
            Id = user.Id;
            Username = user.Username;
            Password = user.Password;
            OfficeName = user.OfficeName;
        }
    }
}