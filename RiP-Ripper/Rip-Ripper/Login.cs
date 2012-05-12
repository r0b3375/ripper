//////////////////////////////////////////////////////////////////////////
// Code Named: RiP-Ripper
// Function  : Extracts Images posted on RiP forums and attempts to fetch
//			   them to disk.
//
// This software is licensed under the MIT license. See license.txt for
// details.
// 
// Copyright (c) The Watcher
// Partial Rights Reserved.
// 
//////////////////////////////////////////////////////////////////////////
// This file is part of the RiP Ripper project base.

namespace RiPRipper
{
    using System;
    using System.Drawing;
    using System.Reflection;
    using System.Resources;
    using System.Windows.Forms;

    /// <summary>
    /// The Login Dialog
    /// </summary>
    public partial class Login : Form
    {
        public ResourceManager rm;

        /// <summary>
        /// Initializes a new instance of the <see cref="Login"/> class.
        /// </summary>
        public Login()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Set Language Strings
        /// </summary>
        private void AdjustCulture()
        {
            this.groupBox1.Text = this.rm.GetString("gbLoginHead");
            this.groupBox2.Text = this.rm.GetString("gbGuestLoginHead");

            this.label1.Text = this.rm.GetString("lblUser");
            this.label2.Text = this.rm.GetString("lblPass");
            this.label4.Text = this.rm.GetString("lblWarningLogin");
            this.label5.Text = this.rm.GetString("gbLanguage");

            this.checkBox1.Text = this.rm.GetString("chRememberCred");

            this.LoginBtn.Text = this.rm.GetString("logintext");
            this.GuestLoginButton.Text =  this.rm.GetString("guestLoginButton");
        }

        /// <summary>
        /// Loads the Form
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void LoginLoad(object sender, EventArgs e)
        {
            // Load Language Setting
            try
            {
                string sLanguage = Utility.LoadSetting("UserLanguage");

                switch (sLanguage)
                {
                    case "de-DE":
                        this.comboBox2.SelectedIndex = 0;
                        break;
                    case "fr-FR":
                        this.comboBox2.SelectedIndex = 1;
                        break;
                    case "en-EN":
                        this.comboBox2.SelectedIndex = 2;
                        break;
                    default:
                        this.comboBox2.SelectedIndex = 2;
                        break;
                }
            }
            catch (Exception)
            {
                this.comboBox2.SelectedIndex = 2;
            }
        }

        /// <summary>
        /// Trys to Login to the Forums
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void LoginBtnClick(object sender, EventArgs e)
        {
            // Encrypt Password
            this.textBox2.Text = Utility.EncodePassword(this.textBox2.Text).Replace("-", string.Empty).ToLower();

            LoginManager lgnMgr = new LoginManager(this.textBox1.Text, this.textBox2.Text);

            string lblWelcome = this.rm.GetString("lblWelcome"), lblFailed = this.rm.GetString("lblFailed");

            if (lgnMgr.DoLogin())
            {
                this.label3.Text = lblWelcome + this.textBox1.Text;
                this.label3.ForeColor = Color.Green;
                this.LoginBtn.Enabled = false;

                Utility.SaveSetting("User", this.textBox1.Text);
                Utility.SaveSetting("Password", this.textBox2.Text);

                this.timer1.Enabled = true;
            }
            else
            {
                this.label3.Text = lblFailed;
                this.label3.ForeColor = Color.Red;
            }
        }

        /// <summary>
        /// TODO : Remove Timer
        /// If Login sucessfully send user data to MainForm
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Timers.ElapsedEventArgs"/> instance containing the event data.</param>
        private void Timer1Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            this.timer1.Enabled = false;
            ((MainForm)Owner).bCameThroughCorrectLogin = true;

            var cacheController = CacheController.GetInstance();

            cacheController.userSettings.User = this.textBox1.Text;
            cacheController.userSettings.Pass = this.textBox2.Text;

            this.Close();
        }

        /// <summary>
        /// Changes the UI Language based on the selected Language
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void ComboBox2SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (this.comboBox2.SelectedIndex)
            {
                case 0:
                    this.rm = new ResourceManager("RiPRipper.Languages.german", Assembly.GetExecutingAssembly());
                    Utility.SaveSetting("UserLanguage", "de-DE");
                    break;
                case 1:
                    this.rm = new ResourceManager("RiPRipper.Languages.french", Assembly.GetExecutingAssembly());
                    Utility.SaveSetting("UserLanguage", "fr-FR");
                    break;
                case 2:
                    this.rm = new ResourceManager("RiPRipper.Languages.english", Assembly.GetExecutingAssembly());
                    Utility.SaveSetting("UserLanguage", "en-EN");
                    break;
            }

            this.AdjustCulture();
        }

        /// <summary>
        /// Login as Guest
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void GuestLoginButton_Click(object sender, EventArgs e)
        {
            ((MainForm)Owner).bCameThroughCorrectLogin = true;

            var cacheController = CacheController.GetInstance();

            cacheController.userSettings.GuestMode = true;

            this.Close();
        }
    }
}