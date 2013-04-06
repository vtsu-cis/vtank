using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using AIFramework.Runner;
using AIFramework.Util;
using System.IO;
using AIFramework.Bot;
using System.Threading;
using System.Reflection;
using VTankBotRunner.Util;
using AIFramework.Bot.Game;

namespace VTankBotRunner.GUI
{
    public partial class MainWindow : Form
    {
        #region Members
        private int consoleLines = 0;
        private string configFile;
        private BotRunner runner;
        private delegate void InsertText(string text);
        private delegate void TickDelegate();
        private System.Windows.Forms.Timer updateTimer;
        private BotManager botManager;
        #endregion

        #region Properties
        public BotManager BotManager
        {
            get { return botManager; }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Displays and configures the main window.
        /// Starts the bot runner.
        /// </summary>
        /// <param name="configurationFile">Path to the bot configuration file.</param>
        /// <param name="botRunner">Created bot runner.</param>
        public MainWindow(string configurationFile, BotRunner botRunner)
        {
            Visible = false;

            botManager = new BotManager(botRunner);
            updateTimer = new System.Windows.Forms.Timer();
            updateTimer.Interval = 100;
            updateTimer.Tick += new EventHandler((sender, e) =>
            {
                botManager.Update();
            });

            this.configFile = configurationFile;
            this.runner = botRunner;

            SetOptions();

            InitializeComponent();
        }
        #endregion

        #region Methods
        /// <summary>
        /// Sets all options to their default values.
        /// </summary>
        public void SetOptions()
        {
            botManager.BalanceBots = BotRunnerOptions.GetValueBool("AutoBotRemoveEnabled");
        }

        /// <summary>
        /// Read the list of bots from the configuration file.
        /// </summary>
        private void ReadBotsFromConfigFile()
        {
            using (StreamReader file = new StreamReader(new FileStream(configFile, FileMode.Open)))
            {
                string line = null;
                while ((line = file.ReadLine()) != null)
                {
                    if (String.IsNullOrEmpty(line))
                        break;

                    if (line.StartsWith("#"))
                        continue;

                    try
                    {
                        string[] data = line.Split(new char[] { '=' }, 2, StringSplitOptions.None);
                        Type type = Type.GetType("VTankBotRunner." + data[0].Trim());
                        if (type == null)
                        {
                            type = Type.GetType(data[0].Trim());
                            if (type == null)
                                throw new Exception("Bot type does not exist.");
                        }
                        
                        AuthInfo auth = AuthInfo.FromString(data[1].Trim());
                        runner.Register(type, auth, null);

                        InsertTextIntoConsole(String.Format("Adding bot: {0}", auth.Username));
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
        /// Refreshes the list view containing the bot list.
        /// </summary>
        private void RefreshBotList()
        {
            onlineBotsList.Items.Clear();
            foreach (VTankBot bot in botManager.GetBots())
            {
                string entry = bot.AuthInfo.Username;
                string tankName = bot.TankName;
                if (tankName != null)
                {
                    entry += " - " + tankName;
                }

                onlineBotsList.Items.Add(entry);
            }

            reservedList.Items.Clear();
            foreach (VTankBot bot in botManager.GetReserveList())
            {
                string entry = bot.AuthInfo.Username;
                reservedList.Items.Add(entry);
            }

            removeButton.Enabled = false;
            moveToReserveButton.Enabled = false;
            moveToOnlineButton.Enabled = false;
        }

        /// <summary>
        /// Attempt to get the player list.
        /// </summary>
        /// <returns></returns>
        public List<string> TryGetPlayerList()
        {
            List<VTankBot> bots = botManager.GetBots();
            List<string> playerList = new List<string>();
			
            try
            {
                for (int i = 0; i < bots.Count; ++i)
                {
                    VTankBot bot = bots[i];
                    if (!bot.GameServer.Connected)
                        continue;

                    List<Player> players = bot.Game.GetPlayerList();
                    foreach (Player player in players)
                    {
                        playerList.Add(player.Name);
                    }
                }
            }
            catch { }

            return playerList;
        }

        /// <summary>
        /// Invocation target to insert text into the text box.
        /// </summary>
        /// <param name="text"></param>
        public void InsertTextIntoConsole(string text)
        {
            consoleArea.Text += text + '\n';
            consoleLines++;

            const int HARD_CAP = 100;
            while (consoleLines > HARD_CAP)
            {
                consoleArea.Text = consoleArea.Text.Remove(0, consoleArea.Text.IndexOf('\n') + 1);
                consoleLines--;
            }

            consoleArea.SelectionStart = consoleArea.Text.Length;
            consoleArea.ScrollToCaret();
            consoleArea.Refresh();
        }

        
        #endregion

        #region Event Handlers
        private void MainWindow_Load(object sender, EventArgs e)
        {
            Thread startupThread = new Thread(new ThreadStart(() =>
            {
                BotRunnerOptions.ReadOptions();

                ReadBotsFromConfigFile();
                botManager.Start();
            }));
            startupThread.Start();

            StartingUpForm form = new StartingUpForm(startupThread);
            form.Show();
            form.BringToFront();

            form.StartupFinished += new StartingUpForm.StartupFinishedCallback(() =>
            {
                updateTimer.Start();

                botManager.BalanceBots = BotRunnerOptions.GetValueBool("AutoBotRemoveEnabled");
                botManager.OnBotChange += new BotManager.BotChangeCallback(botManager_OnBotChange);
                botManager.OnTextWrite += new BotManager.PrinterCallback(InsertTextIntoConsole);

                Visible = true;
                BringToFront();

                RefreshBotList();
            });
        }

        private void botManager_OnBotChange(VTankBot bot)
        {
            RefreshBotList();
        }

        private void MainWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.Visible = false;
            Refresh();
        }

        private void MainWindow_FormClosed(object sender, FormClosedEventArgs e)
        {
            Thread shutdownThread = new Thread(new ThreadStart(() =>
            {
                try
                {
                    botManager.Dispose();
                }
                catch (Exception) { }
            }));
            shutdownThread.Start();

            ShuttingDownDialog dialog = new ShuttingDownDialog(shutdownThread);
            dialog.ShowDialog();
            dialog.BringToFront();
        }
        
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Visible = false;
            Refresh();
            Close();
        }

        private void onlineBotsList_SelectedIndexChanged(object sender, EventArgs e)
        {
            int selectedIndex = onlineBotsList.SelectedIndex;
            if (selectedIndex < 0 || onlineBotsList.SelectedItem == null)
            {
                removeButton.Enabled = false;
                moveToReserveButton.Enabled = false;
            }
            else
            {
                removeButton.Enabled = true;
                moveToReserveButton.Enabled = true;
            }
        }

        private void reservedList_SelectedIndexChanged(object sender, EventArgs e)
        {
            int selectedIndex = reservedList.SelectedIndex;
            if (selectedIndex < 0 || reservedList.SelectedItem == null)
            {
                removeButton.Enabled = false;
                moveToOnlineButton.Enabled = false;
            }
            else
            {
                removeButton.Enabled = true;
                moveToOnlineButton.Enabled = true;
            }
        }

        private void removeButton_Click(object sender, EventArgs e)
        {
            object selectedObj = onlineBotsList.SelectedItem;
            if (selectedObj == null)
            {
                selectedObj = reservedList.SelectedItem;
                if (selectedObj == null)
                {
                    removeButton.Enabled = false;
                    return;
                }
            }

            string[] selectedData = selectedObj.ToString().Split('-');
            string username = selectedData[0].Trim();

            botManager.Remove(username);

            RefreshBotList();
            //removeButton.Enabled = false;
        }

        private void moveToReserveButton_Click(object sender, EventArgs e)
        {
            if (BotRunnerOptions.GetValueBool("AutoBotRemoveEnabled"))
            {
                MessageBox.Show(this, "This option is disabled while auto-removing bots is enabled.",
                    "Can't do that.", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            object selectedObj = onlineBotsList.SelectedItem;
            if (selectedObj == null)
            {
                moveToReserveButton.Enabled = false;
                return;
            }

            string username = selectedObj.ToString().Split('-')[0].Trim();
            botManager.MoveBotToReserve(username);
        }

        private void moveToOnlineButton_Click(object sender, EventArgs e)
        {
            if (BotRunnerOptions.GetValueBool("AutoBotRemoveEnabled"))
            {
                MessageBox.Show(this, "This option is disabled while auto-removing bots is enabled.",
                    "Can't do that.", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            object selectedObj = reservedList.SelectedItem;
            if (selectedObj == null)
            {
                moveToOnlineButton.Enabled = false;
                return;
            }

            string botUsername = (string)selectedObj;

            try
            {
                botManager.MoveBotFromReserve(botUsername);

                InsertTextIntoConsole(String.Format("Bot {0} re-added.", botUsername));
            }
            catch (Exception)
            {
                MessageBox.Show(this, String.Format("Cannot connect with the bot {0}!", botUsername), 
                    "Cannot connect.", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            moveToOnlineButton.Enabled = false;

            RefreshBotList();
        }

        private void addButton_Click(object sender, EventArgs e)
        {
            AddDialog dialog = new AddDialog(this);
            dialog.ShowDialog(this);
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutDialog dialog = new AboutDialog();
            dialog.ShowDialog(this);
        }

        private void optionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OptionsDialog dialog = new OptionsDialog();
            dialog.ShowDialog(this);

            SetOptions();
            BotRunnerOptions.SaveOptions();
        }
        #endregion
    }
}
