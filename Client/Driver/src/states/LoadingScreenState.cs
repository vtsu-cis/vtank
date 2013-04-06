/*!
    \file   LoadingScreenState.cs
    \brief  State informing clients the game is being loaded
    \author (C) Copyright 2008 by Vermont Technical College
*/
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Threading;
using GameForms.Forms;
using Client.src.service;
using Network.Util;
using GameForms.Controls;
using Client.src.callbacks;
using System.IO;
using Client.src.util;
using System.Collections.Generic;
using Client.src.util.game;
using Renderer.Utils;
using Client.src.config;
using Client.src.events;
using Renderer.SceneTools.Entities;
using Renderer.SceneTools.Entities.Particles;

namespace Client.src.states.gamestate
{
    /// <summary>
    /// Loading screen state informs clients of what is happening between
    /// the server select state and the game play state
    /// </summary>
    public class LoadingScreenState : State
    {
        #region LoadingState Declaration
        /// <summary>
        /// Loading state enumeration to track where the loading thread is in terms of
        /// whether or not it's still running, whether it's ready to connect, or whether
        /// the loading was cancelled.
        /// </summary>
        private enum LoadingState
        {
            LOADING,
            READY,
            ERROR
        }
        #endregion

        #region Members
        private static bool loaded = false;
        private const string loadingFormName = "Loading";
        private string currentMap = "";
        private Map currentMapInstance = null;
        public object progressLock;
        private Thread thread;
        private LoadingState state;
        private GameServerInfo server;
        private int value = 0;
        private string message = "";
        private string errorMessage = null;
        private LoadingScreen form;
        private GamePlayState futureGame;
        private GameCallback clientCallback;
        private EventBuffer buffer = new EventBuffer();
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the value of the progress bar.
        /// </summary>
        private int Value
        {
            get
            {
                lock (this)
                {
                    return value;
                }
            }

            set
            {
                lock (this)
                {
                    this.value = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the message displayed on the loading screen.
        /// </summary>
        private string Message
        {
            get
            {
                lock (this)
                {
                    return message;
                }
            }

            set
            {
                lock (this)
                {
                    message = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the error message if the current state is ERROR.
        /// </summary>
        public string ErrorMessage
        {
            get
            {
                lock (this)
                {
                    return errorMessage;
                }
            }

            set
            {
                lock (this)
                {
                    errorMessage = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the state that the loader thread is in.
        /// </summary>
        private LoadingState State
        {
            get
            {
                lock (this)
                {
                    return state;
                }
            }

            set
            {
                lock (this)
                {
                    state = value;
                }
            }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Start the loading screen state, which initializes some basic form components.
        /// </summary>
        /// <param name="gameServer">Server to connect to.</param>
        public LoadingScreenState(GameServerInfo gameServer)
        {
            server = gameServer;
            if (ServiceManager.Game.IsMouseVisible == false)
            {
                ServiceManager.Game.IsMouseVisible = true;
            }

            form = new LoadingScreen(ServiceManager.Game.Manager);
            ServiceManager.Game.FormManager.SwitchWindows(form.Window);
            form.Cancel.Click += new TomShane.Neoforce.Controls.EventHandler(Cancel_Click);

            State = LoadingState.LOADING;
            thread = new Thread(new ThreadStart(Start));
        }
        #endregion

        #region Overridden Methods
        /// <summary>
        /// Initalizes components
        /// </summary>
        public override void Initialize()
        {
        }

        /// <summary>
        /// Loads content to be displayed on screen
        /// </summary>
        public override void LoadContent()
        {
            // TODO: Load loading screen splash screen.
            // TODO: Load loading bar textures.

            try
            {
                thread.Start();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
            }
        }

        /// <summary>
        /// Unloads content.
        /// </summary>
        public override void UnloadContent()
        {
            if (thread.IsAlive)
            {
                try
                {
                    thread.Interrupt();
                }
                catch (Exception e)
                {
                    ServiceManager.Game.Console.DebugPrint(
                        "[ERROR] At LoadingScreenState#UnloadContent(): {0}",
                        e.Message);
                }
            }
            
            if (futureGame != null)
            {
                ServiceManager.Game.BackgroundMovie.Pause();
                ServiceManager.MP3Player.Stop();
                ServiceManager.MP3Player.PlayPlaylist();
                futureGame.OnGameFinished += new GamePlayState.GameFinishHandler(OnGameFinished);
            }

            currentMap = null;
            currentMapInstance = null;
            thread = null;
            server = null;
            form = null;
            futureGame = null;
            clientCallback = null;
            buffer = null;
        }

        /// <summary>
        /// Resume the video once the game is finished.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnGameFinished(object sender, EventArgs e)
        {
            ServiceManager.Game.BackgroundMovie.Resume();
            ServiceManager.MP3Player.Stop();
            ServiceManager.MP3Player.Play(Client.src.service.services.VTank.MENU_PATH, true);
        }
        
        /// <summary>
        /// Checks to see if it is ready to proceed to the game play state
        /// </summary>
        /// <param name="gameTime">XNA Game time</param>
        public override void Update()
        {
            if (State == LoadingState.ERROR)
            {
                ServerListState state = new ServerListState(ErrorMessage);
                ServiceManager.StateManager.ChangeState(state);
            }
            else if (State == LoadingState.READY)
            {
                ServiceManager.StateManager.ChangeState(futureGame);
            }
            else
            {
                form.Value = Value;
                form.Message = Message;
            }
        }

        /// <summary>
        /// Draws the loading screen on screen
        /// </summary>
        public override void Draw()
        {
            DrawBack();
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Event response to the "cancel" button being clicked.
        /// </summary>
        /// <param name="sender">Object which sent the event. Unused.</param>
        /// <param name="e">Mouse properties saved during the event. Unused.</param>
        private void Cancel_Click(object sender, TomShane.Neoforce.Controls.EventArgs e)
        {
            Message = "Loading cancelled.";
            Value = 0;

            try
            {
                thread.Interrupt();
            }
            catch (Exception ex)
            {
                ServiceManager.Game.Console.DebugPrint(
                    "[ERROR] At LoadingScreenState#Cancel_Click(): {0}",
                    ex.Message);
            }

            ServiceManager.StateManager.ChangeState<ServerListState>();
        }

        /// <summary>
        /// Returns a list of weapon models.
        /// </summary>
        /// <returns></returns>
        private List<string> GetWeaponModels()
        {
            List<Weapon> weapons = WeaponLoader.GetWeaponsAsList();
            List<string> result = new List<string>();

            foreach (Weapon weapon in weapons)
            {
                result.Add(weapon.Model);
                result.Add(weapon.DeadModel);
            }

            return result;
        }

        /// <summary>
        /// Returns a list of projectile models.
        /// </summary>
        /// <returns></returns>
        private List<string> GetProjectileModels()
        {
            List<string> result = new List<string>();

            foreach (ProjectileData projectile in WeaponLoader.GetProjectiles().Values)
            {
                result.Add(projectile.Model);
            }

            return result;
        }
        #endregion

        #region Thread Methods

        #region Step 0
        /// <summary>
        /// The thread delegate which handles loading the game resources.
        /// </summary>
        private void Start()
        {
            State = LoadingState.LOADING;
            try
            {
                Value = 0;
                Message = "Cleaning up unused resources...";
                CleanOldResources();

                Value = 10;
                Message = "Establishing connection...";
                ConnectToGameServer();

                Value = 20;
                Message = "Loading map " + currentMap;
                GetCurrentMap();

                Value = 30;
                Message = "Loading game resources...";
                if (!LoadGameResources())
                {
                    // Causes the thread to exit without further issue.
                    State = LoadingState.ERROR;
                    throw new ThreadInterruptedException();
                }

                Value = 100;
                Message = "Starting game...";

                futureGame = new GamePlayState(clientCallback, currentMapInstance, buffer);
                clientCallback.Game = futureGame;

                State = LoadingState.READY;
            }
            catch (ThreadInterruptedException) { }
            catch (Exception ex)
            {
                ServiceManager.Game.Console.DebugPrint("{0}", ex);

                if (ex is Exceptions.VTankException)
                {
                    Exceptions.VTankException realEx = (Exceptions.VTankException)ex;
                    ErrorMessage = "Cannot connect:\n" + realEx.reason;
                }
                else
                {
                    ErrorMessage = "Cannot connect:\n" + ex.Message;
                }

                State = LoadingState.ERROR;
            }
        }
        #endregion

        #region Step 1
        /// <summary>
        /// Clean up unused resources used from the last game.
        /// </summary>
        private void CleanOldResources()
        {
            /*double memoryUsed = GC.GetTotalMemory(false) / 1024.0 / 1024.0;
            ServiceManager.Game.Console.DebugPrint(
                "Memory usage: {0:0.00} MB. {1} objects in ResourceManager.",
                memoryUsed, ServiceManager.Resources.Count);*/
            double memoryUsedBefore = GC.GetTotalMemory(false) / 1024f / 1024f;
            ServiceManager.Game.Console.DebugPrint("Memory used (before GC): {0:0.00} MB", 
                memoryUsedBefore);

            GC.Collect();

            // Allow objects to destruct:
            GC.WaitForPendingFinalizers();

            double memoryUsedAfter = GC.GetTotalMemory(true) / 1024f / 1024f;
            ServiceManager.Game.Console.DebugPrint(
                "Memory used (after GC): {0:0.00} MB ({1:0.00} MB freed)",
                memoryUsedAfter, memoryUsedBefore - memoryUsedAfter);
        }
        #endregion

        #region Step 2
        /// <summary>
        /// Attempts to connect with the game server
        /// </summary>
        private void ConnectToGameServer()
        {
            if (ServiceManager.Theater == null)
            {
                ServiceManager.ConnectToTheater(server);

                ServiceManager.Theater.ClearRegisteredCallbacks();

                clientCallback = new GameCallback(null, buffer);
                ClockSync clockCallback = new ClockSync();

                ServiceManager.Theater.RegisterCallback(clientCallback);
                ServiceManager.Theater.RegisterCallback(clockCallback);

                string key = ServiceManager.Echelon.RequestJoinGameServer(server);
                if (!ServiceManager.Theater.JoinServer(key))
                {
                    throw new Exception("Cannot connect to the game server.");
                }
            }
            else
            {

                ServiceManager.Theater.ClearRegisteredCallbacks();

                clientCallback = new GameCallback(null, buffer);
                ClockSync clockCallback = new ClockSync();

                ServiceManager.Theater.RegisterCallback(clientCallback);
                ServiceManager.Theater.RegisterCallback(clockCallback);

            }

            ServiceManager.CurrentServer = server;
            currentMap = ServiceManager.Theater.GetCurrentMapName();
        }
        #endregion

        #region Step 3
        /// <summary>
        /// Gets the current map being played
        /// </summary>
        private void GetCurrentMap()
        {
            Options options = ServiceManager.Game.Options;
            if (String.IsNullOrEmpty(currentMap))
            {
                throw new Exception("No map is currently being played.");
            }

            try
            {
                FileInfo f = new FileInfo(String.Format(
                    "{0}{1}{2}", options.MapsFolder, Path.DirectorySeparatorChar, currentMap));
                if (f.Exists)
                {
                    currentMapInstance = new Map(f.FullName);

                    string mapHash = currentMapInstance.SHA1Hash;
                    string currentHash = Hash.CalculateSHA1OfFile(f.FullName);

                    bool valid = ServiceManager.Echelon.GetProxy().HashIsValid(
                        currentMap, currentHash);
                    if (!valid)
                    {
                        f.Delete();
                        f = null;

                        DownloadMap(currentMap);
                    }
                }
                else
                {
                    DownloadMap(currentMap);
                }
            }
            catch (Exception ex)
            {
                ServiceManager.Game.Console.DebugPrint("[WARNING] Cannot open map: {0}", ex.Message);
                if (currentMapInstance == null)
                {
                    try
                    {
                        DownloadMap(currentMap);
                    }
                    catch { }
                }
            }
        }

        /// <summary>
        /// Download the map from Echelon.
        /// </summary>
        /// <param name="mapName">Filename of the map to be downloaded.</param>
        private void DownloadMap(string mapName)
        {
            Options options = ServiceManager.Game.Options;
            Message = String.Format("Downloading map {0}...", mapName);
            Value = 25;

            VTankObject.Map map = ServiceManager.Echelon.GetProxy().DownloadMap(mapName);

            Message = "Reading map...";

            Console.WriteLine("Finished downloading map {0}.", map.filename);
            try
                {
                    DirectoryInfo directory = new DirectoryInfo(options.MapsFolder);
                    if (!directory.Exists)
                    {
                        directory.Create();
                    }
                }
                catch (Exception)
                {
                    options.MapsFolder = String.Format(@"{0}\{1}\maps\",
                        Environment.GetEnvironmentVariable("APPDATA"), "VTank");
                    try
                    {
                        Directory.CreateDirectory(options.MapsFolder);
                    }
                    catch (Exception) { }
                }

            Map realMap = new Map(
                map.title, map.filename, (uint)map.width, (uint)map.height);

            List<int> buf = new List<int>();
            for (int i = 0; i < map.supportedGameModes.Length; i++)
            {
                buf.Add(map.supportedGameModes[i]);
            }

            realMap.SetGameModes(buf);

            VTankObject.Tile[] tiles = map.tileData;
            // Set the tile data.
            for (uint y = 0; y < (uint)map.height; y++)
            {
                for (uint x = 0; x < (uint)map.width; x++)
                {
                    int position = (int)(y) * map.width + (int)(x);
                    VTankObject.Tile tempTile = tiles[position];
                    Tile tile = new Tile(
                        (uint)tempTile.id, (ushort)tempTile.objectId, (ushort)tempTile.eventId,
                        tempTile.passable, (ushort)tempTile.height, (ushort)tempTile.type,
                        (ushort)tempTile.effect);
                    realMap.SetTile(x, y, tile);
                }
            }

            try
            {
                realMap.SaveMap(options.MapsFolder);
            }
            catch (Exception)
            {
                options.MapsFolder = String.Format(@"{0}\{1}\maps\",
                    Environment.GetEnvironmentVariable("APPDATA"), "VTank");
                
                try
                {
                    Directory.CreateDirectory(options.MapsFolder);
                    realMap.SaveMap(options.MapsFolder);
                }
                catch (Exception ex)
                {
                    ServiceManager.Game.Console.DebugPrint("[WARNING] Cannot save map: {0}", ex.Message);
                }
            }

            currentMapInstance = realMap;
        }
        #endregion

        #region Step 4
        /// <summary>
        /// Load game resources (tiles, models) into memory.
        /// </summary>
        private bool LoadGameResources()
        {
            ServiceManager.Game.Renderer.ActiveScene.ClearAll();

            if (loaded)
            {
                // Only attempt to load the game resources once. This way subsequent loading
                // times are much faster.
                return true;
            }

            Value = 40;
            if (!TileList.Initialized)
            {
                Message = "Loading tile textures...";
                if (!TileList.Read()) 
                {
                    // The error message is displayed within the parser, so never mind.
                    return false;
                }
                Value = 45;
            }

            Message = "Loading background textures...";
            ServiceManager.Resources.GetTexture2D(
                "misc\\background\\background2");

            Message = "Loading tank models...";
            Value = 50;

            // Pre-load the model resources so that it doesn't have to do so in-game.
            List<object> tankList = GameForms.Utils.GetTankModels();
            foreach (string tankName in tankList)
            {
                ServiceManager.Resources.GetModel(
                    String.Format("tanks{0}{1}", Path.DirectorySeparatorChar, tankName));
            }

            Message = "Loading weapon models...";
            Value = 60;

            List<string> weaponList = GetWeaponModels();
            foreach (string weaponName in weaponList)
            {
                ServiceManager.Resources.GetModel(
                    String.Format("weapons{0}{1}", Path.DirectorySeparatorChar, weaponName));
            }

            Message = "Loading projectile models...";
            Value = 70;

            List<string> projectileList = GetProjectileModels();
            foreach (string projectileName in projectileList)
            {
                ServiceManager.Resources.GetModel(
                    String.Format("projectiles{0}{1}", Path.DirectorySeparatorChar, projectileName));
            }


            Message = "Loading utility models...";
            Value = 75;

            List<string> utilityList = GameForms.Utils.GetUtilityModels();
            foreach (string utilityName in utilityList)
            {
                ServiceManager.Resources.GetModel(
                    String.Format("powerups{0}{1}", Path.DirectorySeparatorChar, utilityName));
            }
            Message = "Loading effects...";
            Value = 80;

            Message = "Loading event models...";
            Value = 85;
            List<string> eventList = Toolkit.GetEventList();
            foreach (string eventModel in eventList)
            {
                ServiceManager.Resources.GetModel(@"events\" + eventModel);
            }

            Message = "Loading renderer assets...";
            Value = 90;
            if (!RendererAssetLoader.HasInitialized)
            {
                RendererAssetLoader.Initialize();
            }

            ParticleEmitterSettings pset = Renderer.RendererAssetPool.ParticleEmitterSettings["Utility"];
            new ParticleEmitter(pset);

            Message = "Loading tank skins...";
            Value = 95;
            List<string> skinList = Toolkit.GetFullSkinList();
            foreach (string skin in skinList)
            {
                string path = String.Format("{0}{1}", Constants.DEFAULT_SKIN_DIR, skin);
                ServiceManager.Resources.Load<Texture2D>(path);
            }

            loaded = true;
            return true;
        }
        #endregion

        #endregion
    }
}

