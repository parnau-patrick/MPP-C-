using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ConcursClient.Grpc;
using ConcursModel.domain.observer;
using ConcursNetworking.dto;
using Grpc.Core;
using Grpc.Net.Client;
using log4net;

// Define aliases to avoid type conflicts
using DomainUser = ConcursModel.domain.User;
using DomainEvent = ConcursModel.domain.Event;
using DomainParticipant = ConcursModel.domain.Participant;

namespace ConcursClient.service
{
    public class ConcursServicesGrpcProxy : IObservable
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(ConcursServicesGrpcProxy));
        
        private readonly ConcursService.ConcursServiceClient client;
        private readonly GrpcChannel channel;
        private readonly List<IObserver> observers = new List<IObserver>();
        private CancellationTokenSource? cancellationTokenSource;
        
        public ConcursServicesGrpcProxy(string host, int port)
        {
            log.Info($"Creating gRPC proxy to {host}:{port}");
            
            // Configure gRPC client to use HTTP/2 cleartext (for development)
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            
            var address = $"http://{host}:{port}";
            
            // Configure gRPC client
            var channelOptions = new GrpcChannelOptions
            {
                MaxReceiveMessageSize = 16 * 1024 * 1024, // 16 MB
                MaxSendMessageSize = 16 * 1024 * 1024     // 16 MB
            };
            
            channel = GrpcChannel.ForAddress(address, channelOptions);
            client = new ConcursService.ConcursServiceClient(channel);
        }
        
        public void InitializeConnection()
        {
            log.Info("Initializing gRPC connection");
            
            // Start listening for notifications
            cancellationTokenSource = new CancellationTokenSource();
            StartNotificationListener();
        }
        
        public void CloseConnection()
        {
            log.Info("Closing gRPC connection");
            
            // Cancel notification listener
            cancellationTokenSource?.Cancel();
            cancellationTokenSource = null;
            
            // Close channel
            channel?.ShutdownAsync().Wait();
        }
        
        private async void StartNotificationListener()
        {
            try
            {
                log.Info("Starting notification listener");
                
                // Subscribe to notifications from server
                var request = new EmptyRequest();
                using (var call = client.Subscribe(request))
                {
                    // Read notifications asynchronously
                    await foreach (var notification in call.ResponseStream.ReadAllAsync(cancellationTokenSource.Token))
                    {
                        log.Info($"Received notification: {notification.Type}");
                        
                        // Notify observers
                        NotifyObservers();
                    }
                }
            }
            catch (RpcException ex) when (ex.StatusCode == StatusCode.Cancelled)
            {
                log.Info("Notification listener cancelled");
            }
            catch (Exception ex)
            {
                log.Error("Error in notification listener", ex);
            }
        }
        
        // Observer pattern implementation
        public void AddObserver(IObserver observer)
        {
            observers.Add(observer);
        }
        
        public void RemoveObserver(IObserver observer)
        {
            observers.Remove(observer);
        }
        
        public void NotifyObservers()
        {
            foreach (var observer in observers)
            {
                try
                {
                    observer.Update();
                }
                catch (Exception ex)
                {
                    log.Error($"Error notifying observer: {ex.Message}", ex);
                }
            }
        }
        
        // Helper methods for type conversion
        private DomainUser ConvertToDomainUser(User grpcUser)
        {
            return new DomainUser(
                grpcUser.Username,
                grpcUser.Password,
                grpcUser.OfficeName)
            {
                Id = grpcUser.Id
            };
        }
        
        private DomainEvent ConvertToDomainEvent(Event grpcEvent)
        {
            return new DomainEvent(
                grpcEvent.Distance,
                grpcEvent.Style)
            {
                Id = grpcEvent.Id
            };
        }
        
        private DomainParticipant ConvertToDomainParticipant(Participant grpcParticipant)
        {
            return new DomainParticipant(
                grpcParticipant.Name,
                grpcParticipant.Age)
            {
                Id = grpcParticipant.Id
            };
        }
        
        // Authentication methods
        public DomainUser Login(string username, string password)
        {
            log.Info($"Login attempt for user: {username}");
            
            try
            {
                // Create login request
                var request = new LoginRequest
                {
                    Username = username,
                    Password = password
                };
                
                // Call login method
                var response = client.Login(request);
                
                if (response.Success)
                {
                    // Convert gRPC User to domain User
                    var user = ConvertToDomainUser(response.User);
                    
                    log.Info($"Login successful for user: {username}");
                    return user;
                }
                
                log.Warn($"Login failed for user: {username}");
                return null;
            }
            catch (Exception ex)
            {
                log.Error("Error during login", ex);
                throw new Exception($"Login error: {ex.Message}", ex);
            }
        }
        
        public DomainUser RegisterUser(string username, string password, string officeName)
        {
            log.Info($"Registering new user: {username}");
            
            try
            {
                // Create register user request
                var request = new RegisterUserRequest
                {
                    Username = username,
                    Password = password,
                    OfficeName = officeName
                };
                
                // Call register user method
                var response = client.RegisterUser(request);
                
                // Convert gRPC User to domain User
                var user = ConvertToDomainUser(response.User);
                
                log.Info($"User registered successfully: {username}");
                return user;
            }
            catch (Exception ex)
            {
                log.Error("Error registering user", ex);
                throw new Exception($"Registration error: {ex.Message}", ex);
            }
        }
        
        // Event methods
        public List<DomainEvent> GetAllEvents()
        {
            log.Info("Getting all events");
            
            try
            {
                // Create empty request
                var request = new EmptyRequest();
                
                // Call get all events method
                var response = client.GetAllEvents(request);
                
                // Convert gRPC Events to domain Events
                var events = new List<DomainEvent>();
                foreach (var grpcEvent in response.Events)
                {
                    var evt = ConvertToDomainEvent(grpcEvent);
                    events.Add(evt);
                }
                
                log.Info($"Got {events.Count} events");
                return events;
            }
            catch (Exception ex)
            {
                log.Error("Error getting events", ex);
                throw new Exception($"Error getting events: {ex.Message}", ex);
            }
        }
        
        public List<EventDTO> GetAllEventsWithParticipantCounts()
        {
            log.Info("Getting all events with participant counts");
            
            try
            {
                // Create empty request
                var request = new EmptyRequest();
                
                // Call get events with counts method
                var response = client.GetEventsWithParticipantCounts(request);
                
                // Convert gRPC EventWithCount to domain EventDTO
                var eventDtos = new List<EventDTO>();
                foreach (var grpcEventWithCount in response.Events)
                {
                    var grpcEvent = grpcEventWithCount.Event;
                    var eventDto = new EventDTO
                    {
                        Id = grpcEvent.Id,
                        Distance = grpcEvent.Distance,
                        Style = grpcEvent.Style,
                        ParticipantsCount = grpcEventWithCount.ParticipantsCount
                    };
                    eventDtos.Add(eventDto);
                }
                
                log.Info($"Got {eventDtos.Count} events with participant counts");
                return eventDtos;
            }
            catch (Exception ex)
            {
                log.Error("Error getting events with counts", ex);
                throw new Exception($"Error getting events with counts: {ex.Message}", ex);
            }
        }
        
        // Participant methods
        public List<ParticipantEventDTO> GetParticipantsByEvent(int eventId)
        {
            log.Info($"Getting participants for event: {eventId}");
            
            try
            {
                // Create event ID request
                var request = new EventIdRequest { Id = eventId };
                
                // Call get participants by event method
                var response = client.GetParticipantsByEvent(request);
                
                // Convert gRPC ParticipantWithEventCount to domain ParticipantEventDTO
                var participantDtos = new List<ParticipantEventDTO>();
                foreach (var grpcParticipantWithCount in response.Participants)
                {
                    var grpcParticipant = grpcParticipantWithCount.Participant;
                    var participantDto = new ParticipantEventDTO(
                        grpcParticipant.Id,
                        grpcParticipant.Name,
                        grpcParticipant.Age,
                        grpcParticipantWithCount.EventsCount
                    );
                    participantDtos.Add(participantDto);
                }
                
                log.Info($"Got {participantDtos.Count} participants for event ID: {eventId}");
                return participantDtos;
            }
            catch (Exception ex)
            {
                log.Error("Error getting participants by event", ex);
                throw new Exception($"Error getting participants by event: {ex.Message}", ex);
            }
        }
        
        public void RegisterParticipant(string name, int age, List<int> eventIds)
        {
            log.Info($"Registering participant: {name} for {eventIds.Count} events");
            
            try
            {
                // Create register participant request
                var request = new RegisterParticipantRequest
                {
                    Name = name,
                    Age = age
                };
                
                // Add all event IDs
                request.EventIds.AddRange(eventIds);
                
                // Call register participant method
                client.RegisterParticipant(request);
                
                log.Info($"Participant {name} registered successfully");
            }
            catch (Exception ex)
            {
                log.Error("Error registering participant", ex);
                throw new Exception($"Error registering participant: {ex.Message}", ex);
            }
        }
        
        public List<DomainParticipant> GetAllParticipants()
        {
            log.Info("Getting all participants");
            
            try
            {
                // Create empty request
                var request = new EmptyRequest();
                
                // Call get all participants method
                var response = client.GetAllParticipants(request);
                
                // Convert gRPC Participants to domain Participants
                var participants = new List<DomainParticipant>();
                foreach (var grpcParticipant in response.Participants)
                {
                    var participant = ConvertToDomainParticipant(grpcParticipant);
                    participants.Add(participant);
                }
                
                log.Info($"Got {participants.Count} participants");
                return participants;
            }
            catch (Exception ex)
            {
                log.Error("Error getting all participants", ex);
                throw new Exception($"Error getting all participants: {ex.Message}", ex);
            }
        }
    }
}