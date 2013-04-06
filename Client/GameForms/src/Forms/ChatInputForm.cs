using System;
using System.Collections.Generic;
using System.Text;
using TomShane.Neoforce.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameForms.Forms
{
    public class ChatInputForm
    {
        private Manager manager;
        private TextBox txtBox;

        public TextBox Box { get { return txtBox; } }
        public String Message { get { return txtBox.Text; } set { txtBox.Text = value; } }

        public event TomShane.Neoforce.Controls.EventHandler EnterPressed;

        public Vector2 Position
        {
            get;// { return new Vector2(window.Left, window.Top); }
            set;// { window.Left = (int)Position.X; window.Top = (int)Position.Y; }
        }

        public ChatInputForm(Manager _manager, Vector2 _pos, int width)
        {
            manager = _manager;
            Init(_pos, width);
            Position = _pos;
            manager.Add(txtBox);
        }

        private void Init(Vector2 pos, int width)
        {
            // Create and setup Window control.
            //window = new Window(manager);
            //window.Init();
            //window.Text = "Login";
            //window.Width = width+30;
            //window.Height = 20;
            //window.Top = (int)pos.Y;
            //window.Left = (int)pos.X;
            //window.Visible = true;
            //window.CloseButtonVisible = false;
            //window.CaptionVisible = false;
            //window.BorderVisible = true;
            //window.Resizable = false;
            //window.Movable = false;

            txtBox = new TextBox(manager);
            txtBox.Init();
            txtBox.Text = "";
            txtBox.Width = width;
            txtBox.Height = 24;
            txtBox.Anchor = Anchors.All;
            txtBox.Top = (int)pos.Y;
            txtBox.Left = (int)pos.X;
            //txtBox.Parent = window;
            txtBox.BackColor = new Color(0, 0, 0, 100);
            txtBox.TextColor = new Color(255, 255, 255, 255);
            txtBox.KeyPress += new KeyEventHandler(Txt_KeyPress);
        }

        void Txt_KeyPress(object sender, KeyEventArgs e)
        {
            if (e.Key == Microsoft.Xna.Framework.Input.Keys.Enter)
                OnEnterPressed(new TomShane.Neoforce.Controls.EventArgs());
        }

        protected virtual void OnEnterPressed(TomShane.Neoforce.Controls.EventArgs e)
        {
            EnterPressed.Invoke(this, e);
        }

        public void Clear()
        {
            txtBox.Text = "";
        }
    }
}
