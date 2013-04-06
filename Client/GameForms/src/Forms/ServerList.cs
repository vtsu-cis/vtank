using System;
using System.Collections.Generic;
using System.Text;
using TomShane.Neoforce.Controls;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using GameForms.Controls;

namespace GameForms.Forms
{
    public class ServerList
    {
        Manager manager;
        Window window;
        Button refresh;
        Button back;
        Button play;
        ListView listview;

        String[] headers;
        List<String[]> data;

        public Window Window { get { return window; } }
        public List<String[]> Items { get { return listview.Items; } set { listview.Items = value; } }
        public Button Refresh { get { return refresh; } }
        public Button Back { get { return back; } }
        public Button Play { get { return play; } }
        public int SelectedIndex { get { return listview.ItemIndex; } set { listview.ItemIndex = value; } }

        public event TomShane.Neoforce.Controls.EventHandler SelectionChanged;

        public ServerList(Manager _manager, String[] _headers, List<String[]> _data)
        {
            manager = _manager;
            headers = _headers;
            data = _data;
            Init();
        }

        public void Init()
        {
            window = new Window(manager) ;
            window.Init();
            window.Text = "Server List";
            window.Width = 700;
            window.Height = 500;
            window.Center();
            window.Visible = true;
            window.CloseButtonVisible = false;

            play = new Button(manager);
            play.Init();
            play.Text = "Play";
            play.Width = 100;
            play.Height = 24;
            play.Left = window.Width - play.Width - 40;
            play.Top = window.Height - 80;
            play.Anchor = Anchors.Right;
            play.Parent = window;

            refresh = new Button(manager);
            refresh.Init();
            refresh.Text = "Refresh Server List";
            refresh.Width = 150;
            refresh.Height = 24;
            refresh.Left = 30;
            refresh.Top = window.Height - 130;
            refresh.Anchor = Anchors.Left;
            refresh.Parent = window;

            back = new Button(manager);
            back.Init();
            back.Text = "Back";
            back.Width = 75;
            back.Height = 24;
            back.Left = 30;
            back.Top = window.Height - 80;
            back.Anchor = Anchors.Left;
            back.Parent = window;

            listview = new ListView(manager, headers);
            listview.Init();
            listview.Width = 630;
            listview.Height = 300;
            listview.Left = 30;
            listview.Top = 40;
            listview.Anchor = Anchors.All;
            listview.HideSelection = false;
            listview.Parent = window;
            listview.Items = data;
            listview.ItemIndexChanged += new TomShane.Neoforce.Controls.EventHandler(listview_ItemIndexChanged);
        }

        void listview_ItemIndexChanged(object sender, TomShane.Neoforce.Controls.EventArgs e)
        {
            OnSelectionChanged(new TomShane.Neoforce.Controls.EventArgs());
        }

        protected virtual void OnSelectionChanged(TomShane.Neoforce.Controls.EventArgs e)
        {
            SelectionChanged.Invoke(this, e);
        }
    }
}
