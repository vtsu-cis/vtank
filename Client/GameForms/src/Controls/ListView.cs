using System;
using System.Collections.Generic;
using System.Text;
using TomShane.Neoforce.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

namespace GameForms.Controls
{
    class ListView : Control
    {

        private const string skListView = "ListView"; //Control's skin we use (from skin file)
        private const string lrListView = "Control"; // Layer we will draw (from skin file)
        public List<String[]> Items;
        public Header Headers;
        private ScrollBar sbVert = null;
        private ClipBox pane = null;
        private int itemIndex = -1;
        private bool hotTrack = false;
        private int itemsCount = 0;
        private bool hideSelection = true;
        Vector2 charArea = Vector2.Zero;
        private Texture2D line;

        #region //// Properties ////////

        ////////////////////////////////////////////////////////////////////////////
        public virtual bool HotTrack
        {
            get { return hotTrack; }
            set
            {
                if (hotTrack != value)
                {
                    hotTrack = value;
                    if (!Suspended) OnHotTrackChanged(new TomShane.Neoforce.Controls.EventArgs());
                }
            }
        }
        ////////////////////////////////////////////////////////////////////////////

        ////////////////////////////////////////////////////////////////////////////
        public virtual int ItemIndex
        {
            get { return itemIndex; }
            set
            {
                //if (itemIndex != value)
                {
                    if (value >= 0 && value < Items.Count)
                    {
                        itemIndex = value;
                    }
                    else
                    {
                        itemIndex = -1;
                    }
                    ScrollTo(itemIndex);

                    if (!Suspended) OnItemIndexChanged(new TomShane.Neoforce.Controls.EventArgs());
                }
            }
        }
        ////////////////////////////////////////////////////////////////////////////

        ////////////////////////////////////////////////////////////////////////////
        public virtual bool HideSelection
        {
            get { return hideSelection; }
            set
            {
                if (hideSelection != value)
                {
                    hideSelection = value;
                    Invalidate();
                    if (!Suspended) OnHideSelectionChanged(new TomShane.Neoforce.Controls.EventArgs());
                }
            }
        }
        ////////////////////////////////////////////////////////////////////////////

        #endregion

        #region //// Events ////////////

        ////////////////////////////////////////////////////////////////////////////                 
        public event TomShane.Neoforce.Controls.EventHandler HotTrackChanged;
        public event TomShane.Neoforce.Controls.EventHandler ItemIndexChanged;
        public event TomShane.Neoforce.Controls.EventHandler HideSelectionChanged;
        ////////////////////////////////////////////////////////////////////////////

        #endregion

        public ListView(Manager manager, String[] _headers)
            : base(manager)
        {

            Width = 610;
            Height = 64;
            MinimumHeight = 16;

            CreateHeaders(_headers, Width);
            Headers.Init();
            Headers.Parent = this;


            sbVert = new ScrollBar(Manager, Orientation.Vertical);
            sbVert.Init();
            sbVert.Parent = this;
            sbVert.Left = Left + Width - sbVert.Width - Skin.Layers["Control"].ContentMargins.Right;
            sbVert.Top = Top + Skin.Layers["Control"].ContentMargins.Top + Headers.Height;
            sbVert.Height = Height - Skin.Layers["Control"].ContentMargins.Vertical - Headers.Height;
            sbVert.Anchor = Anchors.Top | Anchors.Right | Anchors.Bottom;
            sbVert.PageSize = 25;
            sbVert.Range = 1;
            sbVert.PageSize = 1;
            sbVert.StepSize = 10;

            pane = new ClipBox(manager);
            pane.Init();
            pane.Parent = this;
            pane.Top = Skin.Layers["Control"].ContentMargins.Top + Headers.Height;
            pane.Left = Skin.Layers["Control"].ContentMargins.Left;
            pane.Width = Width - sbVert.Width - Skin.Layers["Control"].ContentMargins.Horizontal - 1;
            pane.Height = Height - Skin.Layers["Control"].ContentMargins.Vertical - Headers.Height;
            pane.Anchor = Anchors.All;
            pane.Passive = true;
            pane.CanFocus = false;
            pane.Draw += new DrawEventHandler(DrawPane);

            CanFocus = true;
            Passive = false;
        }



        public override void Init()
        {
            base.Init();
            Items = new List<string[]>();
            line = new Texture2D(Manager.GraphicsDevice, 1, 1);
            line.SetData<Color>(new Color[] { Color.Black });
            //Headers = new List<Header>();
        }
        ////////////////////////////////////////////////////////////////////////////

        ////////////////////////////////////////////////////////////////////////////
        protected override void InitSkin()
        {
            base.InitSkin();

            // We specify what skin this control uses.
            Skin = new SkinControl(Manager.Skin.Controls[skListView]);
        }
        ////////////////////////////////////////////////////////////////////////////

        ////////////////////////////////////////////////////////////////////////////
        public virtual void AutoHeight(int maxItems)
        {
            if (Items != null && Items.Count < maxItems) maxItems = Items.Count;
            if (maxItems < 3) maxItems = 3;

            SkinText font = Skin.Layers["Control"].Text;
            if (Items != null && Items.Count > 0)
            {
                int h = (int)font.Font.Resource.MeasureString(Items[0][0].ToString()).Y;
                Height = (h * maxItems) + (Skin.Layers["Control"].ContentMargins.Vertical);// - Skin.OriginMargins.Vertical);
            }
            else
            {
                Height = 32;
            }
        }
        ////////////////////////////////////////////////////////////////////////////

        ////////////////////////////////////////////////////////////////////////////
        protected override void DrawControl(Renderer renderer, Rectangle rect, GameTime gameTime)
        {
            sbVert.Invalidate();
            pane.Invalidate();
    
            base.DrawControl(renderer, rect, gameTime);
        }
        ////////////////////////////////////////////////////////////////////////////

        ////////////////////////////////////////////////////////////////////////////
        private void DrawPane(object sender, DrawEventArgs e)
        {
            if (Items != null && Items.Count > 0)
            {
                SkinText font = Skin.Layers["Control"].Text;
                SkinLayer sel = Skin.Layers["ListView.Selection"];
                int h = (int)font.Font.Resource.MeasureString(Items[0][0].ToString()).Y;
                int v = (sbVert.Value / 10);
                int p = (sbVert.PageSize / 10);
                int d = (int)(((sbVert.Value % 10) / 10f) * h);
                int c = Items.Count;
                int s = itemIndex;

                FormatHeaders(Headers);

                //e.Renderer.DrawString(this, Skin.Layers["Control"], stufs, new Rectangle(e.Rectangle.Left, e.Rectangle.Top, e.Rectangle.Width, h), false);

                for (int i = v; i <= v + p + 1; i++)
                {
                    if (i < c)
                    {
                        DrawStrings(e, this, Skin.Layers["Control"], Items[i], new Rectangle(e.Rectangle.Left, e.Rectangle.Top - d + ((i - v) * h), e.Rectangle.Width, h), false);
                    }
                }
                if (s >= 0 && s < c && (Focused || !hideSelection))
                {
                    int pos = -d + ((s - v) * h);
                    if (pos > -h && pos < (p + 1) * h)
                    {
                        e.Renderer.DrawLayer(this, sel, new Rectangle(0, pos, e.Rectangle.Width, h));
                        DrawStrings(e, this, sel, Items[s], new Rectangle(e.Rectangle.Left, e.Rectangle.Top + pos, e.Rectangle.Width, h), false);
                    }
                }
            }
        }

        void DrawStrings(DrawEventArgs e, Control control, SkinLayer layer, String[] str, Rectangle rectangle, bool margins)
        {
            Rectangle rect = rectangle;

            for (int i = 0; i < Headers.HeaderButtons.Count; i++)
            {
                //Section off each "cell"
                rect.X = Headers.HeaderButtons[i].Left;
                rect.Width = Headers.HeaderButtons[i].Width;

                //Draw a line on top, draw the string, draw a line on the bottom, draw a line on the left
                e.Renderer.Draw(line, new Rectangle(rect.X - 2, rect.Y, rect.Width, 1), Color.Black);
                e.Renderer.DrawString(control, layer, " " + str[i], rect, margins);
                e.Renderer.Draw(line, new Rectangle(rect.X - 2, rect.Y+rect.Height, rect.Width, 1), Color.Black);
                e.Renderer.Draw(line, new Rectangle(rect.X - 2, rect.Y, 1, rect.Height), Color.Black);
            }
        }

        ////////////////////////////////////////////////////////////////////////////

        ////////////////////////////////////////////////////////////////////////////
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (e.Button == MouseButton.Left || e.Button == MouseButton.Right)
            {
                TrackItem(e.Position.X, e.Position.Y);
            }
        }
        ////////////////////////////////////////////////////////////////////////////

        ////////////////////////////////////////////////////////////////////////////
        private void TrackItem(int x, int y)
        {
            y -= Headers.Height;

            if (Items != null && Items.Count > 0 && (pane.ControlRect.Contains(new Point(x, y+Headers.Height))))
            {
                SkinText font = Skin.Layers["Control"].Text;
                int h = (int)font.Font.Resource.MeasureString(Items[0][0].ToString()).Y;
                int d = (int)(((sbVert.Value % 10) / 10f) * h);
                int i = (int)Math.Floor((sbVert.Value / 10f) + ((float)y / h));
                if (i >= 0 && i < Items.Count && i >= (int)Math.Floor((float)sbVert.Value / 10f) && i < (int)Math.Ceiling((float)(sbVert.Value + sbVert.PageSize) / 10f)) ItemIndex = i;
                Focused = true;
            }
        }
        ////////////////////////////////////////////////////////////////////////////

        ////////////////////////////////////////////////////////////////////////////
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (hotTrack)
            {
                TrackItem(e.Position.X, e.Position.Y);
            }
        }
        ////////////////////////////////////////////////////////////////////////////

        ////////////////////////////////////////////////////////////////////////////
        protected override void OnKeyPress(KeyEventArgs e)
        {
            if (e.Key == Keys.Down)
            {
                e.Handled = true;
                itemIndex += sbVert.StepSize / 10;
            }
            else if (e.Key == Keys.Up)
            {
                e.Handled = true;
                itemIndex -= sbVert.StepSize / 10;
            }
            else if (e.Key == Keys.PageDown)
            {
                e.Handled = true;
                itemIndex += sbVert.PageSize / 10;
            }
            else if (e.Key == Keys.PageUp)
            {
                e.Handled = true;
                itemIndex -= sbVert.PageSize / 10;
            }
            else if (e.Key == Keys.Home)
            {
                e.Handled = true;
                itemIndex = 0;
            }
            else if (e.Key == Keys.End)
            {
                e.Handled = true;
                itemIndex = Items.Count - 1;
            }

            if (itemIndex < 0) itemIndex = 0;
            else if (itemIndex >= Items.Count) itemIndex = Items.Count - 1;

            ItemIndex = itemIndex;

            base.OnKeyPress(e);
        }
        ////////////////////////////////////////////////////////////////////////////

        ////////////////////////////////////////////////////////////////////////////
        protected override void OnGamePadPress(GamePadEventArgs e)
        {
            if (e.Button == GamePadActions.Down)
            {
                e.Handled = true;
                itemIndex += sbVert.StepSize / 10;
            }
            else if (e.Button == GamePadActions.Up)
            {
                e.Handled = true;
                itemIndex -= sbVert.StepSize / 10;
            }

            if (itemIndex < 0) itemIndex = 0;
            else if (itemIndex >= Items.Count) itemIndex = Items.Count - 1;

            ItemIndex = itemIndex;
            base.OnGamePadPress(e);
        }
        ////////////////////////////////////////////////////////////////////////////

        ////////////////////////////////////////////////////////////////////////////
        private void ItemsChanged()
        {
            if (Items != null && Items.Count > 0)
            {
                SkinText font = Skin.Layers["Control"].Text;
                int h = (int)font.Font.Resource.MeasureString(Items[0][0].ToString()).Y;

                int sizev = Height - Skin.Layers["Control"].ContentMargins.Vertical;
                sbVert.Range = Items.Count * 10;
                sbVert.PageSize = (int)Math.Floor((float)sizev * 10 / h);
                Invalidate();
            }
            else if (Items == null || Items.Count <= 0)
            {
                sbVert.Range = 1;
                sbVert.PageSize = 1;
                Invalidate();
            }

            RescaleHeaders();
        }

        ////////////////////////////////////////////////////////////////////////////
        public virtual void ScrollTo(int index)
        {
            ItemsChanged();
            if ((index * 10) < sbVert.Value)
            {
                sbVert.Value = index * 10;
            }
            else if (index >= (int)Math.Floor(((float)sbVert.Value + sbVert.PageSize) / 10f))
            {
                sbVert.Value = ((index + 1) * 10) - sbVert.PageSize;
            }
        }
        ////////////////////////////////////////////////////////////////////////////

        ////////////////////////////////////////////////////////////////////////////
        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            FormatHeaders(Headers);
            if (Visible && Items != null && Items.Count != itemsCount)
            {
                itemsCount = Items.Count;
                ItemsChanged();
            }
        }
        ////////////////////////////////////////////////////////////////////////////

        ////////////////////////////////////////////////////////////////////////////     
        protected virtual void OnItemIndexChanged(TomShane.Neoforce.Controls.EventArgs e)
        {
            if (ItemIndexChanged != null) ItemIndexChanged.Invoke(this, e);
        }
        ////////////////////////////////////////////////////////////////////////////     

        ////////////////////////////////////////////////////////////////////////////     
        protected virtual void OnHotTrackChanged(TomShane.Neoforce.Controls.EventArgs e)
        {
            if (HotTrackChanged != null) HotTrackChanged.Invoke(this, e);
        }
        ////////////////////////////////////////////////////////////////////////////  

        ////////////////////////////////////////////////////////////////////////////     
        protected virtual void OnHideSelectionChanged(TomShane.Neoforce.Controls.EventArgs e)
        {
            if (HideSelectionChanged != null) HideSelectionChanged.Invoke(this, e);
        }
        ////////////////////////////////////////////////////////////////////////////       


        private void FormatHeaders(Header _header)
        {
            for (int i = 0; i < _header.ColumnCount; i++)
            {
                _header.HeaderButtons[i].Left = pane.Left + (i * _header.HeaderButtons[i].Width);
                //_header.HeaderButtons[i].Top = pane.Top;
            }
        }

        private void CreateHeaders(string[] _headers, int _width)
        {
            int headerwidth = _width / _headers.Length;
            Headers = new Header(Manager, _headers.Length, _width+5);

            for(int i = 0;i<_headers.Length;i++)
            {
                Headers.HeaderButtons[i].Text = _headers[i];
            }
        }

        private void RescaleHeaders()
        {
            Headers.Width = pane.Width+5;
            int scale = Headers.Width / Headers.ColumnCount;

            foreach (HeaderButton h in Headers.HeaderButtons)
            {
                h.Width = scale;
            }
        }

        
    }
}
