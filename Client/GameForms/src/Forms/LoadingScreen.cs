using System;
using System.Collections.Generic;
using System.Text;
using TomShane.Neoforce.Controls;

namespace GameForms.Forms
{
    public class LoadingScreen
    {
        #region Members

        private Manager manager;
        private Window window;
        private Label text;
        private ProgressBar bar;
        private Button cancel;

        public Window Window { get { return window; } }
        public int Value { get { return bar.Value; } set { bar.Value = value; } }
        public String Message { get { return text.Text; } set { text.Text = value; } }
        public Button Cancel { get { return cancel; } }

        #endregion

        public LoadingScreen(Manager _manager)
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
            window.Text = "Please Wait";
            window.Width = 300;
            window.Height = 175;
            window.Center();
            window.Visible = true;
            window.CloseButtonVisible = false;

            text = new Label(manager);
            text.Init();
            text.Text = "";
            text.Width = 250;
            text.Height = 24;
            text.Left = 20;
            text.Top = 20;
            text.Anchor = Anchors.Left;
            text.Parent = window;

            bar = new ProgressBar(manager);
            bar.Init();
            bar.Width = 260;
            bar.Height = 15;
            bar.Left = 20;
            bar.Top = 60;
            bar.Anchor = Anchors.Left;
            bar.Parent = window;
            bar.Value = 0;
            bar.Mode = ProgressBarMode.Default;

            cancel = new Button(manager);
            cancel.Init();
            cancel.Left = window.Width - cancel.Width - 20;
            cancel.Top = 110;
            cancel.Text = "Cancel";
            cancel.Anchor = Anchors.Right;
            cancel.Parent = window;
        }
    }
}
