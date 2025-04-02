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
    public partial class LoginForm : Form
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly service.UserService userService;

        public event EventHandler<LoginEventArgs> LoginSuccessful;

        public LoginForm(service.UserService userService)
        {
            InitializeComponent();
            this.userService = userService;
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text;
            string password = txtPassword.Text;

            try
            {
                User user = userService.Login(username, password);
                if (user != null)
                {
                    log.Info("Login successful for user: " + username);
                    LoginSuccessful?.Invoke(this, new LoginEventArgs { User = user });
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                log.Error("Login failed for user: " + username, ex);
                MessageBox.Show("Invalid username or password.", "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private void lnkSignUp_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            log.Info("Opening Sign-Up form");
            
            // Create and show the sign-up form
            SignUpForm signUpForm = new SignUpForm(userService);
            signUpForm.SignUpSuccessful += SignUpForm_SignUpSuccessful;
            signUpForm.ShowDialog();
        }
        
        private void SignUpForm_SignUpSuccessful(object sender, LoginEventArgs e)
        {
            // When sign-up is successful, we can either:
            // 1. Automatically log the user in and close this form
            // 2. Or just inform the user to now log in with their credentials
            
            // For now, we'll go with option 2
            txtUsername.Text = e.User.Username;
            txtPassword.Focus();
        }

        private void InitializeComponent()
        {
            this.txtUsername = new System.Windows.Forms.TextBox();
            this.txtPassword = new System.Windows.Forms.TextBox();
            this.lblUsername = new System.Windows.Forms.Label();
            this.lblPassword = new System.Windows.Forms.Label();
            this.btnLogin = new System.Windows.Forms.Button();
            this.lnkSignUp = new System.Windows.Forms.LinkLabel();
            this.SuspendLayout();
            // 
            // txtUsername
            // 
            this.txtUsername.Location = new System.Drawing.Point(145, 39);
            this.txtUsername.Name = "txtUsername";
            this.txtUsername.Size = new System.Drawing.Size(173, 20);
            this.txtUsername.TabIndex = 0;
            // 
            // txtPassword
            // 
            this.txtPassword.Location = new System.Drawing.Point(145, 78);
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.PasswordChar = '*';
            this.txtPassword.Size = new System.Drawing.Size(173, 20);
            this.txtPassword.TabIndex = 1;
            // 
            // lblUsername
            // 
            this.lblUsername.AutoSize = true;
            this.lblUsername.Location = new System.Drawing.Point(46, 42);
            this.lblUsername.Name = "lblUsername";
            this.lblUsername.Size = new System.Drawing.Size(58, 13);
            this.lblUsername.TabIndex = 2;
            this.lblUsername.Text = "Username:";
            // 
            // lblPassword
            // 
            this.lblPassword.AutoSize = true;
            this.lblPassword.Location = new System.Drawing.Point(46, 81);
            this.lblPassword.Name = "lblPassword";
            this.lblPassword.Size = new System.Drawing.Size(56, 13);
            this.lblPassword.TabIndex = 3;
            this.lblPassword.Text = "Password:";
            // 
            // btnLogin
            // 
            this.btnLogin.Location = new System.Drawing.Point(145, 119);
            this.btnLogin.Name = "btnLogin";
            this.btnLogin.Size = new System.Drawing.Size(75, 23);
            this.btnLogin.TabIndex = 4;
            this.btnLogin.Text = "Login";
            this.btnLogin.UseVisualStyleBackColor = true;
            this.btnLogin.Click += new System.EventHandler(this.btnLogin_Click);
            // 
            // lnkSignUp
            // 
            this.lnkSignUp.AutoSize = true;
            this.lnkSignUp.Location = new System.Drawing.Point(237, 124);
            this.lnkSignUp.Name = "lnkSignUp";
            this.lnkSignUp.Size = new System.Drawing.Size(81, 13);
            this.lnkSignUp.TabIndex = 5;
            this.lnkSignUp.TabStop = true;
            this.lnkSignUp.Text = "Create Account";
            this.lnkSignUp.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkSignUp_LinkClicked);
            // 
            // LoginForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(358, 169);
            this.Controls.Add(this.lnkSignUp);
            this.Controls.Add(this.btnLogin);
            this.Controls.Add(this.lblPassword);
            this.Controls.Add(this.lblUsername);
            this.Controls.Add(this.txtPassword);
            this.Controls.Add(this.txtUsername);
            this.Name = "LoginForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Swimming Competition System - Login";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.TextBox txtUsername;
        private System.Windows.Forms.TextBox txtPassword;
        private System.Windows.Forms.Label lblUsername;
        private System.Windows.Forms.Label lblPassword;
        private System.Windows.Forms.Button btnLogin;
        private System.Windows.Forms.LinkLabel lnkSignUp;
    }

    public class LoginEventArgs : EventArgs
    {
        public User User { get; set; }
    }
}