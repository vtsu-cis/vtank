using System;
using System.Collections.Generic;
using System.Text;
using TomShane.Neoforce.Controls;
using Microsoft.Xna.Framework.Graphics;

namespace GameForms.Controls
{
    public class VTankConsole : Control
    {
        TextBox output;
        TextBox input;
        Queue<String> queue;

        public VTankConsole(Manager manager, int _width, int _height)
            : base(manager)
        {
            this.Width = _width;
            this.Height = _height;
            queue = new Queue<string>(new String[] { "", "", "", "", "", "", "", "", "", ""});
        }

        public override void Init()
        {
            base.Init();
            this.BackColor = Color.TransparentBlack;
            this.Color = new Color(50, 50, 50);
            this.Alpha = 100;

            output = new TextBox(Manager);
            output.Init();
            output.Width = Width;
            output.Height = Height - 30;
            output.Passive = true;
            output.Parent = this;
            output.TextColor = Microsoft.Xna.Framework.Graphics.Color.White;
            output.BackColor = new Color(0, 0, 0, 100);
            output.Alpha = 100;
            output.Color = new Color(0, 0, 0, 100);
            output.ReadOnly = true;
            

            input = new TextBox(Manager);
            input.Init();
            input.Width = Width;
            input.Height = 30;
            input.Top = output.Top + output.Height;
            input.Parent = this;
            input.KeyPress += new KeyEventHandler(input_KeyPress);
            input.TextColor = Microsoft.Xna.Framework.Graphics.Color.White;
            Manager.BringToFront(this);
        }

        void input_KeyPress(object sender, KeyEventArgs e)
        {
            if (e.Key == Microsoft.Xna.Framework.Input.Keys.Enter)
            {
                AddText(input.Text);
                input.Text = "";
            }
        }

        void AddText(String text)
        {
            output.Text = "";

            if(!text.Equals(""))
                queue.Enqueue(text);

            if (queue.Count > 10)
                queue.Dequeue();

            String[] sections = queue.ToArray();
            for (int i = 0; i < sections.Length; i++)
            {
                output.Text += "\n" + sections[i];
            }
        }


    }
}
