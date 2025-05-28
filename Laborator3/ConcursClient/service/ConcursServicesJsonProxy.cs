using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using ConcursModel.domain;
using ConcursModel.domain.observer;
using ConcursNetworking.dto;
using ConcursNetworking.jsonprotocol;
using ConcursNetworking.utils;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ConcursClient.service
{
    public class ConcursServicesJsonProxy
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(ConcursServicesJsonProxy));
        
        private readonly string host;
        private readonly int port;
        private TcpClient connection;
        private NetworkStream stream;
        private readonly List<IObserver> observers = new List<IObserver>();
        private volatile bool connected;
        private readonly object lockObject = new object();
        private Thread responseReaderThread;
        
        public ConcursServicesJsonProxy(string host, int port)
        {
            this.host = host;
            this.port = port;
        }
        
        public void AddObserver(IObserver observer)
        {
            observers.Add(observer);
        }
        
        public void RemoveObserver(IObserver observer)
        {
            observers.Remove(observer);
        }
        
        private void NotifyObservers()
        {
            foreach (var observer in observers)
            {
                observer.Update();
            }
        }
        
        public void InitializeConnection()
        {
            try
            {
                log.Info($"Connecting to server {host}:{port}");
                
                // Test connection first to avoid timeout delays
                if (!JsonSerializerUtils.TestConnection(host, port))
                {
                    throw new Exception($"Could not connect to server {host}:{port}");
                }
                
                connection = new TcpClient(host, port);
                stream = connection.GetStream();
                connected = true;
                
                // Start a background thread to listen for server notifications
                responseReaderThread = new Thread(ResponseReader);
                responseReaderThread.IsBackground = true;
                responseReaderThread.Start();
                
                log.Info($"Connected to server {host}:{port}");
            }
            catch (Exception ex)
            {
                log.Error("Error initializing connection", ex);
                throw new Exception("Error connecting to server: " + ex.Message, ex);
            }
        }
        
        private void ResponseReader()
        {
            try
            {
                while (connected)
                {
                    // Check if there's any data available and connection is established
                    if (connection != null && connection.Connected && connection.Available > 0)
                    {
                        try
                        {
                            // Read response from server
                            Response response = JsonSerializerUtils.ReadFromStream<Response>(stream);
                            
                            // Handle notifications
                            if (response.Type == ResponseType.NEW_EVENT ||
                                response.Type == ResponseType.NEW_PARTICIPANT ||
                                response.Type == ResponseType.NEW_REGISTRATION)
                            {
                                log.Info($"Received notification of type: {response.Type}");
                                NotifyObservers();
                            }
                        }
                        catch (Exception ex)
                        {
                            log.Error("Error reading notification", ex);
                        }
                    }
                    
                    // Sleep for a short time to avoid high CPU usage
                    Thread.Sleep(100);
                }
            }
            catch (Exception ex)
            {
                log.Error("Error in response reader", ex);
                CloseConnection();
            }
        }
        
        private Response SendAndReceive(Request request)
        {
            lock (lockObject)
            {
                try
                {
                    // Make sure we're connected
                    if (connection == null || !connection.Connected)
                    {
                        InitializeConnection();
                    }
                    
                    // Log request details
                    log.Info($"Sending request: {request.Type}, Data type: {request.Data?.GetType().Name ?? "null"}");
                    
                    // Send request to server
                    JsonSerializerUtils.SendToStream(stream, request);
                    
                    // Receive response from server
                    Response response = JsonSerializerUtils.ReadFromStream<Response>(stream);
                    
                    log.Info($"Received response: {response.Type}");
                    
                    if (response.Type == ResponseType.ERROR)
                    {
                        string errorMsg = response.Data?.ToString() ?? "Unknown error";
                        log.Error($"Server returned error: {errorMsg}");
                        throw new Exception("Server error: " + errorMsg);
                    }
                    
                    return response;
                }
                catch (Exception ex)
                {
                    log.Error("Error sending request", ex);
                    throw new Exception("Error communicating with server: " + ex.Message, ex);
                }
            }
        }
        
        public void CloseConnection()
        {
            try
            {
                connected = false;
                
                if (connection != null && connection.Connected)
                {
                    // Send logout request
                    Request request = new Request(RequestType.LOGOUT, null);
                    SendAndReceive(request);
                }
            }
            catch (Exception ex)
            {
                log.Warn("Error sending logout", ex);
            }
            
            try
            {
                if (stream != null)
                {
                    stream.Close();
                }
                if (connection != null)
                {
                    connection.Close();
                }
                
                log.Info("Connection closed");
            }
            catch (Exception ex)
            {
                log.Error("Error closing connection", ex);
            }
        }
        
        // ========== PascalCase Methods (C# convention) ==========
        
        public User Login(string username, string password)
        {
            log.Info($"Login attempt for user: {username}");
            
            UserDTO userDto = new UserDTO(username, password);
            Request request = new Request(RequestType.LOGIN, userDto);
            
            Response response = SendAndReceive(request);
            
            // Extract user from response
            User user = null;
            
            try
            {
                // Convert the response data to User
                if (response.Data is User)
                {
                    user = (User)response.Data;
                }
                else if (response.Data is JObject)
                {
                    // Convert JObject to User
                    var userObj = (JObject)response.Data;
                    user = userObj.ToObject<User>();
                }
                else
                {
                    // Serialize and deserialize to handle type conversion
                    var json = JsonConvert.SerializeObject(response.Data);
                    user = JsonConvert.DeserializeObject<User>(json);
                }
                
                if (user == null)
                {
                    log.Error("Failed to extract User from response data");
                    throw new Exception("Invalid response from server");
                }
                
                log.Info($"Login successful for user: {username}");
            }
            catch (Exception ex)
            {
                log.Error("Error processing login response", ex);
                throw new Exception("Error processing login response: " + ex.Message, ex);
            }
            
            return user;
        }
        
        public User RegisterUser(string username, string password, string officeName)
        {
            log.Info($"Registering new user: {username}");
            
            UserDTO userDto = new UserDTO(username, password, officeName);
            Request request = new Request(RequestType.REGISTER_USER, userDto);
            
            Response response = SendAndReceive(request);
            
            // Extract user from response
            User user = null;
            
            try
            {
                // Convert the response data to User
                if (response.Data is User)
                {
                    user = (User)response.Data;
                }
                else if (response.Data is JObject)
                {
                    // Convert JObject to User
                    var userObj = (JObject)response.Data;
                    user = userObj.ToObject<User>();
                }
                else
                {
                    // Serialize and deserialize to handle type conversion
                    var json = JsonConvert.SerializeObject(response.Data);
                    user = JsonConvert.DeserializeObject<User>(json);
                }
                
                if (user == null)
                {
                    log.Error("Failed to extract User from response data");
                    throw new Exception("Invalid response from server");
                }
                
                log.Info($"User registered successfully: {username}");
            }
            catch (Exception ex)
            {
                log.Error("Error processing registration response", ex);
                throw new Exception("Error processing registration response: " + ex.Message, ex);
            }
            
            return user;
        }
        
        public List<Event> GetAllEvents()
        {
            log.Info("Getting all events");
            
            Request request = new Request(RequestType.GET_EVENTS, null);
            Response response = SendAndReceive(request);
            
            // Extract events from response
            List<Event> events = new List<Event>();
            
            try
            {
                // Convert the response data to List<Event>
                if (response.Data is List<Event>)
                {
                    events = (List<Event>)response.Data;
                }
                else
                {
                    // Serialize and deserialize to handle type conversion
                    var json = JsonConvert.SerializeObject(response.Data);
                    events = JsonConvert.DeserializeObject<List<Event>>(json);
                }
                
                log.Info($"Got {events.Count} events");
            }
            catch (Exception ex)
            {
                log.Error("Error processing get events response", ex);
                throw new Exception("Error processing get events response: " + ex.Message, ex);
            }
            
            return events;
        }
        
        public List<EventDTO> GetAllEventsWithParticipantCounts()
        {
            log.Info("Getting all events with participant counts");
            
            Request request = new Request(RequestType.GET_EVENTS_WITH_PARTICIPANT_COUNTS, null);
            Response response = SendAndReceive(request);
            
            // Extract event DTOs from response
            List<EventDTO> eventDtos = new List<EventDTO>();
            
            try
            {
                // Convert the response data to List<EventDTO>
                if (response.Data is List<EventDTO>)
                {
                    eventDtos = (List<EventDTO>)response.Data;
                }
                else
                {
                    // Serialize and deserialize to handle type conversion
                    var json = JsonConvert.SerializeObject(response.Data);
                    eventDtos = JsonConvert.DeserializeObject<List<EventDTO>>(json);
                }
                
                log.Info($"Got {eventDtos.Count} events with participant counts");
            }
            catch (Exception ex)
            {
                log.Error("Error processing get events with counts response", ex);
                throw new Exception("Error processing get events with counts response: " + ex.Message, ex);
            }
            
            return eventDtos;
        }
        
        public List<ParticipantEventDTO> GetParticipantsByEvent(int eventId)
        {
            log.Info($"Getting participants for event: {eventId}");
            
            Request request = new Request(RequestType.GET_PARTICIPANTS_BY_EVENT, eventId);
            Response response = SendAndReceive(request);
            
            // Extract participant DTOs from response
            List<ParticipantEventDTO> participantDtos = new List<ParticipantEventDTO>();
            
            try
            {
                // Convert the response data to List<ParticipantEventDTO>
                if (response.Data is List<ParticipantEventDTO>)
                {
                    participantDtos = (List<ParticipantEventDTO>)response.Data;
                }
                else
                {
                    // Serialize and deserialize to handle type conversion
                    var json = JsonConvert.SerializeObject(response.Data);
                    participantDtos = JsonConvert.DeserializeObject<List<ParticipantEventDTO>>(json);
                }
                
                log.Info($"Got {participantDtos.Count} participants for event {eventId}");
            }
            catch (Exception ex)
            {
                log.Error("Error processing get participants by event response", ex);
                throw new Exception("Error processing get participants by event response: " + ex.Message, ex);
            }
            
            return participantDtos;
        }
        
        public void RegisterParticipant(string name, int age, List<int> eventIds)
        {
            log.Info($"Registering participant: {name} for {eventIds.Count} events");
            
            RegistrationDTO registrationDto = new RegistrationDTO(name, age, eventIds);
            Request request = new Request(RequestType.REGISTER_PARTICIPANT, registrationDto);
            
            SendAndReceive(request);
            
            log.Info($"Participant {name} registered successfully");
        }
        
        public List<Participant> GetAllParticipants()
        {
            log.Info("Getting all participants");
            
            Request request = new Request(RequestType.GET_ALL_PARTICIPANTS, null);
            Response response = SendAndReceive(request);
            
            // Extract participant DTOs from response
            List<Participant> participants = new List<Participant>();
            
            try
            {
                // Convert the response data to List<Participant>
                if (response.Data is List<Participant>)
                {
                    participants = (List<Participant>)response.Data;
                }
                else
                {
                    // Serialize and deserialize to handle type conversion
                    var json = JsonConvert.SerializeObject(response.Data);
                    participants = JsonConvert.DeserializeObject<List<Participant>>(json);
                }
                
                log.Info($"Got {participants.Count} participants");
            }
            catch (Exception ex)
            {
                log.Error("Error processing get all participants response", ex);
                throw new Exception("Error processing get all participants response: " + ex.Message, ex);
            }
            
            return participants;
        }
        
        // ========== camelCase Methods (Java compatibility) ==========
        
        public List<Event> getAllEvents()
        {
            return GetAllEvents();
        }
        
        public void registerParticipant(string name, int age, List<int> eventIds)
        {
            RegisterParticipant(name, age, eventIds);
        }
        
        public User login(string username, string password)
        {
            return Login(username, password);
        }
        
        public List<EventDTO> getAllEventsWithParticipantCounts()
        {
            return GetAllEventsWithParticipantCounts();
        }
        
        public List<ParticipantEventDTO> getParticipantsByEvent(int eventId)
        {
            return GetParticipantsByEvent(eventId);
        }
        
        public User registerUser(string username, string password, string officeName)
        {
            return RegisterUser(username, password, officeName);
        }
        
        public List<Participant> getAllParticipants()
        {
            return GetAllParticipants();
        }
    }
}