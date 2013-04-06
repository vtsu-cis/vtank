using System;
using System.Collections.Generic;
using System.Text;
using TomShane.Neoforce.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GameForms.Controls
{
    /// <summary>
    /// Represents a simple output screen for displaying text
    /// </summary>
    public class ConsoleArea : Control
    {
        Queue<String> data;
        TextOrigin origin = TextOrigin.BOTTOM;
        ScrollBar vert;

        public int MaxLines = 100;
        public TextOrigin TextPosition { get { return origin; } set { origin = value; } }
        public Color BackgroundColor { get { return this.Color; } set { this.Color = value; } }
        
        /// <summary>
        /// Enumeration to specify how the text is drawn to the console. 
        /// From bottom to top (BOTTOM) or top to bottom (TOP)
        /// </summary>
        public enum TextOrigin
        {
            BOTTOM,
            TOP
        }

        public ConsoleArea(Manager manager)
            : base(manager)
        {
            data = new Queue<string>();
            TextColor = Color.Green;
            BackgroundColor = Color.Black;
        }

        #region Overridden Methods
        /// <summary>
        /// Override of Init
        /// </summary>
        public override void Init()
        {
            base.Init();

            vert = new ScrollBar(Manager, Orientation.Vertical);
            vert.Init();
            vert.Parent = this;
            vert.Left = this.Width - vert.Width;
            vert.Height = this.ClientHeight;
            vert.Value = 0;
            vert.Range = 0;
            vert.PageSize = 25;
            vert.Range = 1;
            vert.PageSize = 1;
            vert.StepSize = 10;
            vert.Anchor = Anchors.Top | Anchors.Right | Anchors.Bottom;

            //event setup
            this.Resize += new ResizeEventHandler(Recalc);
            vert.ValueChanged += new TomShane.Neoforce.Controls.EventHandler(ValueChanged);

            Recalc(this, null); // Calculates initial properties of the scrollbars
        }

        protected override void InitSkin()
        {
            base.InitSkin();
        }

        /// <summary>
        /// Overridden DrawControl method
        /// </summary>
        /// <param name="renderer"></param>
        /// <param name="rect"></param>
        /// <param name="gameTime"></param>
        protected override void DrawControl(Renderer renderer, Rectangle rect, GameTime gameTime)
        {
            base.DrawControl(renderer, rect, gameTime);

            if (data.Count > 0)
            {
                if (origin == TextOrigin.BOTTOM)
                {
                    String[] arr = Reverse(data);
                    SkinText font = Skin.Layers["Control"].Text;
                    int h = (int)font.Font.Resource.MeasureString(data.Peek()).Y;
                    int v = ((vert.Range - vert.PageSize - vert.Value) / 10);
                    int p = (vert.PageSize / 10);
                    int d = (int)(((vert.Value % 10) / 10f) * h);
                    int c = data.Count;

                    for (int i = v; i <= v + p; i++)
                    {
                        if (i < c)
                        {
                            renderer.DrawString(this, Skin.Layers["Control"], arr[i],
                                new Rectangle(rect.Left + 5, (rect.Height - h) - ((i-v)*h), rect.Width, h));
                        }
                    }
                }
                else if (origin == TextOrigin.TOP)
                {
                    rect.Y += 5;
                    rect.Height += 15;
                    String[] arr = Reverse(data);

                    SkinText font = Skin.Layers["Control"].Text;
                    int h = (int)font.Font.Resource.MeasureString(data.Peek()).Y;
                    int v = (vert.Value / 10);
                    int p = (vert.PageSize / 10);
                    int d = (int)(((vert.Value % 10) / 10f) * h);
                    int c = data.Count;

                    for (int i = v; i <= v + p + 1; i++)
                    {
                        if (i < c)
                        {
                            renderer.DrawString(this, Skin.Layers["Control"], arr[i], 
                                new Rectangle(rect.Left + 5, rect.Top - d + ((i - v) * h), rect.Width, h));
                        }
                    }
                }
            }
        }
        #endregion

        #region Text Operations
        /// <summary>
        /// Adds a string to the console, removing oldest string if max number 
        /// of lines has been reached
        /// </summary>
        /// <param name="s">String to add</param>
        public void AddString(String s)
        {
            if (s.Length == 0 || s == "\n")
                return;

            //multiline messages
            if (s.Contains("\\n"))
            {
                foreach (String sub in s.Split(new String[]{"\\n"}, StringSplitOptions.RemoveEmptyEntries))
                {
                    AddString(sub);
                }
                return;
            }

            //Add the string
            if (data.Count < MaxLines)
            {
                data.Enqueue(s);
            }
            else
            {
                data.Dequeue();
                data.Enqueue(s);
            }

            //Resize the scrollbar
            Recalc(this, null);

            //Set the scrollbar position to the most recently sent message
            if (TextPosition == TextOrigin.BOTTOM)
                vert.Value = vert.Range;
            else
                vert.Value = 0;
        }

        /// <summary>
        /// Removes a single string from the end of the console
        /// </summary>
        /// <returns>removed string</returns>
        public String RemoveString()
        {
            String s = data.Dequeue();
            Recalc(this, null);
            Invalidate();
            return s;
        }

        /// <summary>
        /// Removed a specified number of strings from the end of the console.  
        /// </summary>
        /// <param name="numLines"></param>
        /// <returns>a list containing removed strings</returns>
        public List<String> RemoveStrings(int numLines)
        {
            List<String> strings = new List<string>();

            if(numLines > data.Count) numLines = data.Count;

            for (int i = 0; i < numLines; i++)
            {
                strings.Add(data.Dequeue());
            }

            Recalc(this, null);
            Invalidate();
            return strings;
        }
        
        /// <summary>
        /// Clears the console window
        /// </summary>
        public void Clear()
        {
            data.Clear();
            Recalc(this, null);
            Invalidate();
        }
        #endregion

        #region Event Handlers
        /// <summary>
        /// Event handler that will resize the scrollbars when resizes take place or strings are added
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Recalc(object sender, ResizeEventArgs e)
        {
            if (!vert.Enabled) vert.Enabled = true;

            if (data != null && data.Count > 0)
            {
                SkinText font = Skin.Layers["Control"].Text;
                int h = (int)font.Font.Resource.MeasureString(data.Peek()).Y;

                int sizev = Height - Skin.Layers["Control"].ContentMargins.Vertical;
                vert.Range = data.Count * 10;
                vert.PageSize = (int)Math.Floor((float)sizev * 10 / h);
                Invalidate();
            }
            else if (data == null || data.Count <= 0)
            {
                vert.Range = 1;
                vert.PageSize = 1;
                Invalidate();
            }
        }

        /// <summary>
        /// Event Handler for scrollbar movement
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ValueChanged(object sender, TomShane.Neoforce.Controls.EventArgs e)
        {
            // For every scrollbar value change we recalc properties
            Recalc(sender, null);
        }
        #endregion

        #region Utils
        /// <summary>
        /// Returns an array of strings in the reverse order they appeared in the queue.
        /// Equivalent code as .NET 3.0 Queue.Reverse().ToArray()
        /// </summary>
        /// <param name="q"></param>
        /// <returns></returns>
        String[] Reverse(Queue<String> q)
        {
            String[] temp = new String[q.Count];

            Stack<String> newQ = new Stack<String>(q);

            for(int i = 0;i<q.Count;i++)
            {
                temp[i] = newQ.Pop();
            }

            return temp;

        }
        #endregion
    }
}
