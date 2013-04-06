using System;
using System.Collections.Generic;
using System.Text;
using TomShane.Neoforce.Controls;
using System.Threading;

namespace GameForms.Controls
{
    public class MessageBox : Control
    {
        Window window;
        public List<Button> buttons;
        Label label;
        ImageBox box;
        MessageBoxType Type;
        String message = "";
        String title = "";
        public Window Window { get { return window; } }

        public enum MessageBoxType
        {
            OKAY,
            YES_NO,
            ERROR,
            YES_NO_CANCEL
        }

        public MessageBox(Manager manager, MessageBoxType type)
            : base(manager)
        {
            Type = type;
            buttons = new List<Button>();
            Init();
            manager.Add(window);
            Pop();
        }

        public MessageBox(Manager manager, MessageBoxType type, String _message)
            : base(manager)
        {
            Type = type;
            buttons = new List<Button>();
            message = _message;
            Init();
            manager.Add(window);
            Pop();
        }

        public MessageBox(Manager manager, MessageBoxType type, String _message, String _title)
            : base(manager)
        {
            Type = type;
            buttons = new List<Button>();
            message = _message;
            title = _title;
            Init();
            manager.Add(window);
            Pop();
        }

        public override void Init()
        {
            base.Init();
            window = new Window(Manager);
            window.Init();
            window.Text = title;
            window.Width = 375;
            window.Height = 200;
            window.Center();
            window.Visible = true;
            
            box = new ImageBox(Manager);
            box.Init();
            box.Height = 32;
            box.Width = 32;
            
            box.Top = 100-40;
            box.Left = 20;
            box.SizeMode = SizeMode.Stretched;
            box.Parent = window;

            label = new Label(Manager);
            label.Init();
            label.Width = window.Width - 80;
            label.Height = 150;
            label.Top = 0;
            label.Left = box.Left + box.Width + 10;
            label.Text = FormatString(message);
            label.Parent = window;

            if (Type == MessageBoxType.OKAY)
            {
                buttons.Add(new Button(Manager));
                buttons[0].Init();
                buttons[0].Width = 50;
                buttons[0].Height = 24;
                buttons[0].Top = (int)(window.Height * .62);
                buttons[0].Left = (window.Width / 2) - (buttons[0].Width / 2);
                buttons[0].Text = "Okay";
                buttons[0].Parent = window;
                buttons[0].Click += new TomShane.Neoforce.Controls.EventHandler(OkayButtonClick);

                box.Image = Manager.Skin.Images["Icon.Information"].Resource;
            }
            else if (Type == MessageBoxType.ERROR)
            {
                buttons.Add(new Button(Manager));
                buttons[0].Init();
                buttons[0].Width = 50;
                buttons[0].Height = 24;
                buttons[0].Top = (int)(window.Height * .62);
                buttons[0].Left = (window.Width / 2) - (buttons[0].Width / 2);
                buttons[0].Text = "Okay";
                buttons[0].Parent = window;
                buttons[0].Click += new TomShane.Neoforce.Controls.EventHandler(OkayButtonClick);
                box.Image = Manager.Skin.Images["Icon.Error"].Resource;
            }
            else if (Type == MessageBoxType.YES_NO)
            {
                buttons.Add(new Button(Manager));
                buttons[0].Init();
                buttons[0].Width = 50;
                buttons[0].Height = 24;
                buttons[0].Top = (int)(window.Height * .62);
                buttons[0].Left = (window.Width / 2) + 10;
                buttons[0].Text = "No";
                buttons[0].Parent = window;
                buttons[0].Click += new TomShane.Neoforce.Controls.EventHandler(NoButtonClick);

                buttons.Add(new Button(Manager));
                buttons[1].Init();
                buttons[1].Width = 50;
                buttons[1].Height = 24;
                buttons[1].Top = buttons[0].Top;
                buttons[1].Left = (window.Width / 2) - (buttons[1].Width) - 10;
                buttons[1].Text = "Yes";
                buttons[1].Parent = window;
                buttons[1].Click += new TomShane.Neoforce.Controls.EventHandler(YesButtonClick);

                box.Image = Manager.Skin.Images["Icon.Question"].Resource;
            }
            else if (Type == MessageBoxType.YES_NO_CANCEL)
            {
                buttons.Add(new Button(Manager));
                buttons[0].Init();
                buttons[0].Width = 50;
                buttons[0].Height = 24;
                buttons[0].Text = "Yes";
                buttons[0].Top = (int)(window.Height * .62);
                buttons[0].Left = (window.Width / 2) - (buttons[0].Width) - 40;
                buttons[0].Parent = window;
                buttons[0].Click += new TomShane.Neoforce.Controls.EventHandler(YesButtonClick);

                buttons.Add(new Button(Manager));
                buttons[1].Init();
                buttons[1].Width = 50;
                buttons[1].Height = 24;
                buttons[1].Text = "No";
                buttons[1].Top = buttons[0].Top;
                buttons[1].Left = (window.Width / 2) - (buttons[1].Width /2);
                buttons[1].Parent = window;
                buttons[1].Click += new TomShane.Neoforce.Controls.EventHandler(NoButtonClick);

                buttons.Add(new Button(Manager));
                buttons[2].Init();
                buttons[2].Width = 50;
                buttons[2].Height = 24;
                buttons[2].Text = "Cancel";
                buttons[2].Top = buttons[0].Top;
                buttons[2].Left = (window.Width / 2) + 40;

                buttons[2].Parent = window;
                buttons[2].Click += new TomShane.Neoforce.Controls.EventHandler(CancelButtonClick);

                box.Image = Manager.Skin.Images["Icon.Question"].Resource;
            }

            foreach (Button btn in buttons)
            {
                window.Add(btn);
            }
        }

        public void Pop()
        {
            window.ShowModal();
        }

        void OkayButtonClick(object sender, TomShane.Neoforce.Controls.EventArgs e)
        {
            window.ModalResult = ModalResult.Ok;
            window.Close();
            MessageBoxManager.IsShown = false;
        }

        void YesButtonClick(object sender, TomShane.Neoforce.Controls.EventArgs e)
        {
            window.ModalResult = ModalResult.Yes;
            window.Close();
            MessageBoxManager.IsShown = false;
        }

        void NoButtonClick(object sender, TomShane.Neoforce.Controls.EventArgs e)
        {
            window.ModalResult = ModalResult.No;
            window.Close();
            MessageBoxManager.IsShown = false;
        }

        void CancelButtonClick(object sender, TomShane.Neoforce.Controls.EventArgs e)
        {
            window.ModalResult = ModalResult.Cancel;
            window.Close();
            MessageBoxManager.IsShown = false;
        }

        protected override void InitSkin()
        {
            base.InitSkin();
        }

        public String FormatString(String msg)
        {
            String s = "";
            String temp = "";
            int index = 40;
            int len = msg.Length;
            int passes = len / 40;

            for(int i = 0; i<passes;i++)
            {
                temp = msg.Substring(index*i, 40);

                s += temp + "\n";
            }

            s += msg.Substring(index * passes);

            return s;
        }

        public void WaitForClose()
        {
            while (Window.ModalResult == ModalResult.None)
            {
                Thread.Sleep(5);
            }
        }

        public static Window Show(Manager manager, MessageBoxType _type, String _message, String _title)
        {
            MessageBoxManager.IsShown = true;
            return new MessageBox(manager, _type, _message, _title).Window;
        }
    }

    public static class MessageBoxManager
    {
        public static bool IsShown = false;
    }
}
