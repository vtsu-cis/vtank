using System;
using AIFramework.Runner;
using AIFramework.Bot;
using System.Threading;
using AIFramework.Bot.Game;
using System.Collections.Generic;
using AIFramework.Util;
using Exceptions;

namespace VTankBotRunner.Util
{
    public class BotManager : IDisposable
    {
        #region Members
        public delegate void PrinterCallback(string text);
        public delegate void BotChangeCallback(VTankBot bot);

        public event BotChangeCallback OnBotChange;
        public event PrinterCallback OnTextWrite;

        private InvocationBuffer buffer;
        private List<VTankBot> reservedList;
        private bool needsBalance;
        #endregion

        #region Properties
        /// <summary>
        /// Gets the bot runner that is managed.
        /// </summary>
        public BotRunner BotRunner
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets whether or not the bot manager should automatically force bots to
        /// reconnect once they disconnect.
        /// </summary>
        public bool AutoReconnect
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets how long to wait until a bot reconnects after a sudden disconnection.
        /// </summary>
        public long WaitTimeReconnect
        {
            get;
            set;
        }

        /// <summary>
        /// Gets whether or not bots should auto-balance themselves.
        /// </summary>
        public bool BalanceBots
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets whether or not the bots need to be balanced.
        /// </summary>
        private bool NeedsBalance
        {
            get
            {
                lock (this)
                {
                    return needsBalance;
                }
            }

            set
            {
                lock (this)
                {
                    needsBalance = value;
                }
            }
        }
        #endregion

        #region Constructor
        public BotManager(BotRunner runner)
        {
            buffer = new InvocationBuffer();
            reservedList = new List<VTankBot>();

            BotRunner = runner;
            WaitTimeReconnect = 5000;

            BotRunner.OnCrash += new BotRunner.Disconnect((bot) => 
            {
                buffer.Enqueue(new Invocation.InvocationTarget(HandleCrash), 
                    bot, WaitTimeReconnect);
            });

            BotRunner.OnBotStateChange += new BotRunner.BotStateChange(BotRunner_OnBotStateChange);
            needsBalance = false;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Start the bot manager.
        /// </summary>
        public void Start()
        {
            BotRunner.Start(false);
        }

        /// <summary>
        /// Gets a list of currently online players.
        /// </summary>
        /// <param name="includeBots">If true, the bots ran by this bot runner are
        /// included in the list. If false, they are excluded.</param>
        /// <returns>List of tank names of online players. The list is null if the
        /// online player list cannot be accessed.</returns>
        public List<Player> GetOnlinePlayers(bool includeBots)
        {
            List<VTankBot> bots = BotRunner.GetRunningBots();

            List<Player> result = new List<Player>();
            for (int i = 0; i < bots.Count; ++i)
            {
                VTankBot bot = bots[i];
                if (bot.GameServer != null && bot.GameServer.Connected)
                {
                    AIFramework.Util.GameTracker game = bot.Game;
                    List<Player> playerList = game.GetPlayerList();
                    foreach (AIFramework.Bot.Game.Player player in playerList)
                    {
                        if (includeBots)
                        {
                            result.Add(player);
                        }
                        else
                        {
                            if (!ContainsTank(bots, player.Name))
                            {
                                result.Add(player);
                            }
                        }
                    }

                    break;
                }
            }

            return result;
        }

        /// <summary>
        /// Event called by the bot runner when a bot's state changes.
        /// </summary>
        /// <param name="bot"></param>
        /// <param name="newState"></param>
        private void BotRunner_OnBotStateChange(VTankBot bot, 
            AIFramework.Runner.BotRunner.BotRunnerState newState)
        {
            if (newState != BotRunner.BotRunnerState.SelectingTank && OnBotChange != null)
            {
                buffer.Enqueue(new Invocation.InvocationTarget((o) => 
                {
                    OnBotChange((VTankBot)o);
                }), 
                bot);

                NeedsBalance = true;
            }

            buffer.Enqueue(new Invocation((o) =>
            {
                Print("{0}'s state: {1}", bot.AuthInfo.Username, newState.ToString());
            }));
        }

        /// <summary>
        /// Check to see if the given username is in our bot list.
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        private bool Contains(List<VTankBot> bots, string username)
        {
            foreach (VTankBot bot in bots)
                if (bot.AuthInfo.Username.ToLower() == username.ToLower())
                    return true;

            return false;
        }

        /// <summary>
        /// Check to see if a bot in our userlist contains a given tank name.
        /// </summary>
        /// <param name="tankName"></param>
        /// <returns></returns>
        private bool ContainsTank(List<VTankBot> bots, string tankName)
        {
            foreach (VTankBot bot in bots)
                if (!String.IsNullOrEmpty(bot.TankName) && 
                        bot.TankName.ToLower() == tankName.ToLower())
                    return true;

            return false;
        }

        /// <summary>
        /// Print a message to the console, or to any listeners of messages.
        /// </summary>
        /// <param name="text">Text to print.</param>
        private void Print(string text)
        {
            if (OnTextWrite != null)
                OnTextWrite(text);
            else
                Console.WriteLine(text);
        }

        /// <summary>
        /// Print a message to the console, or to any listeners of messages.
        /// </summary>
        /// <param name="format">Format of the text to print.</param>
        /// <param name="p">Parameters to the text to print.</param>
        private void Print(string format, params object[] p)
        {
            Print(String.Format(format, p));
        }

        /// <summary>
        /// Do bot auto-balancing.
        /// </summary>
        private void AutoBalanceBots()
        {
            try
            {
                // Check the status.
                List<VTankBot> bots = BotRunner.GetRunningBots();
                List<Player> onlinePlayers = GetOnlinePlayers(false);
                if (onlinePlayers.Count == 0)
                {
                    // No non-bots are online.
                    while (reservedList.Count > 0)
                    {
                        VTankBot bot = (VTankBot)reservedList[0];
                        Print("[AutoBalance]: Moving {0} from reserve.", bot.AuthInfo.Username);
                        MoveBotFromReserve(bot);
                    }

                    return;
                }

                const int MINIMUM_SUM = 4;
                int botCount = bots.Count;
                int playerCount = onlinePlayers.Count;
                int reservedCount = reservedList.Count;
                int totalBots = botCount + reservedCount;
                int sum = botCount + playerCount;
                bool teams = bots[0].Player.Team != GameSession.Alliance.NONE;

                // Case 1: too many bots.
                if (sum > totalBots && sum > MINIMUM_SUM && bots.Count != 1)
                {
                    // Remove one bot keeping fair teams in mind.
                    VTankBot bot = null;
                    if (teams)
                    {
                        List<Player> fullPlayerList = GetOnlinePlayers(true);
                        int redCount = fullPlayerList.FindAll((x) =>
                        {
                            return x.Team == GameSession.Alliance.RED;
                        }).Count;
                        int blueCount = fullPlayerList.Count - redCount;

                        if (redCount > blueCount)
                            bot = bots.Find((x) => { return x.Player.Team == GameSession.Alliance.RED; });
                        else if (blueCount > redCount)
                            bot = bots.Find((x) => { return x.Player.Team == GameSession.Alliance.BLUE; });
                        else
                            // Doesn't matter. Remove first bot.
                            bot = bots[0];
                    }
                    else
                        bot = bots[0];

                    if (bot != null)
                    {
                        Print(String.Format("[AutoBalance]: Removing bot {0}.",
                            bot.AuthInfo.Username, sum, totalBots));
                        MoveBotToReserve(bot);
                    }
                }

                // Case 2: not enough bots.
                else if (sum < totalBots || (sum < MINIMUM_SUM && reservedCount > 0))
                {
                    VTankBot bot = (VTankBot)reservedList[0];
                    Print("[AutoBalance]: Adding bot " + bot.AuthInfo.Username);
                    MoveBotFromReserve(bot);

                    NeedsBalance = false;
                }
            }
            catch (Exception ex)
            {
                Print(ex.ToString());
            }
        }

        /// <summary>
        /// Move an online bot to the reserved bot list.
        /// </summary>
        /// <param name="username"></param>
        public void MoveBotToReserve(string username)
        {
            List<VTankBot> bots = BotRunner.GetRunningBots();
            VTankBot bot = bots.Find((b) => { return b.AuthInfo.Username == username; });

            if (bot != null)
                MoveBotToReserve(bot);
        }

        /// <summary>
        /// Move an online bot to the reserved bot list.
        /// </summary>
        /// <param name="username"></param>
        public void MoveBotToReserve(VTankBot bot)
        {
            reservedList.Add(bot);
            BotRunner.Kill(bot);

            Print("{0} moved to reserve.", bot.AuthInfo.Username);

            if (OnBotChange != null)
                OnBotChange(bot);
        }

        /// <summary>
        /// Move a bot from the reserved list to the online list.
        /// </summary>
        /// <param name="username"></param>
        public void MoveBotFromReserve(string username)
        {
            VTankBot bot = reservedList.Find((b) => { return b.AuthInfo.Username == username; });
            if (bot != null)
                MoveBotFromReserve(bot);
        }

        /// <summary>
        /// Move a bot from the reserved list to the online list.
        /// </summary>
        /// <param name="username"></param>
        public void MoveBotFromReserve(VTankBot bot)
        {
            try
            {
                AddBot(bot.GetType(), bot.AuthInfo.Username, bot.AuthInfo.Password);
                reservedList.Remove(bot);

                Print(String.Format("Bot {0} moved from reserve.", bot.AuthInfo.Username));
            }
            catch (Exception ex)
            {
                Print(ex.ToString());
            }

            if (OnBotChange != null)
                OnBotChange(bot);
        }

        /// <summary>
        /// Add a bot to the runner.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        public void AddBot(Type type, string username, string password)
        {
            List<VTankBot> bots = BotRunner.GetRunningBots();

            // Remove old bot if it exists.
            for (int i = 0; i < bots.Count; ++i)
            {
                VTankBot bot = bots[i];
                if (bot.AuthInfo.Username == username)
                {
                    Print("Removed duplicate bot {0}.", username);
                    BotRunner.Kill(bot);
                    break;
                }
            }

            AuthInfo auth = new AuthInfo(username, password);
            BotRunner.Register(type, auth, null);
            BotRunner.Start(false);

            Print("Added new bot, {0}.", username);

            if (OnBotChange != null)
                OnBotChange(null);
        }

        /// <summary>
        /// Delegate method called by the invocation buffer after a bot has crashed.
        /// This method attempts to reconnect the bot.
        /// </summary>
        /// <param name="botObj"></param>
        private void HandleCrash(object botObj)
        {
            VTankBot bot = (VTankBot)botObj;

            Print("Restarting bot {0}...", bot.TankName);

            AddBot(bot.GetType(), bot.AuthInfo.Username, bot.AuthInfo.Password);
            /*try
            {
                AddBot(bot.GetType(), bot.AuthInfo.Username, bot.AuthInfo.Password);
            }
            catch (Exception e)
            {
                string errorMessage = e.Message;
                Exception inner = e.InnerException;
                while (inner != null)
                {
                    errorMessage = inner.Message;
                    inner = inner.InnerException;
                }

                // Re-connect attempt failed -- try again soon.
                Print("{0} can't reconnect (trying again soon): {1}",
                    bot.AuthInfo.Username, errorMessage);
                Console.Error.WriteLine();
                Console.Error.WriteLine(e.ToString());
                Console.Error.WriteLine();

                buffer.Enqueue(new Invocation.InvocationTarget(HandleCrash),
                    botObj, WaitTimeReconnect * 2);
            }*/
        }

        /// <summary>
        /// Gets all running bots on the bot runner.
        /// </summary>
        /// <returns>List of running bots.</returns>
        public List<VTankBot> GetBots()
        {
            return BotRunner.GetRunningBots();
        }

        /// <summary>
        /// Gets a list of reserved listed bots. Bots on the reserve list are not running,
        /// but can be moved to the active list anytime.
        /// </summary>
        /// <returns></returns>
        public List<VTankBot> GetReserveList()
        {
            return reservedList;
        }

        /// <summary>
        /// Remove a bot from the entire bot manager, including the reserved list.
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public bool Remove(string username)
        {
            bool found = false;
            if (!BotRunner.Kill(username))
            {
                VTankBot bot = reservedList.Find((b) => { return b.AuthInfo.Username == username; });
                if (bot != null)
                {
                    found = reservedList.Remove(bot);
                }
            }
            else
                found = true;

            if (found && OnBotChange != null)
                OnBotChange(null);

            return found;

        }

        /// <summary>
        /// Updates the bot manager, invoking any events on the invocation buffer and
        /// auto-balancing the bots (if necessary).
        /// </summary>
        public void Update()
        {
            bool executed = false;
            do
            {
                try
                {
                    executed = buffer.InvokeNext();
                }
                catch (Exception e)
                {
                    Print("Invocation error: {0}", e.Message);
                }
            }
            while (executed);

            if (BalanceBots && NeedsBalance)
                AutoBalanceBots();
        }
        #endregion

        #region IDisposable Members
        /// <summary>
        /// Disposes of the bot manager, ensuring that all bots are knocked offline and that
        /// all resources used are cleaned up.
        /// </summary>
        public void Dispose()
        {
            try
            {
                BotRunner.KillAll();
                buffer.Clear();
            }
            catch (Exception) {}
        }
        #endregion
    }
}
