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
        
        private void InitializeConnection()
        {
            try
            {
                connection = new TcpClient(host, port);
                stream = connection.GetStream();
                connected = true;
                
                // Start a background thread to listen for server notifications
                Thread responseReaderThread = new Thread(ResponseReader);
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
                    log.Debug($"Sending request: {request.Type}, Data type: {request.Data?.GetType().Name ?? "null"}");
                    
                    // Send request to server
                    JsonSerializerUtils.SendToStream(stream, request);
                    
                    // Receive response from server
                    Response response = JsonSerializerUtils.ReadFromStream<Response>(stream);
                    
                    log.Debug($"Received response: {response.Type}, Data type: {response.Data?.GetType().Name ?? "null"}");
                    
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
                if (response.Data is UserDTO)
                {
                    user = DTOUtils.GetFromDTO(response.Data as UserDTO);
                }
                else if (response.Data is JObject)
                {
                    var userDtoObj = (response.Data as JObject).ToObject<UserDTO>();
                    user = DTOUtils.GetFromDTO(userDtoObj);
                }
                else
                {
                    var json = JsonConvert.SerializeObject(response.Data, JsonSerializerUtils.SerializerSettings);
                    var responseUserDto = JsonConvert.DeserializeObject<UserDTO>(json, JsonSerializerUtils.SerializerSettings);
                    user = DTOUtils.GetFromDTO(responseUserDto);
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
                if (response.Data is UserDTO)
                {
                    user = DTOUtils.GetFromDTO(response.Data as UserDTO);
                }
                else if (response.Data is JObject)
                {
                    var userDtoObj = (response.Data as JObject).ToObject<UserDTO>();
                    user = DTOUtils.GetFromDTO(userDtoObj);
                }
                else
                {
                    var json = JsonConvert.SerializeObject(response.Data, JsonSerializerUtils.SerializerSettings);
                    var responseUserDto = JsonConvert.DeserializeObject<UserDTO>(json, JsonSerializerUtils.SerializerSettings);
                    user = DTOUtils.GetFromDTO(responseUserDto);
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
                var eventDtoList = JsonConvert.DeserializeObject<List<EventDTO>>(
                    JsonConvert.SerializeObject(response.Data, JsonSerializerUtils.SerializerSettings),
                    JsonSerializerUtils.SerializerSettings);
                
                if (eventDtoList != null)
                {
                    foreach (var eventDto in eventDtoList)
                    {
                        events.Add(DTOUtils.GetFromDTO(eventDto));
                    }
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
                var result = JsonConvert.DeserializeObject<List<EventDTO>>(
                    JsonConvert.SerializeObject(response.Data, JsonSerializerUtils.SerializerSettings),
                    JsonSerializerUtils.SerializerSettings);
                
                if (result != null)
                {
                    eventDtos = result;
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
                var result = JsonConvert.DeserializeObject<List<ParticipantEventDTO>>(
                    JsonConvert.SerializeObject(response.Data, JsonSerializerUtils.SerializerSettings),
                    JsonSerializerUtils.SerializerSettings);
                
                if (result != null)
                {
                    participantDtos = result;
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
        
        public List<ParticipantDTO> GetAllParticipants()
        {
            log.Info("Getting all participants");
            
            Request request = new Request(RequestType.GET_ALL_PARTICIPANTS, null);
            Response response = SendAndReceive(request);
            
            // Extract participant DTOs from response
            List<ParticipantDTO> participantDtos = new List<ParticipantDTO>();
            
            try
            {
                var result = JsonConvert.DeserializeObject<List<ParticipantDTO>>(
                    JsonConvert.SerializeObject(response.Data, JsonSerializerUtils.SerializerSettings),
                    JsonSerializerUtils.SerializerSettings);
                
                if (result != null)
                {
                    participantDtos = result;
                }
                
                log.Info($"Got {participantDtos.Count} participants");
            }
            catch (Exception ex)
            {
                log.Error("Error processing get all participants response", ex);
                throw new Exception("Error processing get all participants response: " + ex.Message, ex);
            }
            
            return participantDtos;
        }
    }
}