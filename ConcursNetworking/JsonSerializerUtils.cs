using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using log4net;

namespace ConcursNetworking.utils
{
    public class JsonSerializerUtils
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(JsonSerializerUtils));
        
        // Simple configuration without type information to match Java server expectations
        public static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            TypeNameHandling = TypeNameHandling.None,  // No type information
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Include,
            DateFormatHandling = DateFormatHandling.IsoDateFormat
        };

        /// <summary>
        /// Send an object to the network stream with length prefixing
        /// </summary>
        public static void SendToStream(NetworkStream stream, object obj)
        {
            try
            {
                // Simple serialization without type information
                string jsonString = JsonConvert.SerializeObject(obj, SerializerSettings);
                log.Debug($"Sending JSON: {jsonString}");
                
                byte[] jsonBytes = Encoding.UTF8.GetBytes(jsonString);
                
                // Use DataOutputStream-like behavior for big-endian compatibility with Java
                byte[] lengthBytes = BitConverter.GetBytes(jsonBytes.Length);
                if (BitConverter.IsLittleEndian)
                {
                    Array.Reverse(lengthBytes);  // Convert to big-endian (network byte order)
                }
                
                // Send length prefix
                stream.Write(lengthBytes, 0, 4);
                
                // Send JSON data
                stream.Write(jsonBytes, 0, jsonBytes.Length);
                stream.Flush();
                
                log.Debug($"Sent {jsonBytes.Length} bytes of data");
            }
            catch (Exception ex)
            {
                log.Error($"Error sending object to stream: {ex.Message}", ex);
                throw;
            }
        }

        /// <summary>
        /// Read an object from the network stream with length prefixing
        /// Uses network byte order (big-endian) for the length prefix
        /// </summary>
        public static T ReadFromStream<T>(NetworkStream stream)
        {
            try
            {
                // Read length prefix (4 bytes)
                byte[] lengthBytes = new byte[4];
                int bytesRead = stream.Read(lengthBytes, 0, 4);
                if (bytesRead < 4)
                {
                    log.Error($"Failed to read message length, read only {bytesRead} bytes");
                    throw new IOException("Failed to read message length");
                }
                
                // Convert from big-endian (Java/network byte order) to host byte order
                if (BitConverter.IsLittleEndian)
                {
                    Array.Reverse(lengthBytes);
                }
                
                int jsonLength = BitConverter.ToInt32(lengthBytes, 0);
                log.Debug($"Message length: {jsonLength} bytes");
                
                // Validate length to prevent malicious large allocations
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
                
                // Convert bytes to string
                string jsonString = Encoding.UTF8.GetString(jsonBytes);
                log.Debug($"Received JSON: {jsonString}");
                
                // Deserialize the JSON string to the requested type
                T result;
                try
                {
                    result = JsonConvert.DeserializeObject<T>(jsonString, SerializerSettings);
                }
                catch (JsonException ex)
                {
                    log.Error($"JSON deserialization error: {ex.Message}", ex);
                    throw new IOException($"Error deserializing JSON: {ex.Message}", ex);
                }
                
                if (result == null)
                {
                    log.Error("Deserialization returned null");
                    throw new InvalidOperationException("Deserialization returned null");
                }
                
                return result;
            }
            catch (IOException ex)
            {
                log.Error($"IO error reading from stream: {ex.Message}", ex);
                throw;
            }
            catch (Exception ex)
            {
                log.Error($"Error reading object from stream: {ex.Message}", ex);
                throw;
            }
        }
        
        /// <summary>
        /// Test if a server is reachable
        /// </summary>
        public static bool TestConnection(string host, int port, int timeoutMs = 2000)
        {
            try
            {
                using (TcpClient client = new TcpClient())
                {
                    var result = client.BeginConnect(host, port, null, null);
                    bool success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromMilliseconds(timeoutMs));
                    
                    if (!success)
                    {
                        log.Error($"Connection timeout when connecting to {host}:{port}");
                        return false;
                    }
                    
                    client.EndConnect(result);
                    log.Info($"Successfully connected to {host}:{port}");
                    return true;
                }
            }
            catch (Exception ex)
            {
                log.Error($"Error testing connection to {host}:{port}: {ex.Message}");
                return false;
            }
        }
    }
}