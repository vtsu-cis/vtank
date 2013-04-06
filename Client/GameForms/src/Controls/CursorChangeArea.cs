using System;
using System.Collections.Generic;
using System.Text;
using TomShane.Neoforce.Controls;
using Microsoft.Xna.Framework;

namespace GameForms.Controls
{
    public class CursorChangeArea : Control
    {
        public CursorChangeArea(Manager mgr)
            : base(mgr)
        {
            this.Cursor = Manager.Skin.Cursors["Hand"].Resource;
        }

        public override void Init()
        {
            base.Init();
        }

        protected override void InitSkin()
        {
            base.InitSkin();
        }

        protected override void DrawControl(Renderer renderer, Rectangle rect, GameTime gameTime)
        {
            base.DrawControl(renderer, rect, gameTime);

            SkinLayer layer = Skin.Layers["Control"];

            if(Text != null && Text != "")
            {
            int width = (int)layer.Text.Font.Resource.MeasureString(Text).X;
            int ox = 5;

            if (width < this.Width)
            {
                ox = (this.Width - width) / 2;
            }
                renderer.DrawString(this, layer, Text, rect, true, ox, 0);
            
            }
        }
    }
}
