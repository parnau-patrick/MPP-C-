using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Laborator3.domain;
using log4net;

namespace Laborator3
{
    public partial class MainForm : Form
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        private User currentUser;
        private readonly service.EventService eventService;
        private readonly service.ParticipantService participantService;
        private readonly service.InscriereService inscriereService;
        private readonly service.UserService userService;
        
        private EventsForm eventsForm;
        private ParticipantSearchForm searchForm;
        private RegisterForm registerForm;

        public MainForm(
            service.EventService eventService,
            service.ParticipantService participantService,
            service.InscriereService inscriereService,
            service.UserService userService)
        {
            InitializeComponent();
            
            this.eventService = eventService;
            this.participantService = participantService;
            this.inscriereService = inscriereService;
            this.userService = userService;
            
            this.FormClosing += MainForm_FormClosing;
            ShowLoginForm();
        }

        private void ShowLoginForm()
        {
            LoginForm loginForm = new LoginForm(userService);
            loginForm.LoginSuccessful += LoginForm_LoginSuccessful;
            loginForm.ShowDialog();
            
            // If we still don't have a current user after login form closes, exit application
            if (currentUser == null)
            {
                Application.Exit();
            }
        }

        private void LoginForm_LoginSuccessful(object sender, LoginEventArgs e)
        {
            currentUser = e.User;
            lblWelcome.Text = $"Welcome, {currentUser.Username} ({currentUser.OfficeName})";
            panelDashboard.Visible = true;
        }

        private void btnEvents_Click(object sender, EventArgs e)
        {
            log.Info("Opening Events form");
            if (eventsForm == null || eventsForm.IsDisposed)
            {
                eventsForm = new EventsForm(eventService);
                eventsForm.MdiParent = this;
                eventsForm.FormClosed += (s, args) => eventsForm = null;
            }
            
            eventsForm.Show();
            eventsForm.BringToFront();
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            log.Info("Opening Search form");
            if (searchForm == null || searchForm.IsDisposed)
            {
                searchForm = new ParticipantSearchForm(eventService, inscriereService);
                searchForm.MdiParent = this;
                searchForm.FormClosed += (s, args) => searchForm = null;
            }
            
            searchForm.Show();
            searchForm.BringToFront();
        }

        private void btnRegister_Click(object sender, EventArgs e)
        {
            log.Info("Opening Registration form");
            if (registerForm == null || registerForm.IsDisposed)
            {
                registerForm = new RegisterForm(eventService, participantService, inscriereService);
                registerForm.MdiParent = this;
                registerForm.FormClosed += (s, args) => registerForm = null;
            }
            
            registerForm.Show();
            registerForm.BringToFront();
        }

        private void btnLogout_Click(object sender, EventArgs e)
        {
            log.Info("Logging out user: " + currentUser.Username);
            
            // Close all child forms
            foreach (Form childForm in this.MdiChildren)
            {
                childForm.Close();
            }
            
            // Reset current user
            currentUser = null;
            lblWelcome.Text = "";
            panelDashboard.Visible = false;
            
            // Show login form again
            ShowLoginForm();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            log.Info("Application closing");
            Application.Exit();
        }

        private void InitializeComponent()
        {
            this.panelDashboard = new System.Windows.Forms.Panel();
            this.btnLogout = new System.Windows.Forms.Button();
            this.btnRegister = new System.Windows.Forms.Button();
            this.btnSearch = new System.Windows.Forms.Button();
            this.btnEvents = new System.Windows.Forms.Button();
            this.lblWelcome = new System.Windows.Forms.Label();
            this.panelDashboard.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelDashboard
            // 
            this.panelDashboard.Controls.Add(this.btnLogout);
            this.panelDashboard.Controls.Add(this.btnRegister);
            this.panelDashboard.Controls.Add(this.btnSearch);
            this.panelDashboard.Controls.Add(this.btnEvents);
            this.panelDashboard.Controls.Add(this.lblWelcome);
            this.panelDashboard.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelDashboard.Location = new System.Drawing.Point(0, 0);
            this.panelDashboard.Name = "panelDashboard";
            this.panelDashboard.Size = new System.Drawing.Size(984, 60);
            this.panelDashboard.TabIndex = 0;
            this.panelDashboard.Visible = false;
            // 
            // btnLogout
            // 
            this.btnLogout.Location = new System.Drawing.Point(897, 12);
            this.btnLogout.Name = "btnLogout";
            this.btnLogout.Size = new System.Drawing.Size(75, 32);
            this.btnLogout.TabIndex = 4;
            this.btnLogout.Text = "Logout";
            this.btnLogout.UseVisualStyleBackColor = true;
            this.btnLogout.Click += new System.EventHandler(this.btnLogout_Click);
            // 
            // btnRegister
            // 
            this.btnRegister.Location = new System.Drawing.Point(567, 12);
            this.btnRegister.Name = "btnRegister";
            this.btnRegister.Size = new System.Drawing.Size(130, 32);
            this.btnRegister.TabIndex = 3;
            this.btnRegister.Text = "Register Participant";
            this.btnRegister.UseVisualStyleBackColor = true;
            this.btnRegister.Click += new System.EventHandler(this.btnRegister_Click);
            // 
            // btnSearch
            // 
            this.btnSearch.Location = new System.Drawing.Point(431, 12);
            this.btnSearch.Name = "btnSearch";
            this.btnSearch.Size = new System.Drawing.Size(130, 32);
            this.btnSearch.TabIndex = 2;
            this.btnSearch.Text = "Search Participants";
            this.btnSearch.UseVisualStyleBackColor = true;
            this.btnSearch.Click += new System.EventHandler(this.btnSearch_Click);
            // 
            // btnEvents
            // 
            this.btnEvents.Location = new System.Drawing.Point(295, 12);
            this.btnEvents.Name = "btnEvents";
            this.btnEvents.Size = new System.Drawing.Size(130, 32);
            this.btnEvents.TabIndex = 1;
            this.btnEvents.Text = "View Events";
            this.btnEvents.UseVisualStyleBackColor = true;
            this.btnEvents.Click += new System.EventHandler(this.btnEvents_Click);
            // 
            // lblWelcome
            // 
            this.lblWelcome.AutoSize = true;
            this.lblWelcome.Location = new System.Drawing.Point(12, 22);
            this.lblWelcome.Name = "lblWelcome";
            this.lblWelcome.Size = new System.Drawing.Size(0, 13);
            this.lblWelcome.TabIndex = 0;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(984, 561);
            this.Controls.Add(this.panelDashboard);
            this.IsMdiContainer = true;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Swimming Competition System";
            this.panelDashboard.ResumeLayout(false);
            this.panelDashboard.PerformLayout();
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.Panel panelDashboard;
        private System.Windows.Forms.Label lblWelcome;
        private System.Windows.Forms.Button btnEvents;
        private System.Windows.Forms.Button btnSearch;
        private System.Windows.Forms.Button btnRegister;
        private System.Windows.Forms.Button btnLogout;
    }
}