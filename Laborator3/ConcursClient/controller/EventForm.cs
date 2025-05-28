using System;
using System.Collections.Generic;
using System.Windows.Forms;
using ConcursModel.domain;
using ConcursModel.domain.observer;
using ConcursClient.service;
using ConcursNetworking.dto;
using log4net;

namespace ConcursClient.controller
{
    public partial class EventsForm : Form, IObserver
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(EventsForm));
        private readonly ConcursServicesGrpcProxy serviceProxy;

        public EventsForm(ConcursServicesGrpcProxy serviceProxy)
        {
            InitializeComponent();
            this.serviceProxy = serviceProxy;
            serviceProxy.AddObserver(this);
            this.FormClosed += EventsForm_FormClosed;
            
            LoadEvents();
        }

        private void EventsForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            serviceProxy.RemoveObserver(this);
        }

        private void LoadEvents()
        {
            log.Info("Loading events with participant counts");
            try
            {
                List<EventDTO> events = serviceProxy.GetAllEventsWithParticipantCounts();
                listViewEvents.Items.Clear();
                
                foreach (var evt in events)
                {
                    ListViewItem item = new ListViewItem(evt.Id.ToString());
                    item.SubItems.Add(evt.Distance + "m");
                    item.SubItems.Add(evt.Style);
                    item.SubItems.Add(evt.ParticipantsCount.ToString());
                    
                    item.Tag = evt;
                    listViewEvents.Items.Add(item);
                }
            }
            catch (Exception ex)
            {
                log.Error("Error loading events", ex);
                MessageBox.Show("Error loading events: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void Update()
        {
            // This method will be called when a new participant is registered
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new MethodInvoker(delegate { LoadEvents(); }));
            }
            else
            {
                LoadEvents();
            }
        }

        // InitializeComponent method - UI elements definition
        private void InitializeComponent()
        {
            this.listViewEvents = new System.Windows.Forms.ListView();
            this.columnId = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnDistance = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnStyle = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnParticipants = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.lblTitle = new System.Windows.Forms.Label();
            this.SuspendLayout();
            //
            // listViewEvents
            // 
            this.listViewEvents.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnId,
            this.columnDistance,
            this.columnStyle,
            this.columnParticipants});
            this.listViewEvents.FullRowSelect = true;
            this.listViewEvents.GridLines = true;
            this.listViewEvents.HideSelection = false;
            this.listViewEvents.Location = new System.Drawing.Point(12, 53);
            this.listViewEvents.Name = "listViewEvents";
            this.listViewEvents.Size = new System.Drawing.Size(525, 335);
            this.listViewEvents.TabIndex = 0;
            this.listViewEvents.UseCompatibleStateImageBehavior = false;
            this.listViewEvents.View = System.Windows.Forms.View.Details;
            // 
            // columnId
            // 
            this.columnId.Text = "ID";
            this.columnId.Width = 40;
            // 
            // columnDistance
            // 
            this.columnDistance.Text = "Distance";
            this.columnDistance.Width = 120;
            // 
            // columnStyle
            // 
            this.columnStyle.Text = "Style";
            this.columnStyle.Width = 120;
            // 
            // columnParticipants
            // 
            this.columnParticipants.Text = "Participants";
            this.columnParticipants.Width = 100;
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTitle.Location = new System.Drawing.Point(12, 19);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(149, 20);
            this.lblTitle.TabIndex = 1;
            this.lblTitle.Text = "Swimming Events";
            // 
            // EventsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(549, 400);
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.listViewEvents);
            this.Name = "EventsForm";
            this.Text = "Swimming Events";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.ListView listViewEvents;
        private System.Windows.Forms.ColumnHeader columnId;
        private System.Windows.Forms.ColumnHeader columnDistance;
        private System.Windows.Forms.ColumnHeader columnStyle;
        private System.Windows.Forms.ColumnHeader columnParticipants;
        private System.Windows.Forms.Label lblTitle;
    }
}