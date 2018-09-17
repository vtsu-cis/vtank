using System;
using Network.Exception;
using Network.Util;
using System.Threading;
using IGame;
using Network.Game.AMI;
using GameSession;
using Network.Main.AMI;

namespace Network.Game
{
    /// <summary>
    /// The GameCommunicator handles all communication between the game server and the client.
    /// It abstracts difficult things like creating the connection, keeping the connection
    /// alive, and forming special game requests.
    /// </summary>
    public class GameCommunicator : IDisposable
    {
        #region Members
        private Ice.Communicator communicator;
        private Glacier2.RouterPrx router;
        private IGame.AuthPrx auth;
        private GameSession.GameInfoPrx session;
        private GameSession.GameInfoPrx sessionOneway;
        private Logger logger;
        private bool keepAlive;
        private Timer keepAliveTimer;
        private Thread timerThread;
        private long keepAliveInterval;
        private bool running;
        private bool connected;
        private bool needsRefresh = true;
        private Pinger pinger;
        private string category;
        private Ice.ObjectAdapter adapter;
        private Ice.Identity clockIdentity;
        private Ice.Identity clientIdentity;
        private long lastTimeStamp;

        // In-game variables.
        private double timeLeft;
        private string currentMapName;
        private VTankObject.GameMode currentGameMode;
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the stored host name which the communicator uses to connect to the
        /// main server.
        /// </summary>
        public string Host
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the stored port number which the communicator uses to connect to the
        /// main server.
        /// </summary>
        public int Port
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the value which determines if the communicator attempts to connect to
        /// a Glacier2 router or not.
        /// </summary>
        public bool UseGlacier2
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets how long to wait before the communicator will consider a connection
        /// lost.
        /// </summary>
        public long Timeout
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets whether or not the channel is continously calling KeepAlive with
        /// the server in order to prevent disconnection.
        /// </summary>
        public bool KeepAlive
        {
            get
            {
                lock (this)
                {
                    return keepAlive;
                }
            }

            set
            {
                lock (this)
                {
                    keepAlive = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the interval at which the client polls the server.
        /// </summary>
        public long KeepAliveInterval
        {
            get
            {
                lock (this)
                {
                    return keepAliveInterval;
                }
            }

            set
            {
                lock (this)
                {
                    if (value <= 1000)
                    {
                        throw new InvalidValueException(
                            "Cannot set the value of KeepAliveInterval to less than 1 second.");
                    }

                    keepAliveInterval = value;
                }
            }
        }

        /// <summary>
        /// Gets whether or not this communicator is still running.
        /// </summary>
        public bool Running
        {
            get
            {
                lock (this)
                {
                    return running;
                }
            }

            private set
            {
                lock (this)
                {
                    running = value;
                }
            }
        }

        /// <summary>
        /// Gets how many seconds are left in this game. When the timer hits zero, the map
        /// will rotate and a new timer will begin. Therefore, this method is not a reliable
        /// means to keep track of time -- the client needs to be prepared for the case that
        /// the time left suddenly jumps from 0:00 to x:xx. The client should keep track of
        /// whether or not he has already rotated.
        /// </summary>
        public double TimeLeftSeconds
        {
            get
            {
                lock (this)
                {
                    return timeLeft;
                }
            }

            private set
            {
                lock (this)
                {
                    timeLeft = value;
                }
            }
        }

        /// <summary>
        /// Internal thread-safe method for detecting whether or not in-game information needs
        /// a refresh.
        /// </summary>
        private bool NeedsRefresh
        {
            get
            {
                lock (this)
                {
                    return needsRefresh;
                }
            }

            set
            {
                lock (this)
                {
                    needsRefresh = value;
                }
            }
        }

        /// <summary>
        /// Gets whether this communicator is connected to the main server or not.
        /// </summary>
        public bool Connected
        {
            get
            {
                lock (this)
                {
                    return connected;
                }
            }

            private set
            {
                lock (this)
                {
                    connected = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the log file for this communicator.
        /// </summary>
        public string LogFile
        {
            get;
            set;
        }
        #endregion

        #region Constructors
        /// <summary>
        /// The easiest constructor assumes that the target host/port uses a Glacier2 router,
        /// that the user wishes to use a secure connection, and that it uses a default timeout
        /// of 10 seconds.
        /// </summary>
        /// <param name="_host">Host name of the target router.</param>
        /// <param name="_port">Port number of the target router. By default, Glacier2
        /// uses ports 4063 (unsecure) and 4064 (secure).</param>
        public GameCommunicator(string _host, int _port)
            : this(_host, _port, true)
        {
        }

        /// <summary>
        /// This constructor gives greater control over how the client will connect to the 
        /// main server. It assigns the 'timeout' value to it's default, 10000 milliseconds.
        /// </summary>
        /// <param name="_host">Host name of the target router or host.</param>
        /// <param name="_port">Port number of the target router or host. If the target is a
        /// router: Glacier2 routers use ports 4063 (unsecure) and 4064 (secure).</param>
        /// <param name="_useGlacier2">True if the target host/port uses a Glacier2 router.
        /// </param>
        /// <param name="_secure">True if the client should use a secure connection. Note that
        /// if _useGlacier2 is false, this attribute will do nothing.</param>
        public GameCommunicator(string _host, int _port, bool _useGlacier2)
            : this(_host, _port, _useGlacier2, 10000)
        {
        }

        /// <summary>
        /// This constructor provides maximum control over how the client will connect to
        /// the main server.
        /// </summary>
        /// <param name="_host">Host name of the target router or host.</param>
        /// <param name="_port">Port number of the target router or host. If the target is a
        /// router: Glacier2 routers use ports 4063 (unsecure) and 4064 (secure).</param>
        /// <param name="_useGlacier2">True if the target host/port uses a Glacier2 router.
        /// </param>
        /// <param name="_timeout">How long (millisecond value) to wait until disconnecting
        /// from the server if the server is not replying to a message.</param>
        public GameCommunicator(
            string _host, int _port, bool _useGlacier2, long _timeout)
        {
            Host        = _host;
            Port        = _port;
            UseGlacier2 = _useGlacier2;
            Timeout     = _timeout;

            Initialize();
        }
        #endregion

        #region Destructor
        /// <summary>
        /// The destructor will attempt to clean up any unmanaged resources left behind.
        /// </summary>
        ~GameCommunicator()
        {
            try
            {
                Dispose();
                logger.Close();
            }
            catch (System.Exception) { }
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Perform basic initializations required for this class.
        /// </summary>
        private void Initialize()
        {
            KeepAlive = true;
            KeepAliveInterval = 10000;
        }

        /// <summary>
        /// Create the communicator. Does not re-create the communicator if it's already
        /// initialized.
        /// </summary>
        private void CreateCommunicator()
        {
            if (communicator == null)
            {
                string[] args = new string[] {
                    "--Ice.ThreadPool.Server.Size=1",
                    "--Ice.ThreadPool.Server.SizeMax=2",
                    "--Ice.ThreadPool.Server.SizeWarn=0",
                    "--Ice.ThreadPool.Client.Size=1",
                    "--Ice.ThreadPool.Client.SizeMax=2",
                    "--Ice.ThreadPool.Client.SizeWarn=0"
                };

                communicator = Ice.Util.initialize(ref args);
            }
        }

        /// <summary>
        /// Destroy the communicator. Does nothing if the communicator was not created.
        /// </summary>
        private void DestroyCommunicator()
        {
            if (communicator != null)
            {
                try
                {
                    communicator.shutdown();
                }
                catch (System.Exception) { }
                finally
                {
                    communicator = null;
                }
            }
        }

        /// <summary>
        /// Create a configured adapter.
        /// </summary>
        private void CreateAdapter()
        {
            if (UseGlacier2)
            {
                adapter = communicator.createObjectAdapterWithRouter(
                    "Callback", router);
            }
            else
            {
                adapter = communicator.createObjectAdapterWithEndpoints(
                    "Callback", "tcp -p 31334");
            }
        }

        /// <summary>
        /// Destroy the adapter. If the adapter has not been created, nothing happens.
        /// </summary>
        private void DestroyAdapter()
        {
            if (adapter != null)
            {
                try
                {
                    adapter.removeAllFacets(clientIdentity);
                    adapter.removeAllFacets(clockIdentity);
                    adapter.destroy();
                }
                catch (System.Exception e)
                {
                    logger.Warning("DestroyAdapter() Unexpected System.Exception: {0}", e);
                }

                adapter = null;
            }
        }

        /// <summary>
        /// Create a proxy pointing to the session factory, which gives the program access to
        /// login methods. If Glacier2 is enabled, it goes through a Glacier2 router.
        /// </summary>
        /// <returns>A proxy pointing to the Auth object adapter on the target server.</returns>
        private AuthPrx CreateAuthProxy()
        {
            AuthPrx authProxy = null;
            if (UseGlacier2)
            {
                Ice.ObjectPrx routerProxy = communicator.stringToProxy(String.Format(
                    "Theatre/router:{0} -h {1} -p {2} -t {3}", "tcp", Host, Port, Timeout));

                router = Glacier2.RouterPrxHelper.checkedCast(routerProxy);
                if (router == null)
                {
                    // Not a valid Glacier2 router.
                    throw new InvalidHostException(
                        "The target host does not use a Glacier2 router.");
                }

                Glacier2.SessionPrx sessionProxy = router.createSession("GameCommunicator", "");
                authProxy = AuthPrxHelper.uncheckedCast(sessionProxy.ice_router(router));
            }
            else
            {
                authProxy = AuthPrxHelper.uncheckedCast(
                    communicator.stringToProxy(String.Format(
                        "GameSessionFactory:{0} -h {1} -p {2} -t {3}", 
                        "tcp", Host, Port, Timeout)));
            }

            // Pinging once will initiate the connection and cause an exception to be thrown if
            // the target host is invalid. This is only useful for non-router connections.
            authProxy.ice_ping();

            logger.Info("AuthProxy created successfully.");

            return authProxy;
        }

        /// <summary>
        /// This method acts as the return point for asynchronous callbacks. When a callback
        /// fails, this method sets the state of this communicator to unconnected.
        /// </summary>
        /// <param name="result">Result of the callback.</param>
        /// <param name="ex">Exception thrown by the callback, if any.</param>
        private void CommonExceptionHandler<T>(Result<T> result)
        {
            if (!result.Success)
            {
                try
                {
                    logger.Error(
                        "Suddenly lost connection to the server: {0}", result.Exception);

                    Disconnect();
                }
                catch (System.Exception e)
                {
                    logger.Error(
                        "CommonExceptionHandler() Unexpected System.Exception: {0}", e);
                }
            }
        }

        /// <summary>
        /// This method acts as the callback for the timer. When called, if KeepAlive is
        /// enabled and Connected is true, this method attempts to call KeepAlive on 
        /// the main server.
        /// </summary>
        /// <param name="o">Unused.</param>
        private void OnKeepAlive(object o)
        {
            if (KeepAlive && Connected)
            {
                lock (this)
                {
                    session.KeepAlive_async(new GameKeepAlive_Callback(
                        CommonExceptionHandler<NoReturnValue>));
                }
            }
        }

        /// <summary>
        /// This method acts as a thread which occasionally increments the time left in a game.
        /// </summary>
        private void TimeTicker()
        {
            try
            {
                lastTimeStamp = Network.Util.Clock.GetTimeMilliseconds();
                while (Connected)
                {
                    long currentStamp = Network.Util.Clock.GetTimeMilliseconds();
                    lock (this)
                    {
                        if (timeLeft > 0)
                        {
                            double delta = (double)(currentStamp - lastTimeStamp) / 1000.0;
                            timeLeft -= delta;

                            if (timeLeft <= 0)
                            {
                                timeLeft = 0;
                                NeedsRefresh = true;
                            }
                        }
                    }
                    lastTimeStamp = currentStamp;

                    Thread.Sleep(250);
                }
            }
            catch (ThreadInterruptedException e) { Console.WriteLine(e); }
        }

        /// <summary>
        /// Refresh the in-game data. This is typically called when new information is needed
        /// due to a map rotation.
        /// </summary>
        private void Refresh()
        {
            lock (this)
            {
                timeLeft = session.GetTimeLeft();
                currentMapName = session.GetCurrentMapName();
                currentGameMode = session.GetGameMode();

                NeedsRefresh = false;
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Connect to the game server using the stored properties.
        /// </summary>
        /// <returns>True if the connection was successful; false otherwise.</returns>
        public bool Connect()
        {
            if (logger == null)
            {
                if (LogFile == null)
                {
                    LogFile = "GameCommunicator.log";
                }

                logger = new Logger(LogFile);
            }

            if (Connected)
            {
                // Already connected, so sever the connection and re-create it.
                Disconnect();
            }

            bool result = false;
            lock (this)
            {
                try
                {
                    CreateCommunicator();
                    auth = CreateAuthProxy();

                    CreateAdapter();

                    category = router.getCategoryForClient();

                    Connected = true;

                    result = true;
                }
                catch (Ice.Exception e)
                {
                    logger.Error("Connect() Ice.Exception: {0}", e);
                }
                catch (System.Exception e)
                {
                    logger.Error("Connect() System.Exception: {0}", e);
                }
            }

            return result;
        }

        /// <summary>
        /// Register a callback to the game's adapter, which is used to receive messages
        /// from the server.
        /// </summary>
        /// <param name="callback">Object to register.</param>
        public void RegisterCallback(Ice.Object callback)
        {
            try
            {
                lock (this)
                {
                    string identity = null;
                    if (callback is ClockSynchronizer)
                    {
                        identity = "ClockSync";
                        clockIdentity = new Ice.Identity(identity, category);

                        adapter.add(callback, clockIdentity);
                    }
                    else if (callback is ClientEventCallback)
                    {
                        identity = "ClientEventCallback";
                        clientIdentity = new Ice.Identity(identity, category);

                        adapter.add(callback, clientIdentity);
                    }
                    else
                    {
                        throw new InvalidValueException(
                            "That type of callback object is not supported.");
                    }
                }
            }
            catch (Ice.AlreadyRegisteredException e)
            {
                // The object is already in the adapter. In this case, we'll ignore the error.
                logger.Warning("RegisterCallback() Ice.AlreadyRegisteredException: {0}", e);
            }
            catch (Ice.Exception e)
            {
                logger.Error("RegisterCallback() Ice.Exception: {0}", e);
                throw new UnknownException(
                    "An unknown error occurred in the game's communicator.");
            }
            catch (System.Exception e)
            {
                logger.Error("RegisterCallback() System.Exception: {0}", e);
                throw new UnknownException(
                    "An unknown error occurred in the game's communicator.");
            }
        }

        /// <summary>
        /// Clear all callbacks registered to this communicator.
        /// </summary>
        public void ClearRegisteredCallbacks()
        {
            if (clockIdentity != null)
            {
                try
                {
                    adapter.remove(clockIdentity);
                }
                catch (Ice.Exception e)
                {
                    logger.Warning("ClearRegisteredCallbacks() Ice.Exception: {0}", e);
                }

                clockIdentity = null;
            }

            if (clientIdentity != null)
            {
                try
                {
                    adapter.remove(clientIdentity);
                }
                catch (Ice.Exception e)
                {
                    logger.Warning("ClearRegisteredCallbacks() Ice.Exception: {0}", e);
                }

                clientIdentity = null;
            }
        }

        /// <summary>
        /// Join the game server. Once active, this officially joins the game.
        /// </summary>
        /// <param name="key">Key given by the main communicator after calling
        /// RequestJoinGameServer().</param>
        /// <returns>True if successful; false otherwise.</returns>
        public bool JoinServer(string key)
        {
            bool result = false;
            try
            {
                lock (this)
                {
                    adapter.activate();

                    ClientEventCallbackPrx clientCallback = ClientEventCallbackPrxHelper.
                        uncheckedCast(adapter.createProxy(clientIdentity));
                    ClockSynchronizerPrx clockCallback = ClockSynchronizerPrxHelper.
                        uncheckedCast(adapter.createProxy(clockIdentity));
                    session = auth.JoinServer(key, clockCallback, clientCallback);
                    // Configure the proxy.
                    session = GameInfoPrxHelper.uncheckedCast(
                        session.ice_router(router));
                    sessionOneway = GameInfoPrxHelper.uncheckedCast(
                        session.ice_oneway());

                    Refresh();

                    timerThread = new Thread(new ThreadStart(TimeTicker));
                    timerThread.Start();

                    if (KeepAlive)
                    {
                        keepAliveTimer = new Timer(new TimerCallback(OnKeepAlive),
                            null, KeepAliveInterval, KeepAliveInterval);
                    }

                    pinger = new Pinger(session);
                }

                result = true;
            }
            catch (Ice.IllegalIdentityException e)
            {
                logger.Error("JoinServer() No callbacks registered: {0}", e);
                throw new InvalidValueException("You must register callbacks first.");
            }
            catch (Exceptions.PermissionDeniedException e)
            {
                logger.Info("JoinServer() Failed (permission denied): {0}", e.reason);
            }
            catch (Ice.Exception e)
            {
                logger.Error("JoinServer() Ice.Exception: {0}", e);
            }
            catch (System.Exception e)
            {
                logger.Error("JoinServer() System.Exception: {0}", e);
            }

            return result;
        }

        /// <summary>
        /// Force the communicator to sever it's connection.
        /// </summary>
        public void Disconnect()
        {
            lock (this)
            {
                try
                {
                    Connected = false;

                    if (pinger != null)
                    {
                        pinger.Running = false;
                    }

                    if (keepAliveTimer != null)
                    {
                        keepAliveTimer.Dispose();
                    }

                    if (timerThread != null)
                    {
                        timerThread.Interrupt();
                    }

                    if (session != null)
                    {
                        session.destroy_async(new Destroy_Callback());
                    }

                    if (router != null)
                    {
                        router.destroySession_async(new GameRouterDestroySession_Callback());
                    }

                    if (logger != null)
                    {
                        logger.Close();
                    }

                    DestroyAdapter();
                    DestroyCommunicator();
                }
                catch (System.Exception e)
                {
                    logger.Error(
                        "Unexpected error in the Disconnect() method: {0}", e);
                }
                finally
                {
                    pinger = null;
                    keepAliveTimer = null;
                    session = null;
                    sessionOneway = null;
                    router = null;
                    auth = null;
                    timerThread = null;
                    logger = null;
                }
            }
        }

        /// <summary>
        /// Get the average latency to the main server.
        /// </summary>
        /// <returns>Latency in seconds.</returns>
        public double GetAverageLatency()
        {
            return pinger.AverageLatency;
        }

        /// <summary>
        /// Get the average latency formatted in a way that's pleasing to the eye.
        /// </summary>
        /// <returns>Formatted average latency like: "0 ms"</returns>
        public string GetFormattedAverageLatency()
        {
            return pinger.GetFormattedAverageLatency();
        }

        /// <summary>
        /// Get the filename of the current map. This value is cached until it needs to be
        /// refreshed again, so repeated calls are allowed (though not recommended).
        /// </summary>
        /// <returns>String filename of the map that should be loaded.</returns>
        public string GetCurrentMapName()
        {
            if (NeedsRefresh)
            {
                Refresh();
            }

            return currentMapName;
        }

        /// <summary>
        /// Get the current game mode being played. This value is cached until it needs to be
        /// refreshed again, so repeated calls are allowed (though not recommended).
        /// </summary>
        /// <returns>The current game mode.</returns>
        public VTankObject.GameMode GetCurrentGameMode()
        {
            if (NeedsRefresh)
            {
                Refresh();
            }

            return currentGameMode;
        }

        /// <summary>
        /// Get the current player list. This value is NOT cached! That means that every call
        /// contacts the game server to get the player list.
        /// </summary>
        /// <returns>Array list of players and their in-game information.</returns>
        public GameSession.Tank[] GetPlayerList()
        {
            return session.GetPlayerList();
        }

        /// <summary>
        /// Get the list of statistics from the server; that is, how well everyone is doing
        /// in-game.
        /// </summary>
        /// <returns>Array of statistics VTank objects.</returns>
        public VTankObject.Statistics[] GetScoreboard()
        {
            return session.GetScoreboard();
        }

        /// <summary>
        /// Gets a list of data on the current team game from the server.
        /// </summary>
        /// <returns>Array of ScoreboardTotals</returns>
        public GameSession.ScoreboardTotals GetTeamStats()
        {
            return session.GetTeamTotals();
        }

        /// <summary>
        /// Send a movement packet to the game server. You must already be connected and
        /// authenticated with Theater for this to do anything.
        /// </summary>
        /// <param name="x">X-position of the player (for sync).</param>
        /// <param name="y">Y-position of the player (for sync).</param>
        /// <param name="direction">New direction to move towards. The only valid directions
        /// are: NONE, FORWARD, REVERSE.</param>
        /// <exception cref="Network.Exception.InvalidValueException">
        /// Thrown if an invalid direction is passed.</exception>
        /// <exception cref="System.NullPointerException">Thrown if the
        /// session was never connected.</exception>
        public void Move(double x, double y, VTankObject.Direction direction)
        {
            if (direction == VTankObject.Direction.LEFT || 
                direction == VTankObject.Direction.RIGHT)
            {
                // Direction is invalid.
                throw new InvalidValueException(direction.ToString() + 
                    " is an invalid value for movement!");
            }

            sessionOneway.Move_async(
                new Move_Callback(new ActionFinished<NoReturnValue>(
                    CommonExceptionHandler<NoReturnValue>)),
                Network.Util.Clock.GetTimeMilliseconds(), new VTankObject.Point(x, y),
                direction);
        }

        /// <summary>
        /// Send a rotation packet to the game server. You must already be connected and
        /// authenticated with Theater for this to do anything.
        /// </summary>
        /// <param name="angle">Current angle (for sync).</param>
        /// <param name="direction">New rotation direction.</param>
        /// <exception cref="Network.Exception.InvalidValueException">
        /// Thrown if an invalid direction is passed.</exception>
        /// <exception cref="System.NullPointerException">Thrown if the
        /// session was never connected.</exception>
        public void Rotate(double angle, VTankObject.Direction direction)
        {
            if (direction == VTankObject.Direction.FORWARD ||
                direction == VTankObject.Direction.REVERSE)
            {
                // Direction is invalid.
                throw new InvalidValueException(direction.ToString() +
                    " is an invalid value for rotation!");
            }

            sessionOneway.Rotate_async(new Rotate_Callback(
                new ActionFinished<NoReturnValue>(
                    CommonExceptionHandler<NoReturnValue>)),
                Network.Util.Clock.GetTimeMilliseconds(), angle, direction);
        }

        /// <summary>
        /// Send a fire packet to the game server. You must already be connected and
        /// authenticated with Theater for this to do anything.
        /// </summary>
        /// <param name="xTarget">The 'x' coordinate of the target to fire at.</param>
        /// <param name="yTarget">The 'y' coordinate of the target to fire at.</param>
        /// <exception cref="System.NullPointerException">Thrown if the
        /// session was never connected.</exception>
        public void Fire(double xTarget, double yTarget)
        {
            sessionOneway.Fire_async(new Fire_Callback(
                new ActionFinished<NoReturnValue>(
                    CommonExceptionHandler<NoReturnValue>)),
                Network.Util.Clock.GetTimeMilliseconds(), 
                new VTankObject.Point(xTarget, yTarget));
        }

        /// <summary>
        /// Tell the game server you want to start charging your weapon.
        /// </summary>
        public void StartCharging()
        {
            sessionOneway.StartCharging();
        }

        /// <summary>
        /// Send a chat message to the server. This chat message (unless a command) is
        /// sent to every other player in the game.
        /// </summary>
        /// <param name="message">Message to send.</param>
        /// <exception cref="System.NullReferenceException">Thrown if the session hasn't
        /// been created yet.</exception>
        public void SendChatMessage(string message)
        {
            sessionOneway.SendMessage_async(
                new Chat_Callback(new ActionFinished<NoReturnValue>(
                    CommonExceptionHandler<NoReturnValue>)),
                message);
        }

        /// <summary>
        /// Tell the game server we are ready. This is not asychronous.
        /// TODO: Make this asynchronous.
        /// </summary>
        public void Ready()
        {
            sessionOneway.Ready();
        }
        #endregion

        #region IDisposable Members

        /// <summary>
        /// Dispose of the unmanaged resources used by this communicator. To re-create the
        /// resources used by this communicator, call Connect().
        /// </summary>
        public void Dispose()
        {
            try
            {
                Disconnect();
                DestroyCommunicator();
            }
            catch (System.Exception e)
            {
                logger.Error("Dispose() Unexpected error: {0}", e);
            }
        }

        #endregion
    }
}
