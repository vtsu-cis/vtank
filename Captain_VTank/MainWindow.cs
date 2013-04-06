using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Captain_VTank.Service;
using Captain_VTank.Network;
using System.Threading;
using Captain_VTank.Util;

namespace Captain_VTank
{
    public partial class MainWindow : Form
    {
        #region Member
        private Thread refreshThread;
        private delegate void DoRefreshAction();
        #endregion

        #region Constructor
        public MainWindow()
        {
            InitializeComponent();

            this.FormClosing += new FormClosingEventHandler(MainWindow_FormClosing);

            CreateContextMenuStripForUserTable();

            refreshThread = new Thread(new ThreadStart(DoRefresh));
            refreshThread.Start();
        }
        #endregion

        #region Methods
        private void CreateContextMenuStripForUserTable()
        {
            ContextMenu menu = new ContextMenu();

            MenuItem viewAccountInfoItem = new MenuItem("More Info...", (object sender, EventArgs e) =>
            {
                // View more information about the given user.
                DataGridViewSelectedRowCollection rows = accountsTable.SelectedRows;
                DataGridViewRow selectedRow = rows[0];
                string username = selectedRow.Cells[0].Value.ToString();

                Admin.Account account = NetworkManager.GetProxy().GetAccountByName(username);
                
            });

            MenuItem kickItem = new MenuItem("Kick", (object sender, EventArgs e) =>
            {
                // Kick the selected player.
                DataGridViewSelectedRowCollection rows = accountsTable.SelectedRows;
                DataGridViewRow selectedRow = rows[0];
                string username = selectedRow.Cells[0].Value.ToString();

                string kickReason = GetInput("Please enter a reason: ", "Kick");
                if (kickReason == null)
                {
                    // Cancelled.
                    return;
                }

                NetworkManager.GetProxy().KickUserByAccountName(username, kickReason);

                RefreshOnlineUserList();
            });
            kickItem.Enabled = false;
            menu.MenuItems.Add(kickItem);

            MenuItem banItem = new MenuItem("Ban", (object sender, EventArgs e) =>
            {
                // Ban the selected player.
                DataGridViewSelectedRowCollection rows = accountsTable.SelectedRows;
                DataGridViewRow selectedRow = rows[0];
                string username = selectedRow.Cells[0].Value.ToString();

                string banReason = GetInput("Please enter a reason: ", "Ban");
                if (banReason == null)
                {
                    // Cancelled.
                    return;
                }
                
                NetworkManager.GetProxy().BanUserByAccountName(username, banReason);

                RefreshOnlineUserList();
            });
            menu.MenuItems.Add(banItem);

            accountsTable.ContextMenu = menu;
            accountsTable.Click += new EventHandler((object sender, EventArgs e) =>
            {
                if (e is MouseEventArgs)
                {
                    // De-select all rows.
                    foreach (DataGridViewRow row in accountsTable.Rows)
                    {
                        row.Selected = false;
                    }

                    MouseEventArgs mouseEvent = (MouseEventArgs)e;
                    DataGridView.HitTestInfo info = accountsTable.HitTest(
                        mouseEvent.X, mouseEvent.Y);
                    
                    if (mouseEvent.Button == MouseButtons.Right)
                    {
                        // Check where it was right-clicked.
                        DataGridViewRow row = accountsTable.Rows[info.RowIndex];
                        row.Selected = true;

                        kickItem.Enabled = row.DefaultCellStyle.BackColor != Color.LightGray;

                        // Show the context menu.
                        menu.Show(accountsTable, mouseEvent.Location);
                    }
                    else if (mouseEvent.Button == MouseButtons.Left)
                    {
                        accountsTable.Rows[info.RowIndex].Selected = true;
                    }
                }
            });
            accountsTable.MultiSelect = false;
        }

        private string GetInput(string message)
        {
            return GetInput(message, "Please enter more details.");
        }

        private string GetInput(string message, string title)
        {
            InputDialog dialog = new InputDialog();
            dialog.Message = message;
            dialog.TitleText = title;
            DialogResult result = dialog.ShowDialog(this);
            if (result == DialogResult.OK)
            {
                return dialog.InputText;
            }
            
            return null;
        }

        private void DoRefresh()
        {
            Thread.Sleep(25);
            Invoke(new DoRefreshAction(RefreshOnlineUserList));
            refreshThread = null;
        }
        
        public void RefreshOnlineUserList()
        {
            accountsTable.Rows.Clear();

            Admin.OnlineUser[] users = NetworkManager.GetProxy().GetFullUserList();
            foreach (Admin.OnlineUser user in users)
            {
                string role = RoleUtil.GetRole(user.userlevel).ToString();
                
                accountsTable.Rows.Add(user.username, user.playingGame ? "Yes" : "No", role, user.clientType);
                if (user.clientType == "Offline")
                {
                    accountsTable.Rows[accountsTable.Rows.Count - 1].DefaultCellStyle.BackColor =
                        Color.LightGray;
                }
            }
        }

        public void RefreshRecentUsersList(Admin.Account[] accounts)
        {
            if (accounts == null || accounts.Length == 0)
                accounts = NetworkManager.GetProxy().GetAccountList();
            recentUsersTable.Rows.Clear();

            foreach (Admin.Account account in accounts)
            {
                recentUsersTable.Rows.Add(account.accountName, 
                    account.lastLoggedIn, account.creationDate);
            }
        }

        public void RefreshTopPlayerList(Admin.Account[] accounts)
        {
            if (accounts == null || accounts.Length == 0)
                accounts = NetworkManager.GetProxy().GetAccountList();

            topPlayersTable.Rows.Clear();

            foreach (Admin.Account account in accounts)
            {
                topPlayersTable.Rows.Add(account.accountName, account.points, "< Unknown >");
            }
        }
        #endregion

        #region Event Handlers
        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Network.NetworkManager.Shutdown();
            this.Visible = false;
            this.Close();
            this.Dispose();

            Services.LoginWindow.Close();
        }

        private void refreshButton_Click(object sender, EventArgs e)
        {
            RefreshOnlineUserList();
        }

        void MainWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            Services.Shutdown();
        }

        private void MainWindow_Load(object sender, EventArgs e)
        {
            /*Admin.Account[] accounts = NetworkManager.GetProxy().GetAccountList();
            RefreshTopPlayerList(accounts);
            RefreshRecentUsersList(accounts);*/
        }

        private void topPlayersRefresh_Click(object sender, EventArgs e)
        {
            RefreshTopPlayerList(null);
        }
        #endregion

        
    }
}
