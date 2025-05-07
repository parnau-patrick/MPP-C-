using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using log4net;

namespace ConcursNetworking.utils
{
    public class JsonSerializerUtils
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(JsonSerializerUtils));
        
        public static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            TypeNameHandling = TypeNameHandling.All,
            Formatting = Formatting.Indented
        };

        public static void SendToStream(NetworkStream stream, object obj)
        {
            try
            {
                // Serialize the object to JSON
                string jsonString = JsonConvert.SerializeObject(obj, SerializerSettings);
                log.Debug($"Sending JSON: {jsonString}");
                
                byte[] jsonBytes = Encoding.UTF8.GetBytes(jsonString);
                
                // First send the length of the JSON data
                byte[] lengthBytes = BitConverter.GetBytes(jsonBytes.Length);
                stream.Write(lengthBytes, 0, 4);
                
                // Then send the JSON data
                stream.Write(jsonBytes, 0, jsonBytes.Length);
                stream.Flush();
                
                log.Debug($"Sent {jsonBytes.Length} bytes of data");
            }
            catch (Exception ex)
            {
                log.Error("Error sending object to stream", ex);
                throw;
            }
        }

        public static T ReadFromStream<T>(NetworkStream stream)
        {
            try
            {
                // Read the length of the JSON data (first 4 bytes)
                byte[] lengthBytes = new byte[4];
                int bytesRead = stream.Read(lengthBytes, 0, 4);
                if (bytesRead < 4)
                {
                    log.Error($"Failed to read message length, read only {bytesRead} bytes");
                    throw new IOException("Failed to read message length");
                }
                
                int jsonLength = BitConverter.ToInt32(lengthBytes, 0);
                log.Debug($"Message length: {jsonLength} bytes");
                
                if (jsonLength <= 0 || jsonLength > 1024 * 1024) // 1MB limit
                {
                    log.Error($"Invalid message length: {jsonLength}");
                    throw new IOException($"Invalid message length: {jsonLength}");
                }
                
                // Read the JSON data
                byte[] jsonBytes = new byte[jsonLength];
                int totalBytesRead = 0;
                
                while (totalBytesRead < jsonLength)
                {
                    int bytesRemaining = jsonLength - totalBytesRead;
                    int bytesReadThisTime = stream.Read(jsonBytes, totalBytesRead, bytesRemaining);
                    
                    if (bytesReadThisTime == 0)
                    {
                        log.Error("Connection closed before all data was read");
                        throw new IOException("Connection closed before all data was read");
                    }
                    
                    totalBytesRead += bytesReadThisTime;
                }
                
                // Deserialize the JSON data
                string jsonString = Encoding.UTF8.GetString(jsonBytes);
                log.Debug($"Received JSON: {jsonString}");
                
                T result = JsonConvert.DeserializeObject<T>(jsonString, SerializerSettings);
                if (result == null)
                {
                    log.Error("Deserialization returned null");
                    throw new InvalidOperationException("Deserialization returned null");
                }
                
                return result;
            }
            catch (Exception ex)
            {
                log.Error("Error reading object from stream", ex);
                throw;
            }
        }
    }
}