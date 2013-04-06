using System;
using System.Collections.Generic;
using System.Text;
using TomShane.Neoforce.Controls;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using GameForms.Controls;
using Microsoft.Xna.Framework;

namespace GameForms.Forms
{
    public class TankCreation
    {
        Manager manager;

        Window window;
        Button create;
        Button cancel;

        Label tankBoxLbl;
        ListBox tankBox;

        Label turretBoxLbl;
        ListBox turretBox;
        ProgressBar tPower;
        ProgressBar tRate;
        ProgressBar tRange;
        Label tPowerLbl;
        Label tRateLbl;
        Label tRangeLbl;

        ImageBox image;

        ProgressBar armorBar;
        ProgressBar speedBar;
        SliderBar ratioSlide;
        Label armorLbl;
        Label speedLbl;
        Label ratioLbl;

        SliderBar red;
        SliderBar green;
        SliderBar blue;
        Label redLbl;
        Label greenLbl;
        Label blueLbl;
        Color tankColor;

        Label name;
        TextBox nameBox;

        Texture2D texture;

        public event TomShane.Neoforce.Controls.EventHandler TankSelectionChanged;
        public event TomShane.Neoforce.Controls.EventHandler TurretSelectionChanged;

        public Window Window { get { return window; } }
        public Button Cancel { get { return cancel; } }
        public Button Create { get { return create; } }
        public List<object> Tanks { get { return tankBox.Items; } set { SetItems(tankBox, value); } }
        public List<object> Turrets { get { return turretBox.Items; } set { SetItems(turretBox, value); } }
        public int TankIndex { get { return tankBox.ItemIndex; } set { tankBox.ItemIndex = value; } }
        public int TurretIndex { get { return turretBox.ItemIndex; } set { turretBox.ItemIndex = value; } }
        public int RatioValue { get { return ratioSlide.Value; } set { ratioSlide.Value = value; } }
        public int TurretPower { get { return tPower.Value; } set { tPower.Value = value; } }
        public int TurretRate { get { return tRate.Value; } set { tRate.Value = value; } }
        public int TurretRange { get { return tRange.Value; } set { tRange.Value = value; } }
        public int Armor { get { return armorBar.Value; } set { armorBar.Value = value; } }
        public int Speed { get { return speedBar.Value; } set { speedBar.Value = value; } }
        public Color TankColor { get { return tankColor; } set { tankColor = value; SetColor(); } }
        public bool NameEditable { get { return nameBox.Enabled; } set { nameBox.Enabled = value; } }
        public String TankName { get { return nameBox.Text; } set { nameBox.Text = value; } }
        public Texture2D Image { get { return image.Image; } set { image.Image = value; } }

        public String TankSkin 
        {
            get
            {
                return SkinLabel.Text;
            }

            set
            {
                SkinLabel.Text = value;
            }
        }

        public Button ScrollSkinLeft { get; set; }
        public Button ScrollSkinRight { get; set; }
        public Label SkinLabel { get; set; }

        private void SetItems(ListBox box, List<object> values)
        {
            box.Items.Clear();
            foreach (object obj in values)
            {
                box.Items.Add(obj);
            }
        }
        
        String[] tanks = new String[] { };
        public TankCreation(Manager _manager)
        {
            manager = _manager;
            Init();
        }

        public void Init()
        {
            texture = new Texture2D(manager.GraphicsDevice, 1, 1);
            texture.SetData<Color>(new Color[] { Color.White });

            #region Window
            window = new Window(manager) ;
            window.Init();
            window.Text = "Tank Creation";
            window.Width = 600;
            window.Height = 500;
            window.Center();
            window.Visible = true;
            window.CloseButtonVisible = false;
            #endregion

            #region ListBoxes

            #region Tank
            tankBoxLbl = new Label(manager);
            tankBoxLbl.Init();
            tankBoxLbl.Text = "Tanks: ";
            tankBoxLbl.Width = 100;
            tankBoxLbl.Height = 10;
            tankBoxLbl.Left = 20;
            tankBoxLbl.Top = 10;
            tankBoxLbl.Anchor = Anchors.Left;
            tankBoxLbl.Parent = window;

            tankBox = new ListBox(manager);
            tankBox.Init();
            tankBox.Width = 200;
            tankBox.Height = 75;
            tankBox.Left = 30;
            tankBox.Top = 30;
            tankBox.Anchor = Anchors.Left;
            tankBox.HideSelection = false;
            tankBox.Parent = window;
            foreach (String s in tanks)
            {
                tankBox.Items.Add(s);
            }
            tankBox.ItemIndexChanged += new TomShane.Neoforce.Controls.EventHandler(tankBox_ItemIndexChanged);
            #endregion

            #region Turret

            turretBoxLbl = new Label(manager);
            turretBoxLbl.Init();
            turretBoxLbl.Text = "Turrets: ";
            turretBoxLbl.Width = 50;
            turretBoxLbl.Height = 10;
            turretBoxLbl.Left = tankBoxLbl.Left;
            turretBoxLbl.Top = tankBox.Top + tankBox.Height + 20;
            turretBoxLbl.Anchor = Anchors.Left;
            turretBoxLbl.Parent = window;

            turretBox = new ListBox(manager);
            turretBox.Init();
            turretBox.Width = 200;
            turretBox.Height = 100;
            turretBox.Left = 30;
            turretBox.Top = turretBoxLbl.Top + turretBoxLbl.Height + 10;
            turretBox.Anchor = Anchors.Left;
            turretBox.HideSelection = false;
            turretBox.Parent = window;
            foreach (String s in tanks)
            {
                turretBox.Items.Add(s);
            }
            turretBox.ItemIndexChanged += new TomShane.Neoforce.Controls.EventHandler(turretBox_ItemIndexChanged);

            tPowerLbl = new Label(manager);
            tPowerLbl.Init();
            tPowerLbl.Text = "Power: ";
            tPowerLbl.Width = 50;
            tPowerLbl.Height = 10;
            tPowerLbl.Left = turretBox.Left;
            tPowerLbl.Top = turretBox.Top + turretBox.Height + 10;
            tPowerLbl.Anchor = Anchors.Left;
            tPowerLbl.Parent = window;

            tPower = new ProgressBar(manager);
            tPower.Init();
            tPower.Width = 75;
            tPower.Height = 10;
            tPower.Left = tPowerLbl.Left + tPowerLbl.Width + 5;
            tPower.Top = tPowerLbl.Top;
            tPower.Anchor = Anchors.Left;
            tPower.Parent = window;
            tPower.Value = 50;

            tRateLbl = new Label(manager);
            tRateLbl.Init();
            tRateLbl.Text = "Rate: ";
            tRateLbl.Width = 50;
            tRateLbl.Height = 10;
            tRateLbl.Left = turretBox.Left;
            tRateLbl.Top = tPowerLbl.Top + tPowerLbl.Height;
            tRateLbl.Anchor = Anchors.Left;
            tRateLbl.Parent = window;

            tRate = new ProgressBar(manager);
            tRate.Init();
            tRate.Width = 75;
            tRate.Height = 10;
            tRate.Left = tRateLbl.Left + tRateLbl.Width + 5;
            tRate.Top = tRateLbl.Top;
            tRate.Anchor = Anchors.Left;
            tRate.Parent = window;
            tRate.Value = 50;

            tRangeLbl = new Label(manager);
            tRangeLbl.Init();
            tRangeLbl.Text = "Range: ";
            tRangeLbl.Width = 50;
            tRangeLbl.Height = 15;
            tRangeLbl.Left = turretBox.Left;
            tRangeLbl.Top = tRateLbl.Top + tRateLbl.Height;
            tRangeLbl.Anchor = Anchors.Left;
            tRangeLbl.Parent = window;

            tRange = new ProgressBar(manager);
            tRange.Init();
            tRange.Width = 75;
            tRange.Height = 10;
            tRange.Left = tRangeLbl.Left + tRangeLbl.Width + 5;
            tRange.Top = tRangeLbl.Top;
            tRange.Anchor = Anchors.Left;
            tRange.Parent = window;
            tRange.Value = 50;


            #endregion

            #endregion

            #region TankStats

            armorLbl = new Label(manager);
            armorLbl.Init();
            armorLbl.Text = "Armor: ";
            armorLbl.Width = 50;
            armorLbl.Height = 24;
            armorLbl.Left = 20;
            armorLbl.Top = window.Height - 160; ;
            armorLbl.Anchor = Anchors.Left;
            armorLbl.Parent = window;

            armorBar = new ProgressBar(manager);
            armorBar.Init();
            armorBar.Width = 150;
            armorBar.Height = 24;
            armorBar.Left = armorLbl.Left + armorLbl.Width + 20; ;
            armorBar.Top = armorLbl.Top;
            armorBar.Anchor = Anchors.Left;
            armorBar.Parent = window;
            armorBar.Value = 50;

            speedLbl = new Label(manager);
            speedLbl.Init();
            speedLbl.Text = "Speed: ";
            speedLbl.Width = 50;
            speedLbl.Height = 24;
            speedLbl.Left = armorLbl.Left;
            speedLbl.Top = armorLbl.Top + armorLbl.Height;
            speedLbl.Anchor = Anchors.Left;
            speedLbl.Parent = window;

            speedBar = new ProgressBar(manager);
            speedBar.Init();
            speedBar.Width = 150;
            speedBar.Height = 24;
            speedBar.Left = speedLbl.Left + speedLbl.Width + 20; ;
            speedBar.Top = speedLbl.Top;
            speedBar.Anchor = Anchors.Left;
            speedBar.Parent = window;
            speedBar.Value = 50;
            
            ratioLbl = new Label(manager);
            ratioLbl.Init();
            ratioLbl.Text = "Ratio: ";
            ratioLbl.Width = 50;
            ratioLbl.Height = 24;
            ratioLbl.Left = armorLbl.Left;
            ratioLbl.Top = speedLbl.Top + speedLbl.Height + 20;
            ratioLbl.Anchor = Anchors.Left;
            ratioLbl.Parent = window;

            ratioSlide = new SliderBar(manager);
            ratioSlide.Init();
            ratioSlide.Width = 150;
            ratioSlide.Height = 20;
            ratioSlide.Left = ratioLbl.Left + ratioLbl.Width + 20; ;
            ratioSlide.Top = ratioLbl.Top + 3;
            ratioSlide.Anchor = Anchors.Left;
            ratioSlide.Parent = window;
            ratioSlide.Value = 50;
            ratioSlide.ValueChanged += new TomShane.Neoforce.Controls.EventHandler(ratioSlide_ValueChanged);

            #endregion

            #region Render Region
            image = new ImageBox(manager);
            image.Init();
            image.SizeMode = SizeMode.Stretched;
            image.Width = 200;
            image.Height = 150;
            image.Left = window.Width - image.Width - 70;
            image.Top = 40;
            image.Anchor = Anchors.Right;
            image.Parent = window;
            image.Image = texture;
            #endregion

            #region Skin Scrolling (Left/Right)
            ScrollSkinLeft = new Button(manager);
            ScrollSkinLeft.Init();
            ScrollSkinLeft.Width = 25;
            ScrollSkinLeft.Height = 25;
            ScrollSkinLeft.Left = window.Width - image.Width - 70;
            ScrollSkinLeft.Top = image.Top + image.Height + 10;
            ScrollSkinLeft.Text = " < ";
            ScrollSkinLeft.Parent = window;

            ScrollSkinRight = new Button(manager);
            ScrollSkinRight.Init();
            ScrollSkinRight.Width = 25;
            ScrollSkinRight.Height = 25;
            ScrollSkinRight.Left = image.Left + image.Width - ScrollSkinRight.Width;
            ScrollSkinRight.Top = image.Top + image.Height + 10;
            ScrollSkinRight.Text = " > ";
            ScrollSkinRight.Parent = window;
            #endregion

            #region Skin Label
            SkinLabel = new Label(manager);
            SkinLabel.Init();
            SkinLabel.Alignment = Alignment.MiddleCenter;
            SkinLabel.Height = 15;
            SkinLabel.Width = 100;
            SkinLabel.Left = ScrollSkinRight.Left - 120;
            SkinLabel.Top = image.Top + image.Width - 33;
            SkinLabel.Text = "Skin";
            SkinLabel.Parent = window;
            #endregion

            #region Color Choosing
            redLbl = new Label(manager);
            redLbl.Init();
            redLbl.Text = "Red: ";
            redLbl.Width = 50;
            redLbl.Height = 24;
            redLbl.Left = image.Left;
            redLbl.Top = ScrollSkinLeft.Top + ScrollSkinLeft.Height + 20;
            redLbl.Anchor = Anchors.Left;
            redLbl.Parent = window;

            red = new SliderBar(manager);
            red.Init();
            red.Width = 120;
            red.Height = 15;
            red.Left = redLbl.Left + redLbl.Width + 20;
            red.Top = ScrollSkinLeft.Top + ScrollSkinLeft.Height + 25;
            red.Anchor = Anchors.Left;
            red.Parent = window;
            red.Value = 50;
            red.ValueChanged += new TomShane.Neoforce.Controls.EventHandler(Color_Changed);

            greenLbl = new Label(manager);
            greenLbl.Init();
            greenLbl.Text = "Green: ";
            greenLbl.Width = 50;
            greenLbl.Height = 24;
            greenLbl.Left = redLbl.Left;
            greenLbl.Top = redLbl.Top + redLbl.Height;
            greenLbl.Anchor = Anchors.Left;
            greenLbl.Parent = window;

            green = new SliderBar(manager);
            green.Init();
            green.Width = red.Width;
            green.Height = red.Height;
            green.Left = greenLbl.Left + greenLbl.Width + 20;
            green.Top = greenLbl.Top + 5;
            green.Anchor = Anchors.Left;
            green.Parent = window;
            green.Value = 50;
            green.ValueChanged += new TomShane.Neoforce.Controls.EventHandler(Color_Changed);

            blueLbl = new Label(manager);
            blueLbl.Init();
            blueLbl.Text = "Blue: ";
            blueLbl.Width = 50;
            blueLbl.Height = 24;
            blueLbl.Left = redLbl.Left;
            blueLbl.Top = greenLbl.Top + greenLbl.Height;
            blueLbl.Anchor = Anchors.Left;
            blueLbl.Parent = window;

            blue = new SliderBar(manager);
            blue.Init();
            blue.Width = red.Width;
            blue.Height = red.Height;
            blue.Left = blueLbl.Left + blueLbl.Width + 20; ;
            blue.Top = blueLbl.Top + 5;
            blue.Anchor = Anchors.Left;
            blue.Parent = window;
            blue.Value = 50;
            blue.ValueChanged += new TomShane.Neoforce.Controls.EventHandler(Color_Changed);
            #endregion

            #region Tank Name
            name = new Label(manager);
            name.Init();
            name.Text = "Tank Name: ";
            name.Width = 100;
            name.Height = 24;
            name.Left = redLbl.Left;
            name.Top = blueLbl.Top + blueLbl.Height + 20;
            name.Anchor = Anchors.Left;
            name.Parent = window;

            nameBox = new TextBox(manager);
            nameBox.Init();
            nameBox.Text = "";
            nameBox.Width = 180;
            nameBox.Height = 24;
            nameBox.Left = name.Left + 10;
            nameBox.Top = name.Top + name.Height;
            nameBox.Anchor = Anchors.Left;
            nameBox.Parent = window;

            #endregion

            #region Buttons
            create = new Button(manager);
            create.Init();
            create.Text = "Create";
            create.Width = 90;
            create.Height = 24;
            create.Left = window.Width - create.Width - 40; ;
            create.Top = ratioLbl.Top;
            create.Anchor = Anchors.Left;
            create.Parent = window;

            cancel = new Button(manager);
            cancel.Init();
            cancel.Text = "Cancel";
            cancel.Width = 80;
            cancel.Height = 24;
            cancel.Left = create.Left - cancel.Width - 60;
            cancel.Top = create.Top;
            cancel.Anchor = Anchors.Right;
            cancel.Parent = window;
            #endregion
        }

        #region Events 
        void turretBox_ItemIndexChanged(object sender, TomShane.Neoforce.Controls.EventArgs e)
        {
            OnTurretChange(new TomShane.Neoforce.Controls.EventArgs());
        }

        void tankBox_ItemIndexChanged(object sender, TomShane.Neoforce.Controls.EventArgs e)
        {
            OnTankChange(new TomShane.Neoforce.Controls.EventArgs());
        }

        protected virtual void OnTurretChange(TomShane.Neoforce.Controls.EventArgs e)
        {
            TurretSelectionChanged.Invoke(this, e);
        }

        protected virtual void OnTankChange(TomShane.Neoforce.Controls.EventArgs e)
        {
            TankSelectionChanged.Invoke(this, e);
        }

        void Color_Changed(object sender, TomShane.Neoforce.Controls.EventArgs e)
        {
            tankColor = new Color(red.Value / 100f, green.Value / 100f, blue.Value / 100f);
        }

        private void SetColor()
        {
            Vector3 color = tankColor.ToVector3();
            red.Value = (int)(color.X * 100f);
            green.Value = (int)(color.Y * 100f);
            blue.Value = (int)(color.Z * 100f);
        }

        void ratioSlide_ValueChanged(object sender, TomShane.Neoforce.Controls.EventArgs e)
        {
            ChangeRatio(ratioSlide.Value);
        }

        private void ChangeRatio(int ratio)
        {
            int armor = ratio;
            int speed = 100 - armor;

            armorBar.Value = armor;
            speedBar.Value = speed;
        }

        #endregion
    }
}
