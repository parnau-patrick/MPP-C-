using System;
using System.Collections.Generic;
using System.Net.Sockets;
using ConcursModel.domain;
using ConcursServices;
using ConcursNetworking.dto;
using ConcursNetworking.jsonprotocol;
using ConcursNetworking.utils;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ConcursModel.domain.observer;

namespace ConcursServer
{
    public class ClientJsonWorker : IObserver
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(ClientJsonWorker));
        
        private readonly TcpClient client;
        private readonly NetworkStream stream;
        private readonly EventService eventService;
        private readonly ParticipantService participantService;
        private readonly InscriereService inscriereService;
        private readonly UserService userService;
        private volatile bool connected;

        public ClientJsonWorker(
            TcpClient client,
            EventService eventService,
            ParticipantService participantService,
            InscriereService inscriereService,
            UserService userService)
        {
            this.client = client;
            this.stream = client.GetStream();
            this.eventService = eventService;
            this.participantService = participantService;
            this.inscriereService = inscriereService;
            this.userService = userService;
            this.connected = true;
            
            // Register this worker as an observer of the event service
            this.eventService.AddObserver(this);
        }

        public void Run()
        {
            try
            {
                while (connected)
                {
                    // Read request from client
                    Request request = JsonSerializerUtils.ReadFromStream<Request>(stream);
                    log.Info($"Received request: {request.Type}");
                    
                    // Process request
                    Response response = HandleRequest(request);
                    
                    // Send response to client
                    JsonSerializerUtils.SendToStream(stream, response);
                }
            }
            catch (Exception ex)
            {
                log.Error("Error in client worker", ex);
            }
            finally
            {
                try
                {
                    // Unregister from event service before closing
                    eventService.RemoveObserver(this);
                    
                    // Close client connection
                    stream.Close();
                    client.Close();
                }
                catch (Exception ex)
                {
                    log.Error("Error closing client connection", ex);
                }
            }
        }
        
        // Implement the Update method from IObserver interface
        public void Update()
        {
            try 
            {
                if (connected)
                {
                    log.Info("Sending update notification to client");
                    
                    // Create a notification response
                    Response notification = new Response(ResponseType.NEW_REGISTRATION, null);
                    
                    // Send the notification to the client
                    JsonSerializerUtils.SendToStream(stream, notification);
                }
            }
            catch (Exception ex)
            {
                log.Error("Error sending notification to client", ex);
            }
        }

        private Response HandleRequest(Request request)
        {
            try
            {
                switch (request.Type)
                {
                    case RequestType.LOGIN:
                        return HandleLogin(request);
                    case RequestType.LOGOUT:
                        return HandleLogout();
                    case RequestType.GET_EVENTS:
                        return HandleGetEvents();
                    case RequestType.GET_PARTICIPANTS_BY_EVENT:
                        return HandleGetParticipantsByEvent(request);
                    case RequestType.REGISTER_PARTICIPANT:
                        return HandleRegisterParticipant(request);
                    case RequestType.GET_EVENTS_WITH_PARTICIPANT_COUNTS:
                        return HandleGetEventsWithCounts();
                    case RequestType.GET_ALL_PARTICIPANTS:
                        return HandleGetAllParticipants();
                    case RequestType.REGISTER_USER:
                        return HandleRegisterUser(request);
                    default:
                        return new Response(ResponseType.ERROR, "Unknown request type");
                }
            }
            catch (Exception ex)
            {
                log.Error("Error handling request", ex);
                return new Response(ResponseType.ERROR, ex.Message);
            }
        }

        private Response HandleLogin(Request request)
        {
            log.Info("Handling login request");
            
            try
            {
                if (request.Data == null)
                {
                    log.Error("Login request data is null");
                    return new Response(ResponseType.ERROR, "No login data provided");
                }

                log.Debug($"Login request data type: {request.Data.GetType().FullName}");
                
                UserDTO userDto;
                
                // Try different approaches to extract the UserDTO
                if (request.Data is UserDTO)
                {
                    userDto = request.Data as UserDTO;
                    log.Debug("Data is already a UserDTO");
                }
                else if (request.Data is JObject)
                {
                    var jobject = request.Data as JObject;
                    userDto = jobject.ToObject<UserDTO>();
                    log.Debug("Converted JObject to UserDTO");
                }
                else
                {
                    string json = JsonConvert.SerializeObject(request.Data, JsonSerializerUtils.SerializerSettings);
                    log.Debug($"Serialized request data to JSON: {json}");
                    userDto = JsonConvert.DeserializeObject<UserDTO>(json, JsonSerializerUtils.SerializerSettings);
                    log.Debug("Deserialized JSON to UserDTO");
                }
                
                if (userDto == null)
                {
                    log.Error("Failed to extract UserDTO from request data");
                    return new Response(ResponseType.ERROR, "Invalid user data");
                }
                
                log.Debug($"Attempting login for user: {userDto.Username}");
                
                User user = userService.Login(userDto.Username, userDto.Password);
                if (user == null)
                {
                    log.Warn($"Login failed for user: {userDto.Username}");
                    return new Response(ResponseType.ERROR, "Invalid username or password");
                }
                
                log.Info($"Login successful for user: {userDto.Username}");
                return new Response(ResponseType.OK, DTOUtils.GetDTO(user));
            }
            catch (Exception ex)
            {
                log.Error("Login error", ex);
                return new Response(ResponseType.ERROR, ex.Message);
            }
        }

        private Response HandleLogout()
        {
            log.Info("Handling logout request");
            
            connected = false;
            return new Response(ResponseType.OK, null);
        }

        private Response HandleGetEvents()
        {
            log.Info("Handling get events request");
            
            try
            {
                IEnumerable<Event> events = eventService.FindAllEvents();
                List<EventDTO> eventDtos = new List<EventDTO>();
                
                foreach (var evt in events)
                {
                    int participantCount = eventService.GetParticipantCountForEvent(evt.Id);
                    eventDtos.Add(DTOUtils.GetDTO(evt, participantCount));
                }
                
                return new Response(ResponseType.OK, eventDtos);
            }
            catch (Exception ex)
            {
                log.Error("Error getting events", ex);
                return new Response(ResponseType.ERROR, ex.Message);
            }
        }

        private Response HandleGetParticipantsByEvent(Request request)
        {
            log.Info("Handling get participants by event request");
            
            try
            {
                if (request.Data == null)
                {
                    log.Error("Event ID is null");
                    return new Response(ResponseType.ERROR, "No event ID provided");
                }
                
                int eventId;
                if (request.Data is int)
                {
                    eventId = (int)request.Data;
                }
                else
                {
                    eventId = Convert.ToInt32(request.Data);
                }
                
                IEnumerable<Participant> participants = inscriereService.FindAllParticipantsByEvent(eventId);
                List<ParticipantEventDTO> participantDtos = new List<ParticipantEventDTO>();
                
                foreach (var participant in participants)
                {
                    int eventsCount = inscriereService.CountEventsForParticipant(participant.Id);
                    participantDtos.Add(new ParticipantEventDTO(participant, eventsCount));
                }
                
                return new Response(ResponseType.OK, participantDtos);
            }
            catch (Exception ex)
            {
                log.Error("Error getting participants by event", ex);
                return new Response(ResponseType.ERROR, ex.Message);
            }
        }

        private Response HandleRegisterParticipant(Request request)
        {
            log.Info("Handling register participant request");
            
            try
            {
                if (request.Data == null)
                {
                    log.Error("Registration data is null");
                    return new Response(ResponseType.ERROR, "No registration data provided");
                }
                
                log.Debug($"Registration request data type: {request.Data.GetType().FullName}");
                
                RegistrationDTO registrationDto;
                
                // Try different approaches to extract the RegistrationDTO
                if (request.Data is RegistrationDTO)
                {
                    registrationDto = request.Data as RegistrationDTO;
                    log.Debug("Data is already a RegistrationDTO");
                }
                else if (request.Data is JObject)
                {
                    var jobject = request.Data as JObject;
                    registrationDto = jobject.ToObject<RegistrationDTO>();
                    log.Debug("Converted JObject to RegistrationDTO");
                }
                else
                {
                    string json = JsonConvert.SerializeObject(request.Data, JsonSerializerUtils.SerializerSettings);
                    log.Debug($"Serialized request data to JSON: {json}");
                    registrationDto = JsonConvert.DeserializeObject<RegistrationDTO>(json, JsonSerializerUtils.SerializerSettings);
                    log.Debug("Deserialized JSON to RegistrationDTO");
                }
                
                if (registrationDto == null)
                {
                    log.Error("Failed to extract RegistrationDTO from request data");
                    return new Response(ResponseType.ERROR, "Invalid registration data");
                }
                
                // Create and save participant
                Participant participant = new Participant(registrationDto.ParticipantName, registrationDto.ParticipantAge);
                participant = participantService.SaveParticipant(participant);
                
                // Register for all selected events
                foreach (int eventId in registrationDto.EventIds)
                {
                    inscriereService.RegisterParticipant(participant.Id, eventId);
                }
                
                log.Info($"Participant {registrationDto.ParticipantName} registered successfully for {registrationDto.EventIds.Count} events");
                return new Response(ResponseType.OK, null);
            }
            catch (Exception ex)
            {
                log.Error("Error registering participant", ex);
                return new Response(ResponseType.ERROR, ex.Message);
            }
        }

        private Response HandleGetEventsWithCounts()
        {
            log.Info("Handling get events with counts request");
            
            try
            {
                IEnumerable<Event> events = eventService.FindAllEvents();
                List<EventDTO> eventDtos = new List<EventDTO>();
                
                foreach (var evt in events)
                {
                    int participantCount = eventService.GetParticipantCountForEvent(evt.Id);
                    eventDtos.Add(DTOUtils.GetDTO(evt, participantCount));
                }
                
                return new Response(ResponseType.OK, eventDtos);
            }
            catch (Exception ex)
            {
                log.Error("Error getting events with counts", ex);
                return new Response(ResponseType.ERROR, ex.Message);
            }
        }

        private Response HandleGetAllParticipants()
        {
            log.Info("Handling get all participants request");
            
            try
            {
                IEnumerable<Participant> participants = participantService.FindAllParticipants();
                List<ParticipantDTO> participantDtos = new List<ParticipantDTO>();
                
                foreach (var participant in participants)
                {
                    participantDtos.Add(DTOUtils.GetDTO(participant));
                }
                
                return new Response(ResponseType.OK, participantDtos);
            }
            catch (Exception ex)
            {
                log.Error("Error getting all participants", ex);
                return new Response(ResponseType.ERROR, ex.Message);
            }
        }

        private Response HandleRegisterUser(Request request)
        {
            log.Info("Handling register user request");
            
            try
            {
                if (request.Data == null)
                {
                    log.Error("User registration data is null");
                    return new Response(ResponseType.ERROR, "No user data provided");
                }
                
                log.Debug($"User registration request data type: {request.Data.GetType().FullName}");
                
                UserDTO userDto;
                
                // Try different approaches to extract the UserDTO
                if (request.Data is UserDTO)
                {
                    userDto = request.Data as UserDTO;
                    log.Debug("Data is already a UserDTO");
                }
                else if (request.Data is JObject)
                {
                    var jobject = request.Data as JObject;
                    userDto = jobject.ToObject<UserDTO>();
                    log.Debug("Converted JObject to UserDTO");
                }
                else
                {
                    string json = JsonConvert.SerializeObject(request.Data, JsonSerializerUtils.SerializerSettings);
                    log.Debug($"Serialized request data to JSON: {json}");
                    userDto = JsonConvert.DeserializeObject<UserDTO>(json, JsonSerializerUtils.SerializerSettings);
                    log.Debug("Deserialized JSON to UserDTO");
                }
                
                if (userDto == null)
                {
                    log.Error("Failed to extract UserDTO from request data");
                    return new Response(ResponseType.ERROR, "Invalid user data");
                }
                
                // Create and save user
                User user = new User(userDto.Username, userDto.Password, userDto.OfficeName);
                user = userService.SaveUser(user);
                
                log.Info($"User {userDto.Username} registered successfully");
                return new Response(ResponseType.OK, DTOUtils.GetDTO(user));
            }
            catch (Exception ex)
            {
                log.Error("Error registering user", ex);
                return new Response(ResponseType.ERROR, ex.Message);
            }
        }
    }
}