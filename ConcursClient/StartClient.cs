using System;
using System.Configuration;
using System.Windows.Forms;
using log4net;
using log4net.Config;
using ConcursClient.service;
using ConcursClient.controller;

namespace ConcursClient
{
    static class StartClient
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(StartClient));
        private static readonly string DEFAULT_SERVER_HOST = "localhost";
        private static readonly int DEFAULT_SERVER_PORT = 55555;
        
        [STAThread]
        static void Main()
        {
            // Configure log4net
            XmlConfigurator.Configure(new System.IO.FileInfo("log4net.config"));
            log.Info("Starting client application");

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            try
            {
                // Get server connection settings
                string serverHost = ConfigurationManager.AppSettings["serverHost"] ?? DEFAULT_SERVER_HOST;
                
                int serverPort = DEFAULT_SERVER_PORT;
                try
                {
                    serverPort = int.Parse(ConfigurationManager.AppSettings["serverPort"]);
                }
                catch (Exception)
                {
                    log.Warn($"Invalid port, using default: {DEFAULT_SERVER_PORT}");
                }
                
                log.Info($"Connecting to server {serverHost}:{serverPort}");
                
                // Create service proxy using gRPC communication
                ConcursServicesGrpcProxy serviceProxy = new ConcursServicesGrpcProxy(serverHost, serverPort);
                serviceProxy.InitializeConnection();
                
                // Create and show login form
                LoginForm loginForm = new LoginForm(serviceProxy);
                Application.Run(loginForm);
            }
            catch (Exception ex)
            {
                log.Error("Error starting client", ex);
                MessageBox.Show($"Error starting client: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}