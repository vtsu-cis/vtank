using Main;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using Network.Util;
using Network.Main.AMI;
using System.Threading;
using Network.Exception;
using Exceptions;
using System;

namespace Network.Main
{
    /// <summary>
    /// The master communicator initializes and manages network communication with Echelon,
    /// the master server of the VTank universe. The communicator handles all communications
    /// between the client and server, including setting up a connection through a Glacier2
    /// router automatically, when wanted.
    /// </summary>
    public class MasterCommunicator : IDisposable
    {
        #region Members
        private Ice.Communicator communicator;
        private Glacier2.RouterPrx router;
        private MainSessionPrx session;
        private SessionFactoryPrx factory;
        private Logger logger;
        private bool keepAlive;
        private Timer keepAliveTimer;
        private long keepAliveInterval;
        private bool running;
        private bool connected;
        private Pinger pinger;
        private VTankObject.TankAttributes[] tankList;
        private bool tankListNeedsRefresh;
      //  private VTankObject.Weapon[] weaponList;
        private ActionFinished remoteCallback;
        public delegate void RankCallback(ActionFinished<int> rank);
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
        /// Gets or sets the value which determines if the communicator attempts to connect
        /// through a secure connection or not. This value does nothing if UseGlacier2 is
        /// not true.
        /// </summary>
        public bool Secure
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
                    if (value <= 5000)
                    {
                        throw new InvalidValueException(
                            "Cannot set the value of KeepAliveInterval to less than 5 seconds.");
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
        /// Get or set the config file. The config file is used to create the communicator.
        /// Do not set this property unless you know what you're doing.
        /// </summary>
        public string ConfigFile
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the log filename used for logging. This is only useful to set
        /// before a connection is created.
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
        public MasterCommunicator(string _host, int _port)
            : this(_host, _port, true)
        {
        }

        /// <summary>
        /// The preferred constructor will assign the 'secure' attribute to true. Note that
        /// if _useGlacier2 is assigned to false, the 'secure' attribute does nothing.
        /// </summary>
        /// <param name="_host">Host name of the target router or host.</param>
        /// <param name="_port">Port number of the target router or host. If the target is a
        /// router: Glacier2 routers use ports 4063 (unsecure) and 4064 (secure).</param>
        /// <param name="_useGlacier2">True if the target host/port uses a Glacier2 router.
        /// </param>
        public MasterCommunicator(string _host, int _port, bool _useGlacier2)
            : this(_host, _port, _useGlacier2, true)
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
        public MasterCommunicator(string _host, int _port, bool _useGlacier2, bool _secure)
            : this(_host, _port, _useGlacier2, _secure, 10000)
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
        /// <param name="_secure">True if the client should use a secure connection. Note that
        /// if _useGlacier2 is false, this attribute will do nothing.</param>
        /// <param name="_timeout">How long (millisecond value) to wait until disconnecting
        /// from the server if the server is not replying to a message.</param>
        public MasterCommunicator(
            string _host, int _port, bool _useGlacier2, bool _secure, long _timeout)
        {
            Host        = _host;
            Port        = _port;
            UseGlacier2 = _useGlacier2;
            Secure      = _secure;
            Timeout     = _timeout;

            Initialize();
        }
        #endregion

        #region Destructor
        /// <summary>
        /// The destructor will attempt to clean up any unmanaged resources left behind.
        /// </summary>
        ~MasterCommunicator()
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
            KeepAliveInterval = 15000;
            Running = true;
            tankListNeedsRefresh = true;
        }

        /// <summary>
        /// Create the communicator. Does not re-create the communicator if it's already
        /// initialized.
        /// </summary>
        private void CreateCommunicator()
        {
            if (communicator == null)
            {
                if (!String.IsNullOrEmpty(ConfigFile))
                {
                    String[] args = new String[] {
                        String.Format("--Ice.Config={0}", ConfigFile),
                        "--Ice.ThreadPool.Client.Size=1",
                        "--Ice.ThreadPool.Client.SizeMax=2",
                        "--Ice.ThreadPool.Client.SizeWarn=0"
                    };

                    communicator = Ice.Util.initialize(ref args);
                }
                else
                {
                    String[] args = null;
                    if (UseGlacier2)
                    {
                        args = new String[] {
                            String.Format(
                                "--Ice.Default.Router=VTank/router:{0} -h {1} -p {2} -t {3}",
                                (Secure ? "tcp" : "tcp"), // TODO: We'll eventually use "ssl" for secure connections.
                                Host, Port, Timeout),
                            "--Ice.ThreadPool.Client.Size=1",
                            "--Ice.ThreadPool.Client.SizeMax=2",
                            "--Ice.ThreadPool.Client.SizeWarn=0"
                        };

                        communicator = Ice.Util.initialize(ref args);
                    }
                    else
                    {
                        args = new String[] {
                            "--Ice.ThreadPool.Client.Size=1",
                            "--Ice.ThreadPool.Client.SizeMax=2",
                            "--Ice.ThreadPool.Client.SizeWarn=0"
                        };

                        communicator = Ice.Util.initialize(ref args);
                    }
                }
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

                communicator = null;
            }

            Running = false;
        }

        /// <summary>
        /// Create a proxy pointing to the session factory, which gives the program access to
        /// login methods. If Glacier2 is enabled, it goes through a Glacier2 router.
        /// </summary>
        /// <returns></returns>
        private SessionFactoryPrx CreateSessionFactoryProxy()
        {
            SessionFactoryPrx sessionFactoryProxy = null;
            if (UseGlacier2)
            {
                /*Ice.ObjectPrx routerProxy = communicator.stringToProxy(String.Format(
                    "VTank/router:{0} -h {1} -p {2} -t {3}",
                        (Secure ? "tcp" : "tcp"), // TODO: We'll eventually use "ssl" for secure connections.
                        Host, Port, Timeout));*/
                Ice.ObjectPrx routerProxy = communicator.getDefaultRouter();

                router = Glacier2.RouterPrxHelper.checkedCast(routerProxy);
                if (router == null)
                {
                    // Not a valid Glacier2 router.
                    throw new InvalidHostException(
                        "The target host does not use a Glacier2 router.");
                }

                Glacier2.SessionPrx sessionProxy = router.createSession("MasterCommunicator", "");
                sessionFactoryProxy = SessionFactoryPrxHelper.uncheckedCast(sessionProxy);
            }
            else
            {
                communicator.setDefaultRouter(null);
                sessionFactoryProxy = SessionFactoryPrxHelper.uncheckedCast(
                    communicator.stringToProxy(String.Format(
                    "SessionFactory:{0} -h {1} -p {2} -t {3}",
                        (Secure ? "tcp" : "tcp"), // TODO: We'll eventually use "ssl" for secure connections.
                        Host, Port, Timeout)));
            }

            // Pinging once will initiate the connection and cause an exception to be thrown if
            // the target host is invalid. This is only useful for non-router connections.
            sessionFactoryProxy.ice_ping();

            logger.Info("SessionFactoryProxy created successfully.");

            return sessionFactoryProxy;
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
                    ActionFinished<NoReturnValue> callback = CommonExceptionHandler<NoReturnValue>;
                    session.KeepAlive_async(new KeepAlive_Callback(callback));
                }
            }
        }

        /// <summary>
        /// Intercepts the login attempt in order to provide some internal handling.
        /// </summary>
        /// <param name="result">Result of the login attempt.</param>
        /// <param name="session">Session proxy, if one exists.</param>
        /// <param name="e">Exception thrown from the callback.</param>
        private void LoginCallback(Result<MainSessionPrx> result)
        {
            System.Exception e = result.Exception;
            if (result.Success)
            {
                if (KeepAlive)
                {
                    keepAliveTimer = new Timer(new TimerCallback(OnKeepAlive),
                        null, KeepAliveInterval, KeepAliveInterval);
                }

                session = result.ReturnedResult;
                session.GetTankList();

                //session = MainSessionPrxHelper.uncheckedCast(session.ice_router(router));
                pinger = new Pinger(session);
            }
            else
            {
                logger.Error("LoginCallback() Login attempt failed: {0}", e);
                if (e is PermissionDeniedException)
                {
                    PermissionDeniedException ex = (PermissionDeniedException)e;
                    e = new LoginFailedException(ex.reason);
                }
                else
                {
                    e = new LoginFailedException("Unable to login.");
                }

                result.Exception = e;
            }

            remoteCallback(new Result(result.Success, session, result.Exception));
            remoteCallback = null;
        }

        /// <summary>
        /// Refresh the cached list of tanks.
        /// </summary>
        private void RefreshTankList()
        {
            if (tankListNeedsRefresh)
            {
                tankList = session.GetTankList();
                tankListNeedsRefresh = false;
            }
        }
        
        /// <summary>
        /// Check a tank first before asking the server to accept it.
        /// Note that this method will throw an exception if it fails the test.
        /// </summary>
        /// <param name="tank">Tank to test.</param>
        private void CheckTank(VTankObject.TankAttributes tank)
        {
            if (string.IsNullOrEmpty(tank.name))
            {
                throw new InvalidValueException("Tank names cannot be empty.");
            }

            Regex pattern = new Regex("[^A-Za-z0-9]");
            if (pattern.IsMatch(tank.name))
            {
                throw new InvalidValueException(
                    "Tank names must contain only letters and numbers.");
            }

            if (tank.name.Contains(" "))
            {
                throw new InvalidValueException(
                    "Tank names are not allowed to have spaces.");
            }

            if (tank.speedFactor < 0.5f || tank.speedFactor > 1.5f || 
                tank.armorFactor < 0.5f || tank.armorFactor > 1.5f)
            {
                throw new InvalidValueException(
                    "The speed factor and armor factor must be between 50% and 150%.");
            }

            float difference_speed = (float)Math.Abs(1.0f - Math.Round(tank.speedFactor, 2));
            float difference_armor = (float)Math.Abs(1.0f - Math.Round(tank.armorFactor, 2));
            if (difference_speed - difference_armor > 0.001f)
            {
                throw new InvalidValueException(
                    "The speed factor and armor factor values must have exactly " +
                    "the same distance from 100%.");
            }

            if (String.IsNullOrEmpty(tank.model))
            {
                throw new InvalidValueException(
                    "You must select a tank model to use.");
            }

            if (tank.color.green < 0 || tank.color.green > 255 || tank.color.red < 0 || tank.color.red > 255 ||
                tank.color.blue < 0 || tank.color.blue > 255)
            {
                throw new InvalidValueException(
                    "The RGB value of the tank must be between 0 and 255.");
            }

            if (tank.weaponID < 0)
            {
                throw new InvalidValueException(
                    "The weapon ID must not be negative.");
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Connect to the main server using the stored properties.
        /// </summary>
        /// <returns>True if the connection was successful; false otherwise.</returns>
        public bool Connect()
        {
            if (Connected)
            {
                // Already connected -- so sever the connection.
                Disconnect();
            }

            if (logger == null)
            {
                if (LogFile == null)
                {
                    LogFile = "MasterCommunicator.log";
                }

                logger = new Logger(LogFile);
            }

            bool result = false;
            lock (this)
            {
                try
                {
                    logger.Info("Connect() Attempting to connect...");

                    CreateCommunicator();
                    factory = CreateSessionFactoryProxy();

                    Connected = true;
                    Running = true;

                    logger.Info("Connect() Connection attempt successful.");

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
        /// Log into the server synchronously, which means it will block until it receives
        /// a response.
        /// </summary>
        /// <param name="user">Username to login with.</param>
        /// <param name="password">Password for the user.</param>
        /// <param name="version">Version of the client.</param>
        /// <param name="callback">Callback, called when the login finishes.</param>
        /// <exception cref="Network.Exceptions.LoginFailedException">
        /// Thrown if the login failed.</exception>
        public void Login(string user, string password, VTankObject.Version version)
        {
            try
            {
                session = MainSessionPrxHelper.uncheckedCast(
                    factory.VTankLogin(user, password, version));

                pinger = new Pinger(session);

                if (KeepAlive)
                {
                    keepAliveTimer = new Timer(new TimerCallback(OnKeepAlive),
                        null, KeepAliveInterval, KeepAliveInterval);
                }
            }
            catch (Exceptions.PermissionDeniedException e)
            {
                logger.Info("Login() PermissionDeniedException: {0}", e.reason);
                throw new LoginFailedException(e.reason);
            }
            catch (Ice.Exception e)
            {
                logger.Error("Login() Ice.Exception: {0}", e);
                throw new UnknownException("Cannot connect to the server.");
            }
            catch (System.Exception e)
            {
                logger.Error("Login() System.Exception: {0}", e);
                throw new UnknownException("Cannot connect to the server.");
            }
        }

        /// <summary>
        /// Asynchronously log into the server -- meaning, this function returns immediately
        /// while it attempts to login to the server. When it finishes, it calls the 
        /// 'callback' function handed in.
        /// </summary>
        /// <param name="user">Username to login with.</param>
        /// <param name="password">Password for the user.</param>
        /// <param name="version">Version of the client.</param>
        /// <param name="callback">Callback, called when the login finishes.</param>
        /// <exception cref="Network.Exceptions.UnknownException">
        /// Thrown if any error occurs. It's unusual.</exception>
        public void LoginAsync(string user, string password, VTankObject.Version version, 
            ActionFinished callback)
        {
            try
            {
                remoteCallback = callback;
                factory.VTankLogin_async(new Login_Callback(LoginCallback),
                    user, password, version);
            }
            // Unknown exceptions are escalated because it's unusual for an error to happen here.
            catch (Ice.Exception e)
            {
                logger.Error("LoginAsync() Ice.Exception: {0}", e);
                throw new UnknownException("Unable to establish a connection for logging in.");
            }
            catch (NullReferenceException e)
            {
                logger.Error("LoginAsync() System.NullReferenceException: {0}", e);
                throw new UnknownException("Unable to establish a connection for logging in.");
            }
            catch (System.Exception e)
            {
                logger.Error("LoginAsync() System.Exception: {0}", e);
                throw new UnknownException("Unable to establish a connection for logging in.");
            }
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
                    if (pinger != null)
                    {
                        pinger.Running = false;
                    }

                    if (keepAliveTimer != null)
                    {
                        keepAliveTimer.Dispose();
                    }

                    if (session != null)
                    {
                        session.Disconnect_async(new Disconnect_Callback());
                    }

                    if (router != null)
                    {
                        router.destroySession_async(new RouterDestroySession_Callback());
                    }

                    if (logger != null)
                    {
                        logger.Close();
                    }

                    DestroyCommunicator();
                }
                catch (System.Exception e)
                {
                    if (logger != null)
                    {
                        logger.Error(
                            "Unexpected error in the Disconnect() method: {0}", e);
                    }
                    else
                    {
                        Console.Error.WriteLine(e);
                    }
                }
                finally
                {
                    Connected = false;
                    pinger = null;
                    keepAliveTimer = null;
                    session = null;
                    router = null;
                    factory = null;
                    tankList = null;
                    tankListNeedsRefresh = true;
                   // weaponList = null;
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
        /// Get the list of tanks from the server. Note that it only actually communicates with
        /// the server when it needs to -- otherwise it caches the list to prevent repeated 
        /// unnecessary calls to the main server.
        /// </summary>
        /// <returns>Array list of tank attribute objects.</returns>
        public VTankObject.TankAttributes[] GetTankList()
        {
            RefreshTankList();
            return tankList;
        }

        /// <summary>
        /// Get the user's list of tanks asynchronously. It won't attempt to get a new list
        /// if it already has it cached and it doesn't need to be refreshed.
        /// </summary>
        /// <param name="finishedCallback">Function to call when the tank list returns.</param>
        /// <returns>True if the tank list does not need refreshing, and it is already
        /// available.</returns>
        public bool GetTankListAsync(ActionFinished<VTankObject.TankAttributes[]> finishedCallback)
        {
            if (!tankListNeedsRefresh)
            {
                return true;
            }

            session.GetTankList_async(new GetTankList_Callback(finishedCallback));

            return false;
        }

        /// <summary>
        /// Get the list of weapons from the server, which helps the client list statistics
        /// about each weapons. Repeated calls on this method are allowed, because it's
        /// cached locally after the first call so as to not waste the server's time.
        /// </summary>
        /// <returns>Array list of weapon types.</returns>
    /*    public VTankObject.Weapon[] GetWeaponList()
        {
            if (weaponList != null)
            {
                return weaponList;
            }

            weaponList = session.GetWeaponList();

            return weaponList;
        }*/

        /// <summary>
        /// Create a new tank synchronously. This method may throw an exception if some values
        /// are invalid.
        /// </summary>
        /// <param name="name">Name of the tank.</param>
        /// <param name="speedFactor">Speed factor of the tank.</param>
        /// <param name="armorFactor">Armor factor of the tank.</param>
        /// <param name="model">Model name of the tank.</param>
        /// <param name="weaponId">Weapon ID used with the tank.</param>
        /// <param name="color">Color of the tank.</param>
        /// <returns>True if the attempt succeeded, false if an error occurred.</returns>
        public bool CreateTank(string name, float speedFactor, float armorFactor, string model, 
            string skin, int weaponId, VTankObject.VTankColor color)
        {
            VTankObject.TankAttributes tank = new VTankObject.TankAttributes(
                name, speedFactor, armorFactor, 0, 100, model, skin,
                weaponId, 
                color);

            CheckTank(tank);

            try
            {
                if (session.CreateTank(tank))
                {
                    tankListNeedsRefresh = true;
                    return true;
                }
            }
            catch (Ice.Exception e)
            {
                logger.Error("CreateTank() Ice.Exception: {0}", e);
                throw new ConnectionLostException("You have lost connection with the server.");
            }
            catch (System.Exception e)
            {
                logger.Error("CreateTank() System.Exception: {0}", e);
                throw new UnknownException("An unknown error occurred.");
            }

            return false;
        }

        /// <summary>
        /// Create a new tank asynchronously, which means that this will return immediately as
        /// soon as the message is sent. It is very unusual, but this method can sometimes
        /// throw an UnknownException.
        /// </summary>
        /// <param name="name">Name of the tank.</param>
        /// <param name="speedFactor">Speed factor of the tank.</param>
        /// <param name="armorFactor">Armor factor of the tank.</param>
        /// <param name="model">Model name of the tank.</param>
        /// <param name="weaponId">Weapon ID used with the tank.</param>
        /// <param name="color">Color of the tank.</param>
        /// <param name="callback">Method to call once the attempt finishes.</param>
        public void CreateTankAsync(string name, float speedFactor, float armorFactor, string model,
            string skin, int weaponId, VTankObject.VTankColor color, ActionFinished<bool> callback)
        {
            VTankObject.TankAttributes tank = new VTankObject.TankAttributes(
                name, speedFactor, armorFactor, 0, 100, model, skin,
                weaponId,
                color);

            CheckTank(tank);

            try
            {
                session.CreateTank_async(new CreateTank_Callback(callback), tank);
            }
            catch (Ice.Exception e)
            {
                logger.Error("CreateTankAsync() Unexpected Ice.Exception: {0}", e);
                throw new UnknownException("Connection lost: An unknown error occurred.");
            }
            catch (System.Exception e)
            {
                logger.Error("CreateTankAsync() Unexpected System.Exception: {0}", e);
                throw new UnknownException("An unknown error occurred.");
            }

            // Assume the best.
            tankListNeedsRefresh = true;
        }

        /// <summary>
        /// Update an existing tank with new information synchronously. This method may throw an
        /// exception if the connection is lost or if an unexpected error occurred. It may also
        /// throw an exception if the given information is invalid.
        /// </summary>
        /// <param name="name">Name of the tank.</param>
        /// <param name="speedFactor">Speed factor of the tank.</param>
        /// <param name="armorFactor">Armor factor of the tank.</param>
        /// <param name="model">Model name of the tank.</param>
        /// <param name="weaponId">Weapon ID used with the tank.</param>
        /// <param name="color">Color of the tank.</param>
        /// <returns>True if the attempt succeeded; false otherwise.</returns>
        public bool UpdateTank(string name, float speedFactor, float armorFactor, string model,
            string skin, int weaponId, VTankObject.VTankColor color)
        {
            VTankObject.TankAttributes tank = new VTankObject.TankAttributes(
                name, speedFactor, armorFactor, 0, 100, model, skin,
                weaponId,
                color);

            CheckTank(tank);

            try
            {
                if (session.UpdateTank(name, tank))
                {
                    tankListNeedsRefresh = true;
                    return true;
                }
            }
            catch (VTankException e)
            {
                logger.Error("UpdateTank() VTankException: {0}", e);
                throw e;
            }
            catch (Ice.Exception e)
            {
                logger.Error("UpdateTank() Ice.Exception: {0}", e);
                throw new ConnectionLostException("You have lost connection with the server.");
            }
            catch (System.Exception e)
            {
                logger.Error("UpdateTank() System.Exception: {0}", e);
                throw new UnknownException("An unknown error occurred.");
            }

            return false;
        }

        /// <summary>
        /// Update an existing tank with new information asynchronously. This method may throw an
        /// exception if the information is invalid, or if an unknown error occurs.
        /// </summary>
        /// <param name="name">Name of the tank.</param>
        /// <param name="speedFactor">Speed factor of the tank.</param>
        /// <param name="armorFactor">Armor factor of the tank.</param>
        /// <param name="model">Model name of the tank.</param>
        /// <param name="weaponId">Weapon ID used with the tank.</param>
        /// <param name="color">Color of the tank.</param>
        /// <param name="callback">Method to be called once it finishes.</param>
        public void UpdateTankAsync(string name, float speedFactor, float armorFactor, string model, string skin,
            int weaponId, VTankObject.VTankColor color, ActionFinished<bool> callback)
        {
            VTankObject.TankAttributes tank = new VTankObject.TankAttributes(
                name, speedFactor, armorFactor, 0, 100, model, skin,
                weaponId,
                color);

            CheckTank(tank);

            try
            {
                session.UpdateTank_async(new UpdateTank_Callback(callback), name, tank);
            }
            catch (Ice.Exception e)
            {
                logger.Error("UpdateTankAsync() Unexpected Ice.Exception: {0}", e);
                throw new UnknownException("Connection lost: An unknown error occurred.");
            }
            catch (System.Exception e)
            {
                logger.Error("UpdateTankAsync() Unexpected System.Exception: {0}", e);
                throw new UnknownException("An unknown error occurred.");
            }

            // Assume the best.
            tankListNeedsRefresh = true;
        }

        /// <summary>
        /// Send a stack trace and description from the client when VTank has crashed.
        /// </summary>
        /// <param name="username">The user who encountered the error.</param>
        /// <param name="details">What was happening when it occurred.</param>
        /// <param name="stackTrace">The full stack trace.</param>
        public void SendErrorMessage(string username, string details, string stackTrace)
        {
            if (String.IsNullOrEmpty(username))
                username = "Unknown";

            factory.SendErrorMessage(username, details, stackTrace);
        }

        /// <summary>
        /// Gets the current rank of a player's tank.
        /// </summary>
        /// <returns>The rank.</returns>
        public int GetRank()
        {
            return (this.session.GetRank());
        }

        /// <summary>
        /// Asynchronously gets the rank of a player's tank.
        /// </summary>
        /// <param name="callback"></param>
        public void GetRankAsync(ActionFinished<int> callback)
        {
            session.GetRank_async(new GetRank_Callback(callback));
        }

        /// <summary>
        /// Gets the ranks of a set of tanks.
        /// </summary>
        /// <param name="tanks">An array of tank names.</param>
        /// <returns>An aarray of tank ranks.</returns>
        public int [] GetRanksOfTanks(string [] tanks)
        {
            int [] ranks = this.session.GetRanksOfTanks(tanks);
            return ranks;
        }

        /// <summary>
        /// Gets the number of points needed for a given rank.
        /// </summary>
        /// <param name="rank">The number of the rank.</param>
        /// <returns>The number of points needed to attain this rank.</returns>
        public long GetPointsForRank(int rank)
        {
            return(this.session.GetPointsForRank(rank));
        }

        /// <summary>
        /// Delete a tank that you own. Note that if you send an invalid name or a name that
        /// is not in the list, it will throw an exception.
        /// </summary>
        /// <param name="name">Name of the tank to delete.</param>
        /// <returns>True if the deletion occurred without issue; false otherwise.</returns>
        public bool DeleteTank(string name)
        {
            if (String.IsNullOrEmpty(name))
            {
                throw new InvalidValueException("The tank name must not be empty.");
            }

            // Check if the tank exists in the cached list.
            RefreshTankList();

            bool exists = false;
            for (int i = 0; i < tankList.Length; i++)
            {
                if (tankList[i].name == name)
                {
                    exists = true;

                    break;
                }
            }

            if (!exists)
            {
                throw new InvalidValueException("That tank does not exist.");
            }

            if (session.DeleteTank(name))
            {
                tankListNeedsRefresh = true;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Delete a tank that you own asynchronously. Note that if you send an invalid name or
        /// a name that is not in the list, it will throw an exception.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="callback"></param>
        public void DeleteTankAsync(string name, ActionFinished<bool> callback)
        {
            if (String.IsNullOrEmpty(name))
            {
                throw new InvalidValueException("The tank name must not be empty.");
            }

            // Check if the tank exists in the cached list.
            RefreshTankList();

            bool exists = false;
            for (int i = 0; i < tankList.Length; i++)
            {
                if (tankList[i].name == name)
                {
                    exists = true;

                    break;
                }
            }

            if (!exists)
            {
                throw new InvalidValueException("That tank does not exist.");
            }

            session.DeleteTank_async(new DeleteTank_Callback(callback), name);

            // Assume the best.
            tankListNeedsRefresh = true;
        }

        /// <summary>
        /// Retrieve a list of game servers from the main server.
        /// </summary>
        /// <returns>Array of GameServerInfo structs.</returns>
        public GameServerInfo[] GetGameServerList()
        {
            VTankObject.ServerInfo[] serverList = session.GetGameServerList();
            List<GameServerInfo> compiledList = new List<GameServerInfo>();

            for (int i = 0; i < serverList.Length; i++)
            {
                try
                {
                    compiledList.Add(new GameServerInfo(communicator, serverList[i]));
                }
                catch (Ice.ConnectionRefusedException e)
                {
                    // ConnectionRefusedException means that the built proxy does not point to
                    // a host that accepts Ice connections.
                    logger.Warning(
                        "GetGameServerList() Ice.ConnectionRefusedException: {0}", e.error);
                }
                catch (Ice.DNSException e)
                {
                    // Thrown if the host name is invalid.
                    logger.Warning(
                        "GetGameServerList() Ice.DNSException: {0}", e.error);
                }
                catch (Ice.ObjectNotExistException e)
                {
                    // The target host took the connection, but it wasn't valid Ice.
                    logger.Warning(
                        "GetGameServerList() Ice.ObjectNotExistException: {0}", e.Message);
                }
                catch (Ice.Exception e)
                {
                    // Unexpected exception.
                    logger.Error(
                        "GetGameServerList() Unexpected Ice.Exception: {0}", e);
                }
            }

            return compiledList.ToArray();
        }

        /// <summary>
        /// Tell the server that the client wants to use a certain tank, identified by it's
        /// name.
        /// </summary>
        /// <param name="tankName">Name of the tank to select.</param>
        /// <returns>True if no errors occurred; false otherwise.</returns>
        public bool SelectTank(string tankName)
        {
            try
            {
                return session.SelectTank(tankName);
            }
            catch (Exceptions.BadInformationException e)
            {
                logger.Info("SelectTank() Failure: {0}", e.reason);
            }
            catch (Ice.Exception e)
            {
                logger.Error(
                    "SelectTank() Unexpected Ice.Exception: {0}", e);

                Disconnect();
                throw new ConnectionLostException("You have lost connection with the server.");
            }

            return false;
        }

        /// <summary>
        /// Ask to join the target game server.
        /// </summary>
        /// <param name="gameServer">Server to join.</param>
        /// <returns>Key to authenticate with Theater. When joining the server, this key
        /// must be presented.</returns>
        public string RequestJoinGameServer(GameServerInfo gameServer)
        {
            string key = "";
            try
            {
                key = session.RequestJoinGameServer(
                    new VTankObject.ServerInfo(gameServer.Host,
                    gameServer.Port, gameServer.Name, gameServer.Approved, gameServer.UseGlacier2,
                    gameServer.NumberOfPlayers, gameServer.PlayerLimit, gameServer.CurrentMap, 
                    gameServer.CurrentGameMode));
            }
            catch (VTankException e)
            {
                logger.Error("RequestJoinGameServer() VTankException: {0}", e);
                throw new InvalidHostException("That game server is not available.");
            }
            catch (Ice.Exception e)
            {
                logger.Error("RequestJoinGameServer() Ice.Exception: {0}", e);
                throw new ConnectionLostException("You have lost connection with the server.");
            }
            catch (System.Exception e)
            {
                logger.Error("RequestJoinGameServer() System.Exception: {0}", e);
                throw new UnknownException("An unknown error occurred.");
            }

            return key;
        }

        /// <summary>
        /// Check what the most recent version of the client is from the Main server.
        /// </summary>
        /// <returns>The current version I.E. a version number such as R18c where 18 is the major revision 
        /// and c is the minor.  A minor revision may not always be present.</returns>
        public string CheckClientVersion()
        {
            string clientVersion = "Unknown";
            try
            {
                clientVersion = session.CheckClientVersion();
            }
            catch (System.Exception e)
            {
                Console.WriteLine(e);
            }
            return clientVersion;
        }
        
        /// <summary>
        /// Get the proxy which has methods that points to the created session, if one exists.
        /// TODO: This method is a temporary replacement for other implementation.
        /// </summary>
        /// <returns>Pointer to the session proxy.</returns>
        public MainSessionPrx GetProxy()
        {
            return session;
        }
        #endregion

        #region IDisposable Members

        /// <summary>
        /// Explicitly clean up unmanaged resources used by this class. The class is useless
        /// after this call -- it must be re-created through Connect().
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
