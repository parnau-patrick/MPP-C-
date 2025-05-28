using System;
using System.Windows.Forms;
using ConcursModel.domain;
using ConcursClient.service;
using log4net;

namespace ConcursClient.controller
{
    public partial class SignUpForm : Form
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(SignUpForm));
        private readonly ConcursServicesGrpcProxy serviceProxy;

        public event EventHandler<UserEventArgs> SignUpSuccessful;

        public class UserEventArgs : EventArgs
        {
            public User User { get; set; }
        }

        public SignUpForm(ConcursServicesGrpcProxy serviceProxy)
        {
            InitializeComponent();
            this.serviceProxy = serviceProxy;
        }

        private void btnSignUp_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text;
            string password = txtPassword.Text;
            string confirmPassword = txtConfirmPassword.Text;
            string officeName = txtOfficeName.Text;

            // Basic validation
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password) || 
                string.IsNullOrWhiteSpace(confirmPassword) || string.IsNullOrWhiteSpace(officeName))
            {
                MessageBox.Show("Please fill in all fields.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (password != confirmPassword)
            {
                MessageBox.Show("Passwords do not match.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // Register user
                User user = serviceProxy.RegisterUser(username, password, officeName);
                
                log.Info($"User {username} signed up successfully");
                MessageBox.Show("Sign-up successful! You can now log in.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                
                // Notify that sign-up was successful
                SignUpSuccessful?.Invoke(this, new UserEventArgs { User = user });
                this.Close();
            }
            catch (Exception ex)
            {
                log.Error($"Error during user sign-up: {ex.Message}", ex);
                MessageBox.Show("Error during sign-up: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        // InitializeComponent method - UI elements definition
        private void InitializeComponent()
        {
            this.lblUsername = new System.Windows.Forms.Label();
            this.txtUsername = new System.Windows.Forms.TextBox();
            this.lblPassword = new System.Windows.Forms.Label();
            this.txtPassword = new System.Windows.Forms.TextBox();
            this.lblConfirmPassword = new System.Windows.Forms.Label();
            this.txtConfirmPassword = new System.Windows.Forms.TextBox();
            this.lblOfficeName = new System.Windows.Forms.Label();
            this.txtOfficeName = new System.Windows.Forms.TextBox();
            this.btnSignUp = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lblUsername
            // 
            this.lblUsername.AutoSize = true;
            this.lblUsername.Location = new System.Drawing.Point(22, 25);
            this.lblUsername.Name = "lblUsername";
            this.lblUsername.Size = new System.Drawing.Size(58, 13);
            this.lblUsername.TabIndex = 0;
            this.lblUsername.Text = "Username:";
            // 
            // txtUsername
            // 
            this.txtUsername.Location = new System.Drawing.Point(143, 22);
            this.txtUsername.Name = "txtUsername";
            this.txtUsername.Size = new System.Drawing.Size(200, 20);
            this.txtUsername.TabIndex = 1;
            // 
            // lblPassword
            // 
            this.lblPassword.AutoSize = true;
            this.lblPassword.Location = new System.Drawing.Point(22, 57);
            this.lblPassword.Name = "lblPassword";
            this.lblPassword.Size = new System.Drawing.Size(56, 13);
            this.lblPassword.TabIndex = 2;
            this.lblPassword.Text = "Password:";
            // 
            // txtPassword
            // 
            this.txtPassword.Location = new System.Drawing.Point(143, 54);
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.PasswordChar = '*';
            this.txtPassword.Size = new System.Drawing.Size(200, 20);
            this.txtPassword.TabIndex = 3;
            // 
            // lblConfirmPassword
            // 
            this.lblConfirmPassword.AutoSize = true;
            this.lblConfirmPassword.Location = new System.Drawing.Point(22, 89);
            this.lblConfirmPassword.Name = "lblConfirmPassword";
            this.lblConfirmPassword.Size = new System.Drawing.Size(94, 13);
            this.lblConfirmPassword.TabIndex = 4;
            this.lblConfirmPassword.Text = "Confirm Password:";
            // 
            // txtConfirmPassword
            // 
            this.txtConfirmPassword.Location = new System.Drawing.Point(143, 86);
            this.txtConfirmPassword.Name = "txtConfirmPassword";
            this.txtConfirmPassword.PasswordChar = '*';
            this.txtConfirmPassword.Size = new System.Drawing.Size(200, 20);
            this.txtConfirmPassword.TabIndex = 5;
            // 
            // lblOfficeName
            // 
            this.lblOfficeName.AutoSize = true;
            this.lblOfficeName.Location = new System.Drawing.Point(22, 121);
            this.lblOfficeName.Name = "lblOfficeName";
            this.lblOfficeName.Size = new System.Drawing.Size(71, 13);
            this.lblOfficeName.TabIndex = 6;
            this.lblOfficeName.Text = "Office Name:";
            // 
            // txtOfficeName
            // 
            this.txtOfficeName.Location = new System.Drawing.Point(143, 118);
            this.txtOfficeName.Name = "txtOfficeName";
            this.txtOfficeName.Size = new System.Drawing.Size(200, 20);
            this.txtOfficeName.TabIndex = 7;
            // 
            // btnSignUp
            // 
            this.btnSignUp.Location = new System.Drawing.Point(143, 164);
            this.btnSignUp.Name = "btnSignUp";
            this.btnSignUp.Size = new System.Drawing.Size(90, 28);
            this.btnSignUp.TabIndex = 8;
            this.btnSignUp.Text = "Sign Up";
            this.btnSignUp.UseVisualStyleBackColor = true;
            this.btnSignUp.Click += new System.EventHandler(this.btnSignUp_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(253, 164);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(90, 28);
            this.btnCancel.TabIndex = 9;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // SignUpForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(364, 211);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnSignUp);
            this.Controls.Add(this.txtOfficeName);
            this.Controls.Add(this.lblOfficeName);
            this.Controls.Add(this.txtConfirmPassword);
            this.Controls.Add(this.lblConfirmPassword);
            this.Controls.Add(this.txtPassword);
            this.Controls.Add(this.lblPassword);
            this.Controls.Add(this.txtUsername);
            this.Controls.Add(this.lblUsername);
            this.Name = "SignUpForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Sign Up - Swimming Competition System";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.Label lblUsername;
        private System.Windows.Forms.TextBox txtUsername;
        private System.Windows.Forms.Label lblPassword;
        private System.Windows.Forms.TextBox txtPassword;
        private System.Windows.Forms.Label lblConfirmPassword;
        private System.Windows.Forms.TextBox txtConfirmPassword;
        private System.Windows.Forms.Label lblOfficeName;
        private System.Windows.Forms.TextBox txtOfficeName;
        private System.Windows.Forms.Button btnSignUp;
        private System.Windows.Forms.Button btnCancel;
    }
}