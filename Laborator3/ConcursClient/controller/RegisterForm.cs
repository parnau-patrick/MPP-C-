using System;
using System.Collections.Generic;
using System.Windows.Forms;
using ConcursModel.domain;
using ConcursClient.service;
using log4net;

namespace ConcursClient.controller
{
    public partial class RegisterForm : Form
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(RegisterForm));
        private readonly ConcursServicesGrpcProxy serviceProxy;
        
        // Add this event for successful registrations
        public event EventHandler RegistrationSuccessful;

        public RegisterForm(ConcursServicesGrpcProxy serviceProxy)
        {
            InitializeComponent();
            this.serviceProxy = serviceProxy;
            
            LoadEvents();
        }

        private void LoadEvents()
        {
            log.Info("Loading events for registration form");
            try
            {
                var events = serviceProxy.GetAllEvents();
                checkedListEvents.Items.Clear();
                
                foreach (var evt in events)
                {
                    checkedListEvents.Items.Add(evt, false);
                }
            }
            catch (Exception ex)
            {
                log.Error("Error loading events", ex);
                MessageBox.Show("Error loading events: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnRegister_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Please enter participant name.", "Registration", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!int.TryParse(txtAge.Text, out int age) || age <= 0)
            {
                MessageBox.Show("Please enter a valid age.", "Registration", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (checkedListEvents.CheckedItems.Count == 0)
            {
                MessageBox.Show("Please select at least one event.", "Registration", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // Get selected event IDs
                List<int> eventIds = new List<int>();
                foreach (Event selectedEvent in checkedListEvents.CheckedItems)
                {
                    eventIds.Add(selectedEvent.Id);
                }
                
                // Register participant
                serviceProxy.RegisterParticipant(txtName.Text, age, eventIds);
                
                MessageBox.Show($"Participant {txtName.Text} has been registered successfully!", "Registration", MessageBoxButtons.OK, MessageBoxIcon.Information);
                
                // Notify that registration was successful
                RegistrationSuccessful?.Invoke(this, EventArgs.Empty);
                
                // Clear form for new registration
                txtName.Text = "";
                txtAge.Text = "";
                for (int i = 0; i < checkedListEvents.Items.Count; i++)
                {
                    checkedListEvents.SetItemChecked(i, false);
                }
            }
            catch (Exception ex)
            {
                log.Error("Error registering participant", ex);
                MessageBox.Show("Error registering participant: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        // InitializeComponent method - UI elements definition
        private void InitializeComponent()
        {
            this.lblName = new System.Windows.Forms.Label();
            this.txtName = new System.Windows.Forms.TextBox();
            this.lblAge = new System.Windows.Forms.Label();
            this.txtAge = new System.Windows.Forms.TextBox();
            this.lblEvents = new System.Windows.Forms.Label();
            this.checkedListEvents = new System.Windows.Forms.CheckedListBox();
            this.btnRegister = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lblName
            // 
            this.lblName.AutoSize = true;
            this.lblName.Location = new System.Drawing.Point(12, 25);
            this.lblName.Name = "lblName";
            this.lblName.Size = new System.Drawing.Size(100, 13);
            this.lblName.TabIndex = 0;
            this.lblName.Text = "Participant Name:";
            // 
            // txtName
            // 
            this.txtName.Location = new System.Drawing.Point(125, 22);
            this.txtName.Name = "txtName";
            this.txtName.Size = new System.Drawing.Size(200, 20);
            this.txtName.TabIndex = 1;
            // 
            // lblAge
            // 
            this.lblAge.AutoSize = true;
            this.lblAge.Location = new System.Drawing.Point(12, 58);
            this.lblAge.Name = "lblAge";
            this.lblAge.Size = new System.Drawing.Size(90, 13);
            this.lblAge.TabIndex = 2;
            this.lblAge.Text = "Participant Age:";
            // 
            // txtAge
            // 
            this.txtAge.Location = new System.Drawing.Point(125, 55);
            this.txtAge.Name = "txtAge";
            this.txtAge.Size = new System.Drawing.Size(100, 20);
            this.txtAge.TabIndex = 3;
            // 
            // lblEvents
            // 
            this.lblEvents.AutoSize = true;
            this.lblEvents.Location = new System.Drawing.Point(12, 93);
            this.lblEvents.Name = "lblEvents";
            this.lblEvents.Size = new System.Drawing.Size(85, 13);
            this.lblEvents.TabIndex = 4;
            this.lblEvents.Text = "Select Events:";
            // 
            // checkedListEvents
            // 
            this.checkedListEvents.FormattingEnabled = true;
            this.checkedListEvents.Location = new System.Drawing.Point(15, 116);
            this.checkedListEvents.Name = "checkedListEvents";
            this.checkedListEvents.Size = new System.Drawing.Size(310, 214);
            this.checkedListEvents.TabIndex = 5;
            // 
            // btnRegister
            // 
            this.btnRegister.Location = new System.Drawing.Point(125, 350);
            this.btnRegister.Name = "btnRegister";
            this.btnRegister.Size = new System.Drawing.Size(100, 30);
            this.btnRegister.TabIndex = 6;
            this.btnRegister.Text = "Register";
            this.btnRegister.UseVisualStyleBackColor = true;
            this.btnRegister.Click += new System.EventHandler(this.btnRegister_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(235, 350);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(90, 30);
            this.btnCancel.TabIndex = 7;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // RegisterForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(349, 400);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnRegister);
            this.Controls.Add(this.checkedListEvents);
            this.Controls.Add(this.lblEvents);
            this.Controls.Add(this.txtAge);
            this.Controls.Add(this.lblAge);
            this.Controls.Add(this.txtName);
            this.Controls.Add(this.lblName);
            this.Name = "RegisterForm";
            this.Text = "Register Participant";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.Label lblName;
        private System.Windows.Forms.TextBox txtName;
        private System.Windows.Forms.Label lblAge;
        private System.Windows.Forms.TextBox txtAge;
        private System.Windows.Forms.Label lblEvents;
        private System.Windows.Forms.CheckedListBox checkedListEvents;
        private System.Windows.Forms.Button btnRegister;
        private System.Windows.Forms.Button btnCancel;
    }
}