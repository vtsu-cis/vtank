/*!
    \file   ServerListState.cs
    \brief  VTank's server list state - The fourth state used in VTank, displays a 
            list of game servers that are available to join.
    \author (C) Copyright 2009 by Vermont Technical College
*/
using System;
using System.Collections.Generic;

using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Client.src.config;
using Client.src.callbacks;
using Exceptions;
using GameForms.Forms;
using Client.src.service;
using Client.src.util;
using Network.Util;
using GameForms.Controls;


namespace Client.src.states.gamestate
{
    public class ServerListState : State
    {
        #region Members
        ServerList form;

        private static readonly String[] headers = new String[] { 
            "Server", "Game Type", "Map Name", "Players", "Ping", "Approved"};
        private List<String[]> serverInfo;
        private GameServerInfo[] serverList;
        private GameServerInfo selectedServer = null;
        private string errorMessage;
        #endregion

        #region Constructors
        public ServerListState()
            : this(null)
        {
        }

        public ServerListState(string errorMessage)
        {
            this.errorMessage = errorMessage;

            ServiceManager.DestroyTheaterCommunicator();
        }
        #endregion

        /// <summary>
        /// Initialize any components required by this state.
        /// </summary>
        public override void Initialize()
        {
            serverList = ServiceManager.Echelon.GetGameServerList();
            serverInfo = new List<String[]>();

            foreach (Network.Util.GameServerInfo server in serverList)
            {
                serverInfo.Add(new String[] {
                    server.Name,
                    Toolkit.GameModeToString(server.CurrentGameMode),
                    server.CurrentMap.Substring(0, server.CurrentMap.IndexOf(".vtmap")),
                    server.NumberOfPlayers.ToString() + " / " + server.PlayerLimit.ToString(),
                    server.GetFormattedAverageLatency(),
                    server.Approved ? "Yes" : "No"
                });
            }

            ServiceManager.Game.Console.DebugPrint(
                "{0} game servers loaded.", serverList.Length);

            form = new ServerList(ServiceManager.Game.Manager, headers, serverInfo);
            ServiceManager.Game.FormManager.SwitchWindows(form.Window);

            form.Play.Enabled = false;
            form.Back.Click += new TomShane.Neoforce.Controls.EventHandler(Back_Click);
            form.Refresh.Click += new TomShane.Neoforce.Controls.EventHandler(Refresh_Click);
            form.Play.Click += new TomShane.Neoforce.Controls.EventHandler(Play_Click);
            form.SelectionChanged += new TomShane.Neoforce.Controls.EventHandler(SelectionChanged);
            if (serverList.Length > 0)
            {
                form.SelectedIndex = 0;
                form.Play.Enabled = true;
            }

            if (errorMessage != null)
            {
                MessageBox alert = new MessageBox(ServiceManager.Game.Manager,
                    MessageBox.MessageBoxType.ERROR,
                    errorMessage, "Cannot connect.");
            }
        }



        /// <summary>
        /// Load any content (textures, fonts, etc) required for this state.
        /// </summary>
        public override void LoadContent()
        {
            
        }
        
        /// <summary>
        /// Unload any content (textures, fonts, etc) used by this state.
        /// Called when the state is removed.
        /// </summary>
        public override void UnloadContent()
        {
            serverList = null;
        }

        /// <summary>
        /// Logical updates occur here.
        /// </summary>
        /// <param name="gameTime">Delta time calculated by XNA.</param>
        public override void Update()
        {
            if (serverInfo.Count < serverList.Length)
            {
                this.RefreshServerList();
            }

            for (int i = 0; i < serverList.Length; i++)
            {
                // TODO: Update 'data'. This could go out of range.                
                serverInfo[i][4] = serverList[i].GetFormattedAverageLatency();
            }
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
        /// Acts as an event handler for the "Back" button.  This method is called when the user
        /// clicks on the button. The result of the action is that it switches to the
        /// tank list state.
        /// </summary>
        /// <param name="obj">Access to the object clicked. Unused.</param>
        /// <param name="e">Event arguments. Unused.</param>
        void Back_Click(object sender, TomShane.Neoforce.Controls.EventArgs e)
        {
            ServiceManager.StateManager.ChangeState<TankListState>();
        }

        /// <summary>
        /// Acts as an event handler for the "Join" button.  This method is called when the user
        /// clicks on the button.
        /// </summary>
        /// <param name="obj">Access to the object clicked. Unused.</param>
        /// <param name="e">Event arguments. Unused.</param>
        void Play_Click(object sender, TomShane.Neoforce.Controls.EventArgs e)
        {
            if (selectedServer != null)
            {
                ServiceManager.StateManager.ChangeState(new LoadingScreenState(selectedServer));
            }
            else
            {
                MessageBox.Show(
                    ServiceManager.Game.Manager, MessageBox.MessageBoxType.ERROR,
                    "No server has been selected.", "No server selected.");
            }
        }

       
        /// <summary>
        /// Event handler for when a user selects a server
        /// </summary>
        /// <param name="obj">The highlighted Field in the Listview. Used</param>
        /// <param name="e">Not used</param>
        void SelectionChanged(object sender, TomShane.Neoforce.Controls.EventArgs e)
        {
            form.Play.Enabled = true;
            string[] serverInfo = form.Items[form.SelectedIndex];
            foreach (GameServerInfo server in serverList)
            {
                if (server.Name == serverInfo[0])
                {
                    selectedServer = server;
                }
            }
        }

        /// <summary>
        /// Acts as an event handler for the "Refresh" button.  This method is called when the user
        /// clicks on the button.
        /// </summary>
        /// <param name="obj">Access to the object clicked. Unused.</param>
        /// <param name="e">Event arguments. Unused.</param>
        void Refresh_Click(object sender, TomShane.Neoforce.Controls.EventArgs e)
        {
            this.RefreshServerList();
        }

        void RefreshServerList()
        {
            // This call re-compiles the list of game servers.
            // TODO: It would be ideal to have some sort of 'loading' animation or message box
            // for getting a fresh game server list.
            serverList = ServiceManager.Echelon.GetGameServerList();
            serverInfo.Clear();

            foreach (Network.Util.GameServerInfo server in serverList)
            {
                serverInfo.Add(new String[] {
                    server.Name,
                    Toolkit.GameModeToString(server.CurrentGameMode),
                    server.CurrentMap.Substring(0, server.CurrentMap.IndexOf(".vtmap")),
                    server.NumberOfPlayers.ToString() + " / " + server.PlayerLimit.ToString(),
                    server.GetFormattedAverageLatency(),
                    server.Approved ? "Yes" : "No"
                });
            }

            ServiceManager.Game.Console.DebugPrint(
                "{0} game servers loaded.", serverList.Length);
        }
    }
}

