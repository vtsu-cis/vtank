using System;
using AIFramework.Bot;
using System.Collections.Generic;
using Network.Util;
using System.Threading;
using AIFramework.Util;
using System.IO;

namespace AIFramework.Runner
{
    /// <summary>
    /// Executes any number of currently running bots.
    /// </summary>
    public sealed class BotRunner
    {
        #region Packed Class
        /// <summary>
        /// Package multiple parameters into one struct.
        /// </summary>
        class BotPackage
        {
            private BotRunnerState state;

            #region Properties
            public Thread Thread;
            public VTankBot Bot;
            public GameServerInfo Server;
            public BotRunnerState State
            {
                get
                {
                    lock (botLock)
                    {
                        return state;
                    }
                }

                set
                {
                    lock (botLock)
                    {
                        state = value;
                    }
                }
            }
            #endregion

            public string Username
            {
                get { return Bot.AuthInfo.Username; }
            }

            public string TankName
            {
                get { return String.IsNullOrEmpty(Bot.TankName) ? "<?>" : Bot.TankName; }
            }

            public bool IsShuttingDown { get; set; }

            public BotPackage(VTankBot bot, GameServerInfo info, Thread thread)
            {
                Bot = bot;
                Server = info;
                Thread = thread;
                state = BotRunnerState.Offline;
                IsShuttingDown = false;
            }
        };

        public enum BotRunnerState
        {
            Offline,
            SelectingTank,
            Playing,
        }
        #endregion

        #region Members
        private static object botLock = new object();

        public delegate void Disconnect(VTankBot bot);
        public delegate void BotStateChange(VTankBot bot, BotRunnerState newState);

        public const string DefaultConfigFile = "config.ai";

        private List<VTankBot> bots;
        private List<BotPackage> threads;

        public event Disconnect OnCrash;
        public event BotStateChange OnBotStateChange;
        #endregion

        #region Properties
        /// <summary>
        /// Gets the configuration file being used by this runner.
        /// </summary>
        public string ConfigFile
        {
            get;
            private set;
        }

        public TargetServer EchelonAddress
        {
            get;
            private set;
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Constructs the bot runner using all default parameters. Note that the bot
        /// runner is not automatically executed. Call 'Start()' to run the bot runner.
        /// </summary>
        public BotRunner()
            : this(DefaultConfigFile)
        {
        }

        /// <summary>
        /// Constructs the bot runner using a custom configuration file. Note that the
        /// bot runner is not automatically executed. Call 'Start()' to run the bot
        /// runner.
        /// </summary>
        /// <param name="configurationFile">Configuration file to read.</param>
        public BotRunner(string configurationFile)
        {
            ConfigFile = configurationFile;

            // TODO: Replace with config.
            EchelonAddress = new TargetServer("glacier2a.cis.vtc.edu", 4063);

            bots = new List<VTankBot>();
            threads = new List<BotPackage>();

        }
        #endregion

        #region Public Methods
        public VTankBot Register(Type botType, AuthInfo authInfo, string tankName)
        {
            // TODO: User still can't specify custom server.
            VTankBot bot = (VTankBot)Activator.CreateInstance(botType, 
                this, EchelonAddress, authInfo);
            bot.TankName = tankName;
            bots.Add(bot);

            return bot;
        }

        public void Parse(string configFile)
        {
            using (StreamReader file = new StreamReader(new FileStream(configFile, FileMode.Open)))
            {
                string line = null;
                while ((line = file.ReadLine()) != null)
                {
                    if (String.IsNullOrEmpty(line))
                    {
                        break;
                    }

                    if (line.StartsWith("#"))
                    {
                        continue;
                    }

                    try
                    {
                        string[] data = line.Split(new char[] { '=' }, 2, StringSplitOptions.None);
                        Type type = Type.GetType("VTankBotRunner." + data[0].Trim());
                        if (type == null)
                        {
                            type = Type.GetType(data[0].Trim());
                        }

                        AuthInfo auth = AuthInfo.FromString(data[1].Trim());

                        Register(type, auth, null);
                    }
                    catch (Exception e)
                    {
                        Console.Error.WriteLine(
                            "Warning: Found corrupted bot in file: {0}", e);
                    }
                }
            }
        }

        /// <summary>
        /// Start the bot runner, letting every bot connect to a game server.
        /// </summary>
        /// <param name="mainLoopMode">If true, loops forever.</param>
        public void Start(bool mainLoopMode)
        {
            CullOfflineBots();

            if (bots.Count == 0)
            {
                throw new InvalidOperationException(
                    "Cannot start without registered bots.");
            }

            // TODO: Perhaps in the future the bots can selected their preferred server.
            GameServerInfo[] servers = bots[0].MainServer.GetGameServerList();
            if (servers.Length == 0)
            {
                throw new Exception("No game servers available.");
            }
            
            foreach (VTankBot bot in bots)
            {
                Thread thread = new Thread(new ParameterizedThreadStart(HandleBot));
                BotPackage package = new BotPackage(bot, servers[0], thread);
                thread.Start(package);
                threads.Add(package);

                Thread.Sleep(250);
            }

            while (mainLoopMode)
            {
                for (int j = 0; j < threads.Count; ++j)
                {
                    BotPackage package = threads[j];
                    Thread thread = package.Thread;
                    if (thread.Join(5000))
                    {
                        // Restart the thread.
                        Console.WriteLine("<<< Restarting bot thread. >>>");
                        thread = new Thread(new ParameterizedThreadStart(HandleBot));
                        thread.Start(threads[j]);
                    }
                }

                Thread.Sleep(5000);
            }

            bots.Clear();
        }

        /// <summary>
        /// Remove all offline bots from the bot list.
        /// </summary>
        private void CullOfflineBots()
        {
            lock (this)
            {
                List<BotPackage> offlineBots = threads.FindAll((bot) =>
                {
                    return bot.State == BotRunnerState.Offline;
                });

                while (offlineBots.Count > 0)
                {
                    BotPackage bot = offlineBots[0];
                    offlineBots.RemoveAt(0);
                    threads.Remove(bot);
                }
            }
        }

        /// <summary>
        /// Kill the given bot and the thread running the bot.
        /// </summary>
        /// <param name="bot">Bot to kill.</param>
        public void Kill(VTankBot bot)
        {
            if (bot == null)
                return;

            lock (botLock)
            {
                for (int i = 0; i < threads.Count; ++i)
                {
                    BotPackage package = threads[i];
                    if (package.Bot == bot)
                    {
                        try
                        {
                            if (!package.IsShuttingDown)
                            {
                                package.Thread.Abort();
                            }

                            package.Bot.Dispose();
                        }
                        catch (Exception e) 
                        {
                            Console.Error.WriteLine(e);
                        }

                        threads.RemoveAt(i);
                        break;
                    }
                }
            }

            CullOfflineBots();
        }

        /// <summary>
        /// Kill the given bot and the thread running the bot.
        /// </summary>
        /// <param name="username">Username of the bot.</param>
        public bool Kill(string username)
        {
            lock (botLock)
            {
                bool found = false;
                BotPackage package = threads.Find((b) =>
                {
                    return b.Bot.AuthInfo.Username == username;
                });
                if (package != null)
                    found = true;

                if (found)
                {
                    try
                    {
                        if (package.Thread.IsAlive)
                            package.Thread.Abort();
                        else
                            package.Bot.Dispose();
                    }
                    catch (Exception) { }

                    threads.Remove(package);
                }

                return found;
            }
        }

        /// <summary>
        /// Kill and remove all bots and their running threads.
        /// </summary>
        public void KillAll()
        {
            lock (botLock)
            {
                for (int i = 0; i < threads.Count; ++i)
                {
                    try
                    {
                        if (threads[i].Thread.IsAlive)
                            threads[i].Thread.Abort();
                        else
                            threads[i].Bot.Dispose();
                    }
                    catch (Exception) { }
                }

                threads.Clear();
            }
        }

        /// <summary>
        /// Gets all bots currently running.
        /// </summary>
        /// <returns></returns>
        public List<VTankBot> GetRunningBots()
        {
            lock (botLock)
            {
                List<VTankBot> result = new List<VTankBot>();
                List<BotPackage> runningBots = threads.FindAll((bot) =>
                    { return bot.State == BotRunnerState.Playing; });

                foreach (BotPackage package in runningBots)
                    result.Add(package.Bot);

                runningBots.Clear();

                return result;
            }
        }
        #endregion

        #region Private Methods
        private void SetState(BotPackage bot, BotRunnerState state)
        {
            bot.State = state;
            if (OnBotStateChange != null)
                OnBotStateChange(bot.Bot, state);
        }

        /// <summary>
        /// Runs the bot on a separate thread.
        /// </summary>
        /// <param name="botKlass"></param>
        private void HandleBot(object botObj)
        {
            BotPackage package = (BotPackage)botObj;
            package.State = BotRunnerState.Offline;
            VTankBot bot = package.Bot;
            GameServerInfo server = package.Server;

            try
            {
                if (!bot.MainServer.Connected)
                {
                    bot.MainServer.Connect();
                }

                SetState(package, BotRunnerState.SelectingTank);

                // TODO: Let the user pick their own tank..
                if (string.IsNullOrEmpty(bot.TankName))
                {
                    VTankObject.TankAttributes[] tanks = bot.MainServer.GetTankList();
                    if (tanks.Length == 0)
                    {
                        Console.Error.WriteLine("Error: Bot {0} cannot play: he has no tanks.",
                            bot.ToString());
                        SetState(package, BotRunnerState.Offline);
                        return;
                    }
                    bot.SelectTank(tanks[0].name);
                }
                else
                {
                    bot.SelectTank(bot.TankName);
                }

                ConnectToServer(bot, server);
                SetState(package, BotRunnerState.Playing);

                DoLoop(package, bot);
            }
            catch (Exception e)
            {
                using (Logger logger = new Logger("error.log", FileMode.Append))
                {
                    logger.Exception(e);
                }

                Console.Error.WriteLine(e);
            }
            finally
            {
                package.IsShuttingDown = true;
                Kill(bot);
            }
        }

        /// <summary>
        /// Connect to the given game server.
        /// </summary>
        /// <param name="bot"></param>
        /// <param name="server"></param>
        private static void ConnectToServer(VTankBot bot, GameServerInfo server)
        {
            bot.ConnectToGameServer(server);
        }

        /// <summary>
        /// Do the main game loop.
        /// </summary>
        /// <param name="bot"></param>
        private void DoLoop(BotPackage package, VTankBot bot)
        {
            bool crash = false;
            const int WAIT_TIME = 14; // ms
            try
            {
                while (true)
                {
                    Thread.Sleep(WAIT_TIME);

                    bot.InvokeUpdate();
                }
            }
            catch (System.Threading.ThreadInterruptedException)
            {
                Console.WriteLine("Bot {0} was interrupted.", bot.AuthInfo.Username);
            }
            catch (ThreadAbortException)
            {
                Console.WriteLine("Bot {0} was aborted.", bot.AuthInfo.Username);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                crash = true;
            }

            if (crash && OnCrash != null)
            {
                package.State = BotRunnerState.Offline;
                OnCrash(bot);
            }
            else
            {
                SetState(package, BotRunnerState.Offline);
            }
        }
        #endregion
    }
}
