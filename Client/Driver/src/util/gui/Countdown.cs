using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Client.src.service;

namespace Client.src.util.game
{
    /// <summary>
    /// TODO: Document me.
    /// TODO: Re-do most of this class, but keep the drawing functions.
    /// TODO: Set most of the members to private.
    /// </summary>
    public class Countdown
    {
        
        List<Texture2D> images;
        Texture2D back;

        public TextLocation tLocation = TextLocation.Top;

        Color fillColor = Color.Green;
        Color backColor = new Color(0, 0, 0, .5f);
        double len = 10;
        public double Length { get { return len; } set { len = value; } }

        double current = 0;
        double snapshot = 0;
        SpriteFont bigFont;
        Vector2 space;
        bool enabled = false;
        public bool Enabled { get { return enabled; } set { enabled = value; } }
        bool locked = false;
        public bool Locked { get { return locked; } set { locked = value; } }

        Rectangle rect;
        Vector2 messagePos;
        Rectangle backRect;

        public String Message = "";

        public enum CountdownType
        {
            Respawn,
            MapChange
        };

        public enum TextLocation
        {
            Top,
            Bottom
        };

        const int respawnTime = 6;
        const int mapTime = 10;

        String[] messages = { "Respawn in", "Map change in" };

        public Countdown()
        {
            LoadContent();
        }

        void LoadContent()
        {
            images = new List<Texture2D>();
            String s;
            for (int i = 0; i < 10; i++)
            {
                s = (i + 1).ToString();
                images.Add(ServiceManager.Resources.GetTexture2D("textures\\misc\\CD\\" + s));
            }
            
            back = ServiceManager.Resources.GetTexture2D("textures\\misc\\CD\\cdBack");
            bigFont = ServiceManager.Resources.GetFont("cd");
            space = bigFont.MeasureString("55");

            if (tLocation == TextLocation.Bottom)
            {
                rect = new Rectangle(0, 0, (int)(images[0].Width * Constants.UI_SCALE), (int)(images[0].Height * Constants.UI_SCALE));
                messagePos = new Vector2(rect.X, rect.Y);
                messagePos.Y += rect.Height + 5;

                backRect = rect;
                backRect.Width += 5;
            }
            else if (tLocation == TextLocation.Top)
            {
                messagePos = new Vector2(5, 5);

                rect = new Rectangle(0, 0, (int)(images[0].Width * Constants.UI_SCALE), (int)(images[0].Height * Constants.UI_SCALE));

                backRect = rect;
                backRect.Width += 5;
            }
        }

        public void StartCD(CountdownType _type)
        {
            if (!locked)
            {
                snapshot = Network.Util.Clock.GetTimeMilliseconds();
                current = snapshot;
                enabled = true;

                if (_type == CountdownType.Respawn)
                {
                    Length = respawnTime;
                    fillColor = Color.Red;
                    Message = messages[0];
                }
                else if (_type == CountdownType.MapChange)
                {
                    Length = mapTime;
                    fillColor = Color.GreenYellow;
                    Message = messages[1];
                    Locked = true;
                }

                Vector2 size = bigFont.MeasureString(Message);


                messagePos.X = (int)(rect.X + (rect.Width / 2) - ((size.X * Constants.UI_SCALE) / 2));

                if (tLocation == TextLocation.Bottom)
                {
                    backRect.Height = rect.Height + (int)(size.Y * Constants.UI_SCALE) + 5;
                }
                else if (tLocation == TextLocation.Top)
                {
                    rect.Y = (int)(messagePos.Y + (size.Y * Constants.UI_SCALE));
                    backRect.Height = rect.Height + (int)(size.Y * Constants.UI_SCALE) + 10;
                }
            }
        }

        public void Update()
        {
            current += ServiceManager.Game.DeltaTime;
        }

        public void Draw()
        {
            if (enabled)
            {
                double elapsed = current - snapshot;
                int cd = (int)(len - elapsed);

                if (cd <= 0 || cd > len)
                {
                    enabled = false;
                    snapshot = 0;
                    current = 0;
                    locked = false;
                    return;
                }

                ServiceManager.Game.Batch.Begin();

                ServiceManager.Game.Batch.Draw(back,
                    backRect,
                    backColor);
                ServiceManager.Game.Batch.Draw(images[cd - 1], 
                    rect, 
                    fillColor);
                ServiceManager.Game.Batch.DrawString(bigFont, Message,
                    messagePos,
                    fillColor, 0f, Vector2.Zero, Constants.UI_SCALE, SpriteEffects.None, 0f);

                ServiceManager.Game.Batch.End();
            }
        }
        
    }
}
