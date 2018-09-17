using System;
using AIFramework.Runner;
using Network.Main;
using Network.Game;
using Network.Util;
using AIFramework.Util;
using AIFramework.Runner.Callbacks;
using AIFramework.Bot.EventArgs;
using AIFramework.Bot.Game;
using System.Text;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using AIFramework.Util.Event;
using AIFramework.Util.Modes;
using AIFramework.Util.Modes.Impl;

namespace AIFramework.Bot
{
    public abstract class VTankBot : IDisposable
    {
        #region Members
        private ClientI clientCallback;
        private ClockSync clockCallback;
        private string localTankName;
        private delegate void Callback(object o);
        private string currentMapName;
        private EventBuffer buffer;
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the bot runner for this bot.
        /// </summary>
        protected BotRunner BotRunner
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the target server address.
        /// </summary>
        public TargetServer ServerAddress
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the authentication information for this bot.
        /// </summary>
        public AuthInfo AuthInfo
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the master communicator.
        /// </summary>
        public MasterCommunicator MainServer
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the game commnicator.
        /// </summary>
        public GameCommunicator GameServer
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the tank's name.
        /// </summary>
        public string TankName
        {
            get
            {
                return localTankName;
            }

            internal set
            {
                localTankName = value;
            }
        }

        /// <summary>
        /// Gets or sets the bot's preferred server name. None if it doesn't care.
        /// </summary>
        public string PreferredServerName
        {
            get;
            set;
        }

        /// <summary>
        /// Store information about the game.
        /// </summary>
        public GameTracker Game
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the local player (you).
        /// </summary>
        public Player Player
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the current game mode.
        /// </summary>
        public VTankObject.GameMode GameMode
        {
            get;
            private set;
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Constructs a VTankBot.
        /// </summary>
        /// <param name="runner">Parent bot runner.</param>
        /// <param name="server">Target Echelon server.</param>
        /// <param name="auth">Authentication information.</param>
        public VTankBot(BotRunner runner, TargetServer server, AuthInfo auth)
        {
            BotRunner = runner;
            ServerAddress = server;
            AuthInfo = auth;
            Game = new GameTracker(this);
            buffer = new EventBuffer();

            CreateMasterCommunicator();
        }
        #endregion

        #region Public Methods
        public void SelectTank(string tankName)
        {
            localTankName = tankName;
            MainServer.SelectTank(tankName);
        }

        /// <summary>
        /// Connect to a target game server. Throws an exception if something goes
        /// wrong.
        /// </summary>
        /// <param name="server">Game server to connect to.</param>
        public void ConnectToGameServer(GameServerInfo server)
        {
            string key = MainServer.RequestJoinGameServer(server);

            GameServer = new GameCommunicator(
                server.Host, server.Port, server.UseGlacier2);
            GameServer.LogFile = AuthInfo.Username + "GameLog.log";
            
            if (!GameServer.Connect())
            {
                HandleError("Could not connect to the game server: reason unknown.");
            }
            
            clientCallback = new ClientI(BotRunner, this, buffer);
            clockCallback = new ClockSync();

            GameServer.RegisterCallback(clientCallback);
            GameServer.RegisterCallback(clockCallback);

            if (!GameServer.JoinServer(key))
            {
                HandleError("Could not join the game server: invalid key.");
            }
            Thread.Sleep(100);

            Debugger.Write("{0}: Connected to the game server! Ping: {1}", 
                AuthInfo.Username, server.GetFormattedAverageLatency());

            RefreshPlayerList();
            DownloadAndLoadMap();
            Game.GameModeHandler = CreateGameHandler(GameMode);
            GameServer.Ready();
        }

        /// <summary>
        /// Refresh the player list.
        /// </summary>
        public void RefreshPlayerList()
        {
            Game.Reset();
            
            bool localPlayerFound = false;
            GameSession.Tank[] tanks = GameServer.GetPlayerList();
            for (int i = 0; i < tanks.Length; ++i)
            {
                bool isLocalPlayer = false;
                Player newPlayer = new Player(tanks[i]);
                if (newPlayer.Name.Equals(localTankName, StringComparison.CurrentCultureIgnoreCase))
                {
                    // Found local player.
                    Player = newPlayer;
                    isLocalPlayer = true;
                    localPlayerFound = true;
                }

                Game.AddPlayer(newPlayer, isLocalPlayer);
            }

            if (!localPlayerFound)
            {
                throw new Exception("Fatal error: Local player not found in player list.");
            }
        }

        /// <summary>
        /// Downloads the current map if necessary.
        /// </summary>
        public void DownloadAndLoadMap()
        {
            const string MapFolder = "maps";
            DirectoryInfo dir = new DirectoryInfo(MapFolder);
            if (!dir.Exists)
            {
                dir.Create();
            }

            currentMapName = GameServer.GetCurrentMapName();
            GameMode = GameServer.GetCurrentGameMode();

            string fileName = String.Format("{0}{1}{2}",
                MapFolder, System.IO.Path.DirectorySeparatorChar, currentMapName);
            
            Map playableMap = MapDownloader.GetMap(currentMapName, fileName, MainServer);

            Game.CurrentMap = playableMap;
        }

        /// <summary>
        /// Shutdown the bot's communications, disabling future event callbacks and
        /// action packets.
        /// </summary>
        public void Shutdown()
        {
            if (MainServer != null)
            {
                MainServer.Dispose();
                MainServer = null;
            }

            if (GameServer != null)
            {
                GameServer.Dispose();
                GameServer = null;
            }

            Debugger.Write("Communicator shutdown.");
        }

        /// <summary>
        /// Overrides the return value of ToString(). Returns the bot's username.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return AuthInfo.Username;
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Internal method for creating the master communicator. It also
        /// performs the login.
        /// </summary>
        private void CreateMasterCommunicator() 
        {
            MainServer = new MasterCommunicator(ServerAddress.Host, ServerAddress.Port);
            MainServer.LogFile = AuthInfo.Username + "MasterConfig.log";
            if (!MainServer.Connect())
            {
                HandleError("Could not connect to the server: reason unknown.");
            }

            MainServer.Login(AuthInfo.Username, AuthInfo.Password,
                new VTankObject.Version()); // TODO: Use real version.

            Debugger.Write("Login as {0} successful.", AuthInfo.Username);
        }

        /// <summary>
        /// Internal method for handling an error. This will escalate an exception
        /// with the given error message.
        /// </summary>
        /// <param name="errorMessage">Error message formatted for arguments.</param>
        /// <param name="args">Arguments if applicable.</param>
        private void HandleError(string errorMessage, params object[] args)
        {
            Debugger.Error(errorMessage, args);
            throw new Network.Exception.UnknownException(
                String.Format(errorMessage, args));
        }

        /// <summary>
        /// Repeatedly attempt to download a map.
        /// </summary>
        /// <param name="currentMapName"></param>
        private Map AttemptDownload(string currentMapName)
        {
            Map playableMap = null;
            bool done = false;
            int tries = 0;
            const int MaxTries = 3;
            while (!done)
            {
                playableMap = DownloadMap(currentMapName);
                playableMap.SaveMap();
                string hash = playableMap.SHA1Hash;

                if (!MainServer.GetProxy().HashIsValid(currentMapName, hash))
                {
                    if (tries++ >= MaxTries)
                    {
                        throw new Exception(String.Format(
                            "The download for the map file {0} keeps being corrupted.",
                            currentMapName));
                    }

                    Debugger.Write("Download of map {0} is corrupted -- {1} more {2}...",
                        currentMapName, MaxTries - tries, tries == 1 ? "try" : "tries");
                }
                else
                {
                    done = true;
                }
            }

            Debugger.Write("Downloaded and saved map {0}.", currentMapName);

            return playableMap;
        }

        /// <summary>
        /// Performs the actual map download operation.
        /// </summary>
        /// <param name="localPath">Local path in the operating system.</param>
        /// <param name="mapFileName"></param>
        /// <returns></returns>
        private Map DownloadMap(string mapFileName)
        {
            Debugger.Write("Downloading map {0}...", mapFileName);
            VTankObject.Map map = MainServer.GetProxy().DownloadMap(mapFileName);
            string title = map.title;
            int width = map.width;
            int height = map.height;
            VTankObject.Tile[] tiles = map.tileData;

            Map newMap = new Map(title, mapFileName, (uint)width, (uint)height);

            int size = width * height;
            for (int y = 0; y < height; ++y)
            {
                for (int x = 0; x < width; ++x)
                {
                    VTankObject.Tile relevantTile = tiles[y * width + x];
                    newMap.SetTile((uint)x, (uint)y, new Tile(
                        (uint)relevantTile.id, (ushort)relevantTile.objectId,
                        (ushort)relevantTile.eventId, relevantTile.passable,
                        (ushort)relevantTile.height, (ushort)relevantTile.type,
                        (ushort)relevantTile.effect));
                }
            }

            List<int> buf = new List<int>();
            for (int i = 0; i < map.supportedGameModes.Length; i++)
            {
                buf.Add(map.supportedGameModes[i]);
            }

            newMap.SetGameModes(buf);

            return newMap;
        }
        #endregion

        #region Invoke Methods
        public void InvokeUpdate()
        {
            if (!MainServer.Connected || !GameServer.Connected)
            {
                MainServer.Disconnect();
                GameServer.Disconnect();
                CreateMasterCommunicator();
                
                throw new Exception(Player.Name + " disconnected from main server.");
            }

            IEvent[] events = buffer.PopAll();
            for (int i = 0; i < events.Length; ++i)
            {
                events[i].DoAction();
                events[i].Dispose();
            }

            Game.Update();
            Update();
        }

        public void InvokePlayerJoined(PlayerJoinedEventArgs e)
        {
            Game.AddPlayer(e.Player, false);
            PlayerJoined(e);

            // Re-sync moves.
            GameServer.Move(Player.Position.x, Player.Position.y, Player.MovementDirection);
            GameServer.Rotate(Player.Angle, Player.RotationDirection);
        }

        public void InvokePlayerLeft(int playerID)
        {
            Player player = Game.GetPlayerByID(playerID);
            if (player != null)
            {
                Game.RemovePlayer(playerID);
                PlayerLeftEventArgs args = new PlayerLeftEventArgs(player);
                PlayerLeft(args);
            }
        }

        public void InvokeChatMessage(string message)
        {
            ChatMessage(message);
        }

        public void InvokeOnHitWall()
        {
            OnHitWall();
        }

        public void InvokeOnProjectileHit(
            int victimID, int projectileID, int damageInflicted, bool killingBlow)
        {
            Player victim = Game.GetPlayerByID(victimID);
            if (victim != null)
            {
                victim.InflictDamage(damageInflicted, killingBlow);

                ProjectileHitEventArgs args = new ProjectileHitEventArgs(
                    victim, damageInflicted, killingBlow);

                if (victim == Player && !victim.Alive)
                {
                    Game.StopMoving();
                    victim.MovementDirection = VTankObject.Direction.NONE;
                    victim.RotationDirection = VTankObject.Direction.NONE;
                }

                OnProjectileHit(args);

                args.Dispose();
            }
        }

        public void InvokeOnProjectileFired(int projectileID, int ownerID,
            int projectileTypeId, VTankObject.Point projectilePosition)
        {
            Player owner = Game.GetPlayerByID(ownerID);
            if (owner != null)
            {
                double angle = Math.Atan2(owner.Position.y - projectilePosition.y,
                    owner.Position.x - projectilePosition.x);

                Projectile projectile = new Projectile(
                    projectileID, owner.Weapon.ProjectileID, projectilePosition, angle);
                ProjectileFiredEventArgs args = new ProjectileFiredEventArgs(
                    owner, projectile);

                Game.AddProjectile(projectile);

                OnProjectileFired(args);

                args.Dispose();
            }
        }

        public void InvokeOnCompletedRotation()
        {
            OnCompletedRotation();
        }

        public void InvokeOnPlayerMove(int playerID, VTankObject.Point position,
            VTankObject.Direction direction)
        {
            Player player = Game.GetPlayerByID(playerID);
            if (player != null)
            {
                Game.SetPlayerMovement(player, position, direction);
            }
        }

        public void InvokeOnPlayerRotate(int playerID, double angle,
            VTankObject.Direction direction)
        {
            Player player = Game.GetPlayerByID(playerID);
            if (player != null)
            {
                Game.SetPlayerRotation(player, angle, direction);
            }
        }

        public void InvokePlayerHasRespawned(int playerID, VTankObject.Point position)
        {
            Player player = Game.GetPlayerByID(playerID);
            if (player != null)
            {
                player.Respawn(position);

                PlayerRespawnEventArgs args = new PlayerRespawnEventArgs(player);
                PlayerHasRespawned(args);
                args.Dispose();
            }
        }

        public void InvokeOnMapRotation()
        {
            buffer.PopAll();

            const int MAX_TRIES = 20;
            string mapName = null;
            for (int i = 0; i < MAX_TRIES; ++i)
            {
                mapName = GameServer.GetCurrentMapName();
                if (currentMapName != mapName)
                {
                    break;
                }

                Thread.Sleep(1000);
            }

            VTankObject.GameMode mode = GameServer.GetCurrentGameMode();

            MapRotationEventArgs args = new MapRotationEventArgs(mapName, mode);

            Game.Reset();
            if (Game.GameModeHandler != null)
                Game.GameModeHandler.Dispose();

            DownloadAndLoadMap();
            Game.GameModeHandler = CreateGameHandler(mode);

            RefreshPlayerList();
            
            OnMapRotation(args);

            GameServer.Ready();
        }

        private GameModeHandler CreateGameHandler(VTankObject.GameMode mode)
        {
            if (mode == VTankObject.GameMode.CAPTURETHEBASE)
                return new CaptureTheBaseMode(Game.CurrentMap);

            return null;
        }

        public void InvokeResetPosition(VTankObject.Point position)
        {
            Game.LocalPlayer.Position = position;
            OnPositionReset();
        }

        public void InvokeSpawnUtility(int utilityID, VTankObject.Utility util, VTankObject.Point pos)
        {

        }

        public void InvokeApplyUtility(int utilityId, VTankObject.Utility utility, int playerId)
        {

        }
        #endregion

        #region Virtual Methods
        public virtual void Update() { }
        public virtual void PlayerJoined(PlayerJoinedEventArgs e) { }
        public virtual void PlayerLeft(PlayerLeftEventArgs e) { }
        public virtual void ChatMessage(string message) { }
        public virtual void OnHitWall() { }
        public virtual void OnProjectileHit(ProjectileHitEventArgs e) { }
        public virtual void OnProjectileFired(ProjectileFiredEventArgs e) { }
        public virtual void OnCompletedRotation() { }
        public virtual void PlayerHasRespawned(PlayerRespawnEventArgs e) { }
        public virtual void OnPositionReset() { }
        public virtual void OnMapRotation(MapRotationEventArgs e) { }
        #endregion

        #region IDisposable Members
        public void Dispose()
        {
            Shutdown();

            if (buffer != null)
            {
                IEvent[] events = buffer.PopAll();
                if (events != null && events.Length > 0)
                {
                    foreach (IEvent evt in events)
                    {
                        evt.Dispose();
                    }
                }
            }

            BotRunner = null;
            clientCallback = null;
            clockCallback = null;
            buffer = null;
            Game = null;
            Player = null;
        }
        #endregion
    }
}
