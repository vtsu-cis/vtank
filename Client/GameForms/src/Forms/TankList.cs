using System;
using System.Collections.Generic;
using System.Text;
using TomShane.Neoforce.Controls;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GameForms.Forms
{
    public class TankList
    {
        #region Members
        private Manager manager;
        private Window window;
        private Button create;
        private Button edit;
        private Button delete;
        private Button play;
        private Button back;

        private ListBox listbox;
        private ImageBox image;
        private ProgressBar armorBar;
        private ProgressBar speedBar;
        private Label rankLbl;
        private Label armorLbl;
        private Label speedLbl;

        private Texture2D texture;
        #endregion

        #region Properties
        public Window Window { get { return window; } }
        public Button CreateButton { get { return create; } }
        public Button EditButton { get { return edit; } }
        public Button DeleteButton { get { return delete; } }
        public Button PlayButton { get { return play; } }
        public Button BackButton { get { return back; } }
        public ProgressBar RankProgressBar { get; set; }
        public ImageBox RankImage { get; set; }
        public Label RankLabel { get { return rankLbl; } set { rankLbl = value; } }
        public Label RankNextLabel { get; set; }
        public List<object> List { get { return listbox.Items; } set { SetItems(value); } }
        public Texture2D Image { get { return image.Image; } set { image.Image = value; } }
        public Color ImageColor { get { return image.Color; } set { image.Color = value; } }
        public int SelectedIndex { get { return listbox.ItemIndex; } set { listbox.ItemIndex = value; } }
        public int Armor { get { return armorBar.Value; } set { armorBar.Value = value; } }
        public int Speed { get { return speedBar.Value; } set { speedBar.Value = value; } }

        public event TomShane.Neoforce.Controls.EventHandler SelectionChanged;
        #endregion
        
        public TankList(Manager _manager)
        {
            manager = _manager;
            Init();
        }

        #region Methods
        protected virtual void OnSelectionChanged(TomShane.Neoforce.Controls.EventArgs e)
        {
            SelectionChanged.Invoke(this, e);
        }

        private void SetItems(List<object> value)
        {
            listbox.Items.Clear();
            listbox.Items.AddRange(value);
        }

        public void Init()
        {
            texture = new Texture2D(manager.GraphicsDevice, 1, 1);
            texture.SetData<Color>(new Color[] { Color.White });

            window = new Window(manager) ;
            window.Init();
            window.Text = "Tank List";
            window.Width = 600;
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
            play.Top = window.Height - 90;
            play.Anchor = Anchors.Right;
            play.Parent = window;

            create = new Button(manager);
            create.Init();
            create.Text = "Create New Tank";
            create.Width = 150;
            create.Height = 24;
            create.Left = 30;
            create.Top = window.Height - 160;
            create.Anchor = Anchors.Left;
            create.Parent = window;

            edit = new Button(manager);
            edit.Init();
            edit.Text = "Edit Tank";
            edit.Width = 100;
            edit.Height = 24;
            edit.Left = 30;
            edit.Top = window.Height - 125;
            edit.Anchor = Anchors.Left;
            edit.Parent = window;

            delete = new Button(manager);
            delete.Init();
            delete.Text = "Delete Tank";
            delete.Width = 100;
            delete.Height = 24;
            delete.Left = edit.Left + edit.Width + 30;
            delete.Top = edit.Top;
            delete.Anchor = Anchors.Left;
            delete.Parent = window;

            back = new Button(manager);
            back.Init();
            back.Text = "Back";
            back.Width = 150;
            back.Height = 24;
            back.Left = 30;
            back.Top = play.Top;
            back.Anchor = Anchors.Left;
            back.Parent = window;

            listbox = new ListBox(manager);
            listbox.Init();
            listbox.Width = 225;
            listbox.Height = 275;
            listbox.Left = 30;
            listbox.Top = 40;
            listbox.Anchor = Anchors.Left;
            listbox.HideSelection = false;
            listbox.Parent = window;
            listbox.ItemIndexChanged += new TomShane.Neoforce.Controls.EventHandler(listbox_ItemIndexChanged);

            image = new ImageBox(manager);
            image.Init();
            image.Image = texture;
            image.SizeMode = SizeMode.Stretched;
            image.Width = 200;
            image.Height = 150;
            image.Left = window.Width - image.Width - 70;
            image.Top = 40;
            image.Anchor = Anchors.Right;
            image.Parent = window;

            RankImage = new ImageBox(manager);
            RankImage.Init();
            //RankImage.Image = null;
            RankImage.SizeMode = SizeMode.Stretched;
            RankImage.Width = 32;
            RankImage.Height = 32;
            RankImage.Top = window.Height - 280;
            RankImage.Left = window.Width - RankImage.Width - 240;
            RankImage.Anchor = Anchors.Left;
            RankImage.Parent = window;

            rankLbl = new Label(manager);
            rankLbl.Init();
            rankLbl.Text = "Unknown";
            rankLbl.Top = window.Height - 275;
            rankLbl.Anchor = Anchors.Left;
            rankLbl.Width = 232 - RankImage.Width;
            rankLbl.Left = window.Width - rankLbl.Width - RankImage.Width;
            rankLbl.Height = 24;
            rankLbl.Parent = window;

            RankProgressBar = new ProgressBar(manager);
            RankProgressBar.Init();
            RankProgressBar.Value = 0;
            RankProgressBar.Top = window.Height - 243;
            RankProgressBar.Anchor = Anchors.Left;
            RankProgressBar.Width = 200;
            RankProgressBar.Left = window.Width - rankLbl.Width - 69;
            RankProgressBar.Height = 25;
            RankProgressBar.ToolTip = new ToolTip(manager);
            RankProgressBar.ToolTip.Text = "0 / 0";
            RankProgressBar.Parent = window;

            RankNextLabel = new Label(manager);
            RankNextLabel.Init();
            RankNextLabel.Top = window.Height - 215;
            RankNextLabel.Width = 200;
            RankNextLabel.Height = 15;
            RankNextLabel.Left = window.Width - RankImage.Width - 235;
            RankNextLabel.Text = "Progress is loading...";
            RankNextLabel.Parent = window;

            armorBar = new ProgressBar(manager);
            armorBar.Init();
            armorBar.Width = 150;
            armorBar.Height = 24;
            armorBar.Left = window.Width - armorBar.Width - 50; ;
            armorBar.Top = window.Height - 180;
            armorBar.Anchor = Anchors.Left;
            armorBar.Parent = window;
            armorBar.Value = 50;

            armorLbl = new Label(manager);
            armorLbl.Init();
            armorLbl.Text = "Armor: ";
            armorLbl.Width = 50;
            armorLbl.Height = 24;
            armorLbl.Left = armorBar.Left - armorLbl.Width - 20;
            armorLbl.Top = armorBar.Top;
            armorLbl.Anchor = Anchors.Left;
            armorLbl.Parent = window;

            speedBar = new ProgressBar(manager);
            speedBar.Init();
            speedBar.Width = 150;
            speedBar.Height = 24;
            speedBar.Left = window.Width - speedBar.Width - 50; ;
            speedBar.Top = window.Height - 150;
            speedBar.Anchor = Anchors.Left;
            speedBar.Parent = window;
            speedBar.Value = 50;

            speedLbl = new Label(manager);
            speedLbl.Init();
            speedLbl.Text = "Speed: ";
            speedLbl.Width = 50;
            speedLbl.Height = 24;
            speedLbl.Left = speedBar.Left - speedLbl.Width - 20;
            speedLbl.Top = speedBar.Top;
            speedLbl.Anchor = Anchors.Left;
            speedLbl.Parent = window;
        }

        void listbox_ItemIndexChanged(object sender, TomShane.Neoforce.Controls.EventArgs e)
        {
            OnSelectionChanged(new TomShane.Neoforce.Controls.EventArgs());
        }
        #endregion
    }
}
