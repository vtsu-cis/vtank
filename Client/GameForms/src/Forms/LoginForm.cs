using System;
using System.Collections.Generic;
using System.Text;
using TomShane.Neoforce.Controls;
using GameForms.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameForms.Forms
{
    public class LoginForm
    {
        #region Members
        public static string username = null;
        private Manager manager;
        private Window window;
        private Button login;
        private Button exit;
        private Label unLbl;
        private Label pwLbl;
        private TextBox unTxt;
        private TextBox pwTxt;
        private CursorChangeArea regArea;
        private CursorChangeArea passArea;
        public Window Window { get { return window; } }
        public Button Login { get { return login; } }
        public Button Exit { get { return exit; } }
        public String Username { get { return unTxt.Text; } }
        public String Password { get { return pwTxt.Text; } }

        public event TomShane.Neoforce.Controls.EventHandler EnterPressed;

        #endregion

        public LoginForm(Manager _manager)
        {
            LoginForm.username = null;
            manager = _manager;
            Init();
        }

        ////////////////////////////////////////////////////////////////////////////

        public void Init()
        {
            // Create and setup Window control.
            window = new Window(manager);
            window.Init();
            window.Text = "Login";
            window.Width = 300;
            window.Height = 225;
            window.Center();
            window.Visible = true;
            window.CloseButtonVisible = false;

            unLbl = new Label(manager);
            unLbl.Init();
            unLbl.Text = "User Name: ";
            unLbl.Width = 72;
            unLbl.Height = 24;
            unLbl.Left = 40;
            unLbl.Top = 40;
            unLbl.Anchor = Anchors.Left;
            unLbl.Parent = window;

            pwLbl = new Label(manager);
            pwLbl.Init();
            pwLbl.Text = "Password: ";
            pwLbl.Width = 72;
            pwLbl.Height = 24;
            pwLbl.Left = 40;
            pwLbl.Top = 70;
            pwLbl.Anchor = Anchors.Left;
            pwLbl.Parent = window;

            unTxt = new TextBox(manager);
            unTxt.Init();
            unTxt.Text = "";
            unTxt.Width = 124;
            unTxt.Height = 24;
            unTxt.Left = 125;
            unTxt.Top = 40;
            unTxt.Anchor = Anchors.Right;
            unTxt.Parent = window;
            unTxt.KeyPress += new KeyEventHandler(Txt_KeyPress);

            pwTxt = new TextBox(manager);
            pwTxt.Init();
            pwTxt.Text = "";
            pwTxt.Width = 124;
            pwTxt.Height = 24;
            pwTxt.Left = 125;
            pwTxt.Top = 70;
            pwTxt.Anchor = Anchors.Right;
            pwTxt.Mode = TextBoxMode.Password;
            pwTxt.Parent = window;
            pwTxt.KeyPress += new KeyEventHandler(Txt_KeyPress);

            login = new Button(manager);
            login.Init();
            login.Text = "Login";
            login.Width = 80;
            login.Height = 24;
            login.Left = window.ClientWidth - pwTxt.Width + 7;
            login.Top = window.ClientHeight - login.Height - 50;
            login.Anchor = Anchors.Top;
            login.Parent = window;

            exit = new Button(manager);
            exit.Init();
            exit.Text = "Exit";
            exit.Width = 80;
            exit.Height = 24;
            exit.Left = unLbl.Left;
            exit.Top = login.Top;
            exit.Anchor = Anchors.Top;
            exit.Parent = window;

            regArea = new CursorChangeArea(manager);
            regArea.Init();
            regArea.Text = "Register";
            regArea.Width = 58;
            regArea.Height = 16;
            regArea.Color = new Color(80, 80, 80, 255);
            regArea.TextColor = Color.LightBlue;
            regArea.Left = exit.Left;
            regArea.Top = exit.Top + exit.Height + 15;
            regArea.Anchor = Anchors.Left;
            regArea.Parent = window;
            regArea.MousePress += new MouseEventHandler(reg_MousePress);
            regArea.ToolTip.Text = "Visit VTank registration site";

            passArea = new CursorChangeArea(manager);
            passArea.Init();
            passArea.Text = "Forgot password?";
            passArea.Width = 110;
            passArea.Height = 16;
            passArea.Color = new Color(80, 80, 80, 255);
            passArea.TextColor = Color.LightBlue;
            passArea.Left = login.Left + login.Width - passArea.Width;
            passArea.Top = exit.Top + exit.Height + 15;
            passArea.Anchor = Anchors.Left;
            passArea.Parent = window;
            passArea.MousePress += new MouseEventHandler(pass_MousePress);
            passArea.ToolTip.Text = "Visit password recovery site";
        }

        void reg_MousePress(object sender, MouseEventArgs e)
        {
            string targetURL = @"http://vtank.cis.vtc.edu/community/community/accounts/register/";
            System.Diagnostics.Process.Start(targetURL);
        }

        void pass_MousePress(object sender, MouseEventArgs e)
        {
            string targetURL = @"http://vtank.cis.vtc.edu/community/community/accounts/forgot/";
            System.Diagnostics.Process.Start(targetURL);
        }

        void Txt_KeyPress(object sender, KeyEventArgs e)
        {
            if (e.Key == Microsoft.Xna.Framework.Input.Keys.Enter)
                OnEnterPressed(new TomShane.Neoforce.Controls.EventArgs());
            LoginForm.username = Username;
        }

        protected virtual void OnEnterPressed(TomShane.Neoforce.Controls.EventArgs e)
        {
            EnterPressed.Invoke(this, e);
            LoginForm.username = Username;
        }
    }
}
