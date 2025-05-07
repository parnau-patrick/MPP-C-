using System;
using System.Collections.Generic;
using System.Windows.Forms;
using ConcursModel.domain;
using ConcursClient.service;
using ConcursNetworking.dto;
using log4net;

namespace ConcursClient.controller
{
    public partial class ParticipantSearchForm : Form
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(ParticipantSearchForm));
        private readonly ConcursServicesJsonProxy serviceProxy;

        public ParticipantSearchForm(ConcursServicesJsonProxy serviceProxy)
        {
            InitializeComponent();
            this.serviceProxy = serviceProxy;
            
            LoadEvents();
        }

        private void LoadEvents()
        {
            log.Info("Loading events for search form");
            try
            {
                var events = serviceProxy.GetAllEvents();
                cmbEvents.Items.Clear();
                
                foreach (var evt in events)
                {
                    cmbEvents.Items.Add(evt);
                }
                
                if (cmbEvents.Items.Count > 0)
                {
                    cmbEvents.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                log.Error("Error loading events", ex);
                MessageBox.Show("Error loading events: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            if (cmbEvents.SelectedItem == null)
            {
                MessageBox.Show("Please select an event first.", "Search", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            log.Info("Searching participants for selected event");
            try
            {
                Event selectedEvent = (Event)cmbEvents.SelectedItem;
                var participants = serviceProxy.GetParticipantsByEvent(selectedEvent.Id);
                
                listViewParticipants.Items.Clear();
                
                foreach (var participant in participants)
                {
                    ListViewItem item = new ListViewItem(participant.Id.ToString());
                    item.SubItems.Add(participant.Name);
                    item.SubItems.Add(participant.Age.ToString());
                    item.SubItems.Add(participant.EventsCount.ToString());
                    
                    item.Tag = participant;
                    listViewParticipants.Items.Add(item);
                }
                
                lblResults.Text = $"Found {listViewParticipants.Items.Count} participants for {selectedEvent}";
            }
            catch (Exception ex)
            {
                log.Error("Error searching participants", ex);
                MessageBox.Show("Error searching participants: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // InitializeComponent method - UI elements definition
        private void InitializeComponent()
        {
            this.cmbEvents = new System.Windows.Forms.ComboBox();
            this.lblSelectEvent = new System.Windows.Forms.Label();
            this.btnSearch = new System.Windows.Forms.Button();
            this.listViewParticipants = new System.Windows.Forms.ListView();
            this.columnId = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnAge = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnEventCount = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.lblResults = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // cmbEvents
            // 
            this.cmbEvents.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbEvents.FormattingEnabled = true;
            this.cmbEvents.Location = new System.Drawing.Point(112, 26);
            this.cmbEvents.Name = "cmbEvents";
            this.cmbEvents.Size = new System.Drawing.Size(240, 21);
            this.cmbEvents.TabIndex = 0;
            // 
            // lblSelectEvent
            // 
            this.lblSelectEvent.AutoSize = true;
            this.lblSelectEvent.Location = new System.Drawing.Point(12, 29);
            this.lblSelectEvent.Name = "lblSelectEvent";
            this.lblSelectEvent.Size = new System.Drawing.Size(94, 13);
            this.lblSelectEvent.TabIndex = 1;
            this.lblSelectEvent.Text = "Select Event:";
            // 
            // btnSearch
            // 
            this.btnSearch.Location = new System.Drawing.Point(363, 24);
            this.btnSearch.Name = "btnSearch";
            this.btnSearch.Size = new System.Drawing.Size(75, 23);
            this.btnSearch.TabIndex = 2;
            this.btnSearch.Text = "Search";
            this.btnSearch.UseVisualStyleBackColor = true;
            this.btnSearch.Click += new System.EventHandler(this.btnSearch_Click);
            // 
            // listViewParticipants
            // 
            this.listViewParticipants.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnId,
            this.columnName,
            this.columnAge,
            this.columnEventCount});
            this.listViewParticipants.FullRowSelect = true;
            this.listViewParticipants.GridLines = true;
            this.listViewParticipants.HideSelection = false;
            this.listViewParticipants.Location = new System.Drawing.Point(15, 91);
            this.listViewParticipants.Name = "listViewParticipants";
            this.listViewParticipants.Size = new System.Drawing.Size(520, 297);
            this.listViewParticipants.TabIndex = 3;
            this.listViewParticipants.UseCompatibleStateImageBehavior = false;
            this.listViewParticipants.View = System.Windows.Forms.View.Details;
            // 
            // columnId
            // 
            this.columnId.Text = "ID";
            this.columnId.Width = 40;
            // 
            // columnName
            // 
            this.columnName.Text = "Name";
            this.columnName.Width = 200;
            // 
            // columnAge
            // 
            this.columnAge.Text = "Age";
            this.columnAge.Width = 80;
            // 
            // columnEventCount
            // 
            this.columnEventCount.Text = "Events Count";
            this.columnEventCount.Width = 100;
            // 
            // lblResults
            // 
            this.lblResults.AutoSize = true;
            this.lblResults.Location = new System.Drawing.Point(12, 65);
            this.lblResults.Name = "lblResults";
            this.lblResults.Size = new System.Drawing.Size(0, 13);
            this.lblResults.TabIndex = 4;
            // 
            // ParticipantSearchForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(549, 400);
            this.Controls.Add(this.lblResults);
            this.Controls.Add(this.listViewParticipants);
            this.Controls.Add(this.btnSearch);
            this.Controls.Add(this.lblSelectEvent);
            this.Controls.Add(this.cmbEvents);
            this.Name = "ParticipantSearchForm";
            this.Text = "Search Participants";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.ComboBox cmbEvents;
        private System.Windows.Forms.Label lblSelectEvent;
        private System.Windows.Forms.Button btnSearch;
        private System.Windows.Forms.ListView listViewParticipants;
        private System.Windows.Forms.ColumnHeader columnId;
        private System.Windows.Forms.ColumnHeader columnName;
        private System.Windows.Forms.ColumnHeader columnAge;
        private System.Windows.Forms.ColumnHeader columnEventCount;
        private System.Windows.Forms.Label lblResults;
    }
}