/*!
    \file   LoginScreenState.cs
    \brief  VTank's default state - the screen in which a user can log into VTank.
    \author (C) Copyright 2008 by Vermont Technical College
*/
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Client.src.config;
using Microsoft.Xna.Framework.Input;
using Exceptions;
using GameForms.Forms;
using Client.src.service;
using GameForms.Controls;
using TomShane.Neoforce.Controls;
using System.Threading;
using Client.src.util;
using System.Collections.Generic;
using Client.src.util.game;

namespace Client.src.states.gamestate
{
    /// <summary>
    /// This state will allow the user to view the title screen and log into the game.
    /// </summary>
    public class LoginScreenState : State
    {
        #region Members
        LoginForm form;
        #endregion
        
        #region Constructors
        /// <summary>
        /// Constructor for the game.
        /// </summary>
        /// <param name="game">The VTank game object.</param>
        public LoginScreenState()
        {

        }
        #endregion

        #region Methods
        /// <summary>
        /// Initialize any components required by this state.
        /// </summary>
        public override void Initialize()
        {
            form = new LoginForm(ServiceManager.Game.Manager);
            form.Login.Click += new TomShane.Neoforce.Controls.EventHandler(LoginButton_Click);
            form.Exit.Click += new TomShane.Neoforce.Controls.EventHandler(Exit_Click);
            form.EnterPressed += new TomShane.Neoforce.Controls.EventHandler(form_EnterPressed);
            ServiceManager.Game.FormManager.SwitchWindows(form.Window);
        }

        /// <summary>
        /// Ensure that the window is no longer visible.
        /// </summary>
        private void EnsureClosed()
        {
            //form.Window.Close();
        }

        void Exit_Click(object sender, TomShane.Neoforce.Controls.EventArgs e)
        {
            ServiceManager.Game.Exit();
        }

        void form_EnterPressed(object sender, TomShane.Neoforce.Controls.EventArgs e)
        {
            PerformLogin(); 
        }

        /// <summary>
        /// Load any content (textures, fonts, etc) required for this state.
        /// </summary>
        public override void LoadContent()
        {
            
        }
        
        /// <summary>
        /// Unload any content (textures, fonts, etc) used by this state. Called when the state is removed.
        /// </summary>
        public override void UnloadContent()
        {
            EnsureClosed();
        }

        /// <summary>
        /// Logical updates occur here.
        /// </summary>
        /// <param name="gameTime">Delta time calculated by XNA.</param>
        public override void Update()
        {
            UpdateKeyHandler();
        }

        /// <summary>
        /// Draw this state to the screen.
        /// </summary>
        /// <param name="gameTime">Delta time calculated by XNA.</param>
        public override void Draw()
        {
            DrawBack();
        }

        /// <summary>
        /// Acts as an event handler for the "Login" button.  This method is called when the user
        /// clicks on the button.
        /// </summary>
        /// <param name="obj">Access to the object clicked. Unused.</param>
        /// <param name="e">Event arguments.</param>
        void LoginButton_Click(object sender, TomShane.Neoforce.Controls.EventArgs e)
        {
            PerformLogin();
        }

        /// <summary>
        /// Perform the Login action. This is invoked by clicking on the "Login" button or by
        /// some other event handler related to that.
        /// This method performs necessary error checking and error reporting.
        /// </summary>
        private void PerformLogin()
        {
            if (CheckValidity())
            {
                form.Login.Enabled = false;
                form.Login.Text = "Wait...";
                if (ServiceManager.Echelon == null)
                {
                    ServiceManager.ConnectToEchelonAsync((connectSuccessful) =>
                    {
                        if (connectSuccessful)
                        {
                            DoLogin();
                        }
                        else
                        {
                            // Unable to connect. The error is logged in MasterCommunicator.cs.
                            MessageBox.Show(ServiceManager.Game.Manager,
                                MessageBox.MessageBoxType.ERROR,
                                "Unable to connect to the authentication server. " +
                                "Check your connection and try again.", "Error");

                            ServiceManager.Game.Console.DebugPrint("Login failed.");
                        }
                    });
                }
                else
                {
                    DoLogin();
                }
            }
        }

        private void DoLogin()
        {
            try
            {
                ServiceManager.Echelon.LoginAsync(form.Username, form.Password,
                    Client.src.config.Options.ClientVersion.ToIceVersion(
                        ServiceManager.Game.Options.Version), LoginFinished);
                // TODO: Bring up a cancelable 'Connecting...' dialog.

            }
            catch (Network.Exception.InvalidValueException e)
            {
                // The client's username/password is bad -- this is thrown from the communicator.
                MessageBox.Show(ServiceManager.Game.Manager,
                    MessageBox.MessageBoxType.ERROR, e.Message, "Error");
                form.Login.Enabled = true;
                form.Login.Text = "Login";
            }
            catch (Exception e)
            {
                // Unable to connect. The error is logged in MasterCommunicator.cs.
                // TODO: Message box error. The exception message is user-friendly.
                MessageBox.Show(ServiceManager.Game.Manager,
                    MessageBox.MessageBoxType.ERROR,
                    e.Message, "Error");
                form.Login.Enabled = true;
                form.Login.Text = "Login";
            }
        }

        private bool CheckValidity()
        {
            if (!form.Username.Trim().Equals("") || !form.Password.Trim().Equals(""))
            {
                return true;
            }
            else
            {
                MessageBox.Show(ServiceManager.Game.Manager, MessageBox.MessageBoxType.ERROR, 
                    "Must fill out username/password fields", "Error");
            }
            return false;
        }

        /// <summary>
        /// This method acts as a callback for the LoginAsync method of the Echelon communicator.
        /// Once the callback attempt is finished -- success or not -- this method is invoked.
        /// </summary>
        /// <param name="result">Result of the login attempt.</param>
        private void LoginFinished(Network.Util.Result result)
        {
            if (result.Success)
            {
                //string latestVersion = ServiceManager.Echelon.CheckClientVersion();
                //System.Console.WriteLine("Latest client version is: " + latestVersion);
                CheckVersion();
            }

            ServiceManager.Game.Invoke(HandleLoginFinished, result);
        }

        /// <summary>
        /// Handle the login being finished on a separate thread.
        /// </summary>
        /// <param name="resultObj"></param>
        private void HandleLoginFinished(object resultObj)
        {
            Network.Util.Result result = (Network.Util.Result)resultObj;
            if (!result.Success)
            {
                // The login attempt failed.
                ServiceManager.Game.Console.DebugPrint(
                    "Login unsuccessful: {0}", result.Exception.Message);
                // TODO: Message box error. The message is user-friendly.
                MessageBox.Show(ServiceManager.Game.Manager,
                    MessageBox.MessageBoxType.ERROR,
                    result.Exception.Message, result.Exception.Message);
            }
            else
            {
                // The login attempt succeeded.
                ServiceManager.Game.Console.DebugPrint("Login successful.");

                ServiceManager.StateManager.ChangeState<TankListState>();
            }

            form.Login.Enabled = true;
            form.Login.Text = "Login";
        }

        /// <summary>
        /// Check with the server to see if the client is up to date.
        /// Displays a message box if out of date asking them if they would like to
        /// reinstall.  Terminates program and launches the browser if so.
        /// </summary>
        private void CheckVersion()
        {
            try
            {
                float currentVersion = float.Parse(Toolkit.GetClientVersion("version.ini").Replace("v", "").Replace("V", ""));
                float latestVersionFromServer = float.Parse(ServiceManager.Echelon.CheckClientVersion().Replace("v", "").Replace("V", ""));
                //bool mismatch = false;

                //System.Console.WriteLine("Current version: {0}; Latest version: {1}", currentVersion, latestVersionFromServer);
                
                if (latestVersionFromServer > currentVersion)
                {
                    MessageBox msgBox = new MessageBox(ServiceManager.Game.Manager,
                        MessageBox.MessageBoxType.YES_NO,
                        "Your client is out of date, would you like to update now? (Requires reinstall)",
                        "Version Mismatch");
                    msgBox.Show();

                    msgBox.WaitForClose();

                    if (msgBox.Window.ModalResult == ModalResult.Yes)
                    {
                        const string VTANK_WEBSITE_URL = @"http://vtank.cis.vtc.edu/downloads/";
                        System.Diagnostics.Process.Start(VTANK_WEBSITE_URL);
                        ServiceManager.Game.Exit();
                    }
                }
            }
            catch (Exception e)
            {
                System.Console.Error.WriteLine("[ERROR] Unable to detect version of client: {0}", e.Message);
            }
        }

        /// <summary>
        /// Register or unregister certain key presses.
        /// </summary>
        private void UpdateKeyHandler()
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                ServiceManager.Game.Exit();
            }
        }
        #endregion
    }
}
