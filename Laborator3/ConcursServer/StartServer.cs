using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Configuration;
using System.Threading;
using log4net;
using log4net.Config;
using ConcursModel.domain.validator;
using ConcursModel.domain;
using ConcursPersistence.repository;
using ConcursPersistence.repository.Interface;
using ConcursPersistence.repository.impl;
using ConcursServices;
using Laborator3.repository;

namespace ConcursServer
{
    public class StartServer
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(StartServer));
        private static readonly int DEFAULT_PORT = 55555;
        
        static void Main(string[] args)
        {
            // Configure log4net
            XmlConfigurator.Configure(new FileInfo("log4net.config"));
            log.Info("Starting server application");

            try
            {
                // Set up configuration
                IDictionary<string, string> props = new Dictionary<string, string>();
                props["ConnectionString"] = ConfigurationManager.ConnectionStrings["swim"].ConnectionString;
                
                // Create validators
                IValidator<Event> eventValidator = new EventValidator();
                IValidator<Participant> participantValidator = new ParticipantValidator();
                IValidator<User> userValidator = new UserValidator();
                
                // Create repositories
                IEventRepository eventRepository = new EventRepositoryImpl(props, eventValidator);
                IParticipantRepository participantRepository = new ParticipantRepositoryImpl(props, participantValidator);
                IInscriereRepository inscriereRepository = new InscriereRepositoryImpl(props);
                IUserRepository userRepository = new UserRepositoryImpl(props, userValidator);
                
                // Create services
                EventService eventService = new EventService(eventRepository, inscriereRepository);
                ParticipantService participantService = new ParticipantService(participantRepository);
                UserService userService = new UserService(userRepository);
                InscriereService inscriereService = new InscriereService(inscriereRepository, eventService);
                
                // Get server port
                int serverPort = DEFAULT_PORT;
                try
                {
                    serverPort = int.Parse(ConfigurationManager.AppSettings["serverPort"]);
                }
                catch (Exception ex)
                {
                    log.Error("Error reading server port setting, using default", ex);
                }
                
                log.Info($"Starting server on port {serverPort}");
                
                // Create server socket
                TcpListener serverSocket = new TcpListener(IPAddress.Any, serverPort);
                serverSocket.Start();
                
                log.Info("Server started");
                
                while (true)
                {
                    log.Info("Waiting for clients...");
                    TcpClient client = serverSocket.AcceptTcpClient();
                    log.Info("Client connected");
                    
                    // Create and start a new client worker thread
                    ClientJsonWorker worker = new ClientJsonWorker(client, eventService, participantService, inscriereService, userService);
                    Thread clientThread = new Thread(new ThreadStart(worker.Run));
                    clientThread.Start();
                }
            }
            catch (Exception ex)
            {
                log.Error("Server error", ex);
                Console.WriteLine($"Server error: {ex.Message}");
            }
        }
    }
}