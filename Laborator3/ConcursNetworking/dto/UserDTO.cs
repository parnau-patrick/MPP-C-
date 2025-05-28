using Newtonsoft.Json;

namespace ConcursNetworking.dto
{
    [JsonObject(MemberSerialization.OptIn)]
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

        public UserDTO() { }

        public UserDTO(string username, string password)
        {
            Username = username;
            Password = password;
        }

        public UserDTO(string username, string password, string officeName)
        {
            Username = username;
            Password = password;
            OfficeName = officeName;
        }
    }
}