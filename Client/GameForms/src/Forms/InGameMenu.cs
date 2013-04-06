using System;
using System.Collections.Generic;
using System.Text;
using TomShane.Neoforce.Controls;
using GameForms.Controls;
using Microsoft.Xna.Framework;

namespace GameForms.Forms
{
    public class InGameMenu
    {
        #region Members

        private Manager manager;
        private Window window;

        private Button resume;
        private Button logoff;
        private Button options;
        private Button exit;

        private Label title;

        public Window Window { get { return window; } }

        public Button Resume { get { return resume; } }
        public Button LogOff { get { return logoff; } }
        public Button Options { get { return options; } }
        public Button Exit { get { return exit; } }

        //public event TomShane.Neoforce.Controls.EventHandler EnterPressed;

        #endregion

        public InGameMenu(Manager _manager)
        {
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
            window.Height = 400;
            window.Center();
            window.Visible = true;
            window.CloseButtonVisible = false;
            // Create Button control and set the previous window as its parent.


            title = new Label(manager);
            title.Init();
            title.Text = "VTank";
            title.Width = 260;
            title.Height = 24;
            title.Left = 20;
            title.Top = 40;
            title.Anchor = Anchors.Top;
            title.Parent = window;

            resume = new Button(manager);
            resume.Init();
            resume.Text = "Resume";
            resume.Width = 200;
            resume.Height = 35;
            resume.Left = title.Left + 30;
            resume.Top = title.Top + title.Height + 20;
            resume.Anchor = Anchors.Top;
            resume.Parent = window;

            logoff = new Button(manager);
            logoff.Init();
            logoff.Text = "Log Off";
            logoff.Width = 200;
            logoff.Height = 35;
            logoff.Left = title.Left + 30;
            logoff.Top = resume.Top + resume.Height + 20;
            logoff.Anchor = Anchors.Top;
            logoff.Parent = window;

            options = new Button(manager);
            options.Init();
            options.Text = "Resume";
            options.Width = 200;
            options.Height = 35;
            options.Left = title.Left + 30;
            options.Top = logoff.Top + logoff.Height + 20;
            options.Anchor = Anchors.Top;
            options.Parent = window;

            exit = new Button(manager);
            exit.Init();
            exit.Text = "Resume";
            exit.Width = 200;
            exit.Height = 35;
            exit.Left = title.Left + 30;
            exit.Top = options.Top + options.Height + 20;
            exit.Anchor = Anchors.Top;
            exit.Parent = window;
        }

        //void Txt_KeyPress(object sender, KeyEventArgs e)
        //{
        //    if (e.Key == Microsoft.Xna.Framework.Input.Keys.Enter)
        //        OnEnterPressed(new TomShane.Neoforce.Controls.EventArgs());
        //}

        //protected virtual void OnEnterPressed(TomShane.Neoforce.Controls.EventArgs e)
        //{
        //    EnterPressed.Invoke(this, e);
        //}

    }
}
