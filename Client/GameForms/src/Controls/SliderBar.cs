/*!
    \file   SliderBar.cs
    \brief  A custom sliderbar control for NeoForce Forms
    \author (C) Copyright 2009 by Vermont Technical College
*/
using System;
using System.Collections.Generic;
using System.Text;
using TomShane.Neoforce.Controls;
using Microsoft.Xna.Framework;

namespace GameForms.Controls
{

    class SliderBar : Control
    {
        #region Members and Properties
        int prevValue = 0;
        int _value = 0;
        public int Value { get { return _value; } set { _value = value; } }
        bool bMouseDown = false;
        bool bSliderDown = false;
        Button slider;
        Point p = Point.Zero;
        int delta = 0;
        double increment = 1.5;

        public event TomShane.Neoforce.Controls.EventHandler ValueChanged;

        public override int Width
        {
            get
            {
                return base.Width;
            }
            set
            {
                base.Width = value;
                increment = (double)this.Width / 100.0;
            }
        }

        public override int Height
        {
            get
            {
                return base.Height;
            }
            set
            {
                base.Height = value;
                if (slider != null)
                    slider.Height = this.Height - 2;
            }
        }
        #endregion

        public SliderBar(Manager manager)
            : base(manager)
        {
        }

        #region Events
        ////////////////////////////////////////////////////////////////////////////
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            bMouseDown = true;
            SetValue(e);
        }


        ////////////////////////////////////////////////////////////////////////////

        ////////////////////////////////////////////////////////////////////////////
        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            bMouseDown = false;
        }
        ////////////////////////////////////////////////////////////////////////////

        ////////////////////////////////////////////////////////////////////////////
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (bMouseDown)
            {
                SetValue(e);
            }
        }
        ////////////////////////////////////////////////////////////////////////////

        private void SetValue(MouseEventArgs e)
        {
            Point p = new Point(e.Position.X, e.Position.Y);

            Value = (int)(100-((((float)Width - (float)p.X) / (float)Width) * (float)100)); ;
            Value = (int)MathHelper.Clamp(Value, 0.0f, 100.0f);
        }

        void slider_MouseMove(object sender, MouseEventArgs e)
        {
            if (bSliderDown)
            {
                delta = p.X - e.Position.X;
                Value -= (int)(delta / 1.5);
            }
        }

        void slider_MouseUp(object sender, MouseEventArgs e)
        {
            bSliderDown = false;   
        }

        void slider_MouseDown(object sender, MouseEventArgs e)
        {
            bSliderDown = true;
            p = new Point(e.Position.X, e.Position.Y);
        }

        ////////////////////////////////////////////////////////////////////////////     
        protected virtual void OnValueChanged(TomShane.Neoforce.Controls.EventArgs e)
        {
            if(ValueChanged != null) ValueChanged.Invoke(this, e);
        }
        //////////////////////////////////////////////////////////////////////////// 
        #endregion

        #region Control Overrides
        ////////////////////////////////////////////////////////////////////////////
        public override void Init()
        {
            base.Init();

            slider = new Button(Manager);
            slider.Init();
            slider.Parent = this;
            slider.Top = 1;
            slider.Left = 0;
            slider.Width = 10;
            slider.Height = this.Height - 2;
            slider.Text = "";
            slider.ToolTip = new ToolTip(Manager);
            slider.ToolTip.Text = this.Value.ToString();
            slider.MouseDown += new MouseEventHandler(slider_MouseDown);
            slider.MouseUp += new MouseEventHandler(slider_MouseUp);
            slider.MouseMove += new MouseEventHandler(slider_MouseMove);
        }
        ////////////////////////////////////////////////////////////////////////////
        protected override void InitSkin()
        {
            base.InitSkin();

            // We specify what skin this control uses.
            Skin = new SkinControl(Manager.Skin.Controls["SliderBar"]);
        }
        ////////////////////////////////////////////////////////////////////////////

        ////////////////////////////////////////////////////////////////////////////
        protected override void DrawControl(Renderer renderer, Rectangle rect, GameTime gameTime)
        {
            // We draw the control the same way the ancestor does.
            // In this case, ancestor draws all states automatically according to description in the skin file.
            base.DrawControl(renderer, rect, gameTime);
        }
        ////////////////////////////////////////////////////////////////////////////   

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            this.Text = ""; //Not sure why I need this, but it won't work correctly if it's not here

            if (this.Initialized && prevValue != Value)
            {
                OnValueChanged(new TomShane.Neoforce.Controls.EventArgs());
                prevValue = Value;
            }

            Value = (int)MathHelper.Clamp(Value, 0.0f, 100.0f);

            slider.ToolTip.Text = this.Value.ToString();
            slider.Left = (int)(Value * increment) - (slider.Width/2);
        }
        #endregion
    }
}
