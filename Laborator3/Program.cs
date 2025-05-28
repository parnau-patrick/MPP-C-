using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Laborator3.domain.validator;
using Laborator3.domain;
using Laborator3.repository;
using Laborator3.service;
using log4net;
using log4net.Config;
using System.IO;
using System.Reflection;
using Laborator3.repository.impl;
using Laborator3.repository.Interface;

namespace Laborator3
{
    static class Program
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(Program));

        
        [STAThread]
        static void Main()
        {
            // Configure log4net
            XmlConfigurator.Configure(new FileInfo("log4net.config"));
            log.Info("Starting application");

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            try
            {
                // Set up configuration
                IDictionary<string, string> props = new Dictionary<string, string>();
                props["ConnectionString"] = System.Configuration.ConfigurationManager.ConnectionStrings["swim"].ConnectionString;
                
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
                
                // Create and run main form
                MainForm mainForm = new MainForm(eventService, participantService, inscriereService, userService);
                Application.Run(mainForm);
            }
            catch (Exception ex)
            {
                log.Error("Unhandled exception in main thread", ex);
                MessageBox.Show($"An unexpected error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}