/*!
    \file   HUD_Bar.cs
    \brief  Display bar for use with a HUD
    \author (C) Copyright 2008 by Vermont Technical College
*/
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Client.src.service;

namespace Client.src.util.game
{
    public class HUD_Bar
    {
        public int Max;
        public int Min;
        public int Value;
        public float Scale;
        public Color BarColor;
        public Color PulseColor = Color.White;
        public bool IsPulsing { get; private set; }
        public byte PulseIncrement = 5;
        public float Opacity;
        public Vector2 Origin;
        public HUDOrientation Orientation;
        public Texture2D Outline;
        public Texture2D Filler;
        public bool TextEnabled;
        public TextOrientation TOrientation;

        public static Color defaultColor = Color.Green;
        public static float defaultOpacity = .9f;
        private Color originalColor;

        //coordinates of topleft corner
        int x;
        int y;

        //width and height of the current resolution
        int winWidth;
        int winHeight;

        /// <summary>
        /// Determines which way the bar will be drawn
        /// </summary>
        public enum HUDOrientation
        {
            LEFT,
            RIGHT
        };

        /// <summary>
        /// Determines where the text is located in relation to the bar
        /// </summary>
        public enum TextOrientation
        {
            BOTTOMLEFT,
            BOTTOMRIGHT,
            BOTTOM,
            TOPLEFT,
            TOPRIGHT,
            TOP
        };

        public HUD_Bar()
        {
            winWidth = ServiceManager.Game.Window.ClientBounds.Width;
            winHeight = ServiceManager.Game.Window.ClientBounds.Height;
        }

        #region Fading Members/Methods
        /// <summary>
        /// Control whether or not a HUD is fading in or out.
        /// </summary>
        enum FadingState
        {
            NONE,
            IN,
            OUT
        };

        // How long it takes to fade out, in seconds.
        const float fadeOutTime = 2.0f;
        // How long it takes to fade in, in seconds.
        const float fadeInTime = 0.25f;
        // How long it takes to hold at faded-in opacity, in seconds
        const float holdTime = 2.5f;

        // The opacity for each state of the bars
        const float inOpacity  = .8f;
        const float outOpacity = .2f;

        FadingState state;
        float elapsed;
        float holdElapsed;
        public bool holding;
        private bool pulseIn;

        /// <summary>
        /// Have the HUD gradually fade it's opacity to near-invisible.
        /// </summary>
        public void FadeOut()
        {
            state = FadingState.OUT;
            elapsed = 0f;
        }

        /// <summary>
        /// Have the HUD gradually fade it's opacity to 100% visible.
        /// </summary>
        public void FadeIn()
        {
            state = FadingState.IN;
            elapsed = 0f;
        }

        /// <summary>
        /// Stop fading the HUD immediately.
        /// </summary>
        private void StopFading()
        {
            state = FadingState.NONE;
            elapsed = 0f;
        }

        /// <summary>
        /// Start pulsing the color of the action bar.
        /// </summary>
        /// <param name="R">Red value to pulse to.</param>
        /// <param name="G">Green value to pulse to.</param>
        /// <param name="B">Blue value to pulse to.</param>
        public void StartPulsing(byte R, byte G, byte B)
        {
            IsPulsing = true;
            originalColor = new Color(BarColor, Opacity);
            PulseColor = new Color(R, G, B);
        }

        /// <summary>
        /// Stop pulsing, returning the color to it's original form.
        /// </summary>
        public void StopPulsing()
        {
            IsPulsing = false;
            BarColor = new Color(originalColor, Opacity);
        }

        /// <summary>
        /// Do the actual pulse operation.
        /// </summary>
        private void DoPulse()
        {
            if (pulseIn)
            {
                // Pulse "in", meaning to the pulse color.
                bool stillPulsing = false;
                if (BarColor.R != PulseColor.R)
                {
                    stillPulsing = true;
                    if (PulseColor.R > BarColor.R)
                        BarColor.R += PulseIncrement;
                    else
                        BarColor.R -= PulseIncrement;
                }

                if (BarColor.G != PulseColor.G)
                {
                    stillPulsing = true;
                    if (PulseColor.G > BarColor.G)
                        BarColor.G += PulseIncrement;
                    else
                        BarColor.G -= PulseIncrement;
                }

                if (BarColor.B != PulseColor.B)
                {
                    stillPulsing = true;
                    if (PulseColor.B > BarColor.B)
                        BarColor.B += PulseIncrement;
                    else
                        BarColor.B -= PulseIncrement;
                }

                if (!stillPulsing)
                {
                    pulseIn = false;
                }
            }
            else
            {
                // Pulse "out", meaning to the original color.
                bool stillPulsingOut = false;
                if (BarColor.R != originalColor.R)
                {
                    stillPulsingOut = true;
                    if (originalColor.R > PulseColor.R)
                        BarColor.R += PulseIncrement;
                    else
                        BarColor.R -= PulseIncrement;
                }

                if (BarColor.G != originalColor.G)
                {
                    stillPulsingOut = true;
                    if (originalColor.G > PulseColor.G)
                        BarColor.G += PulseIncrement;
                    else
                        BarColor.G -= PulseIncrement;
                }

                if (BarColor.B != originalColor.B)
                {
                    stillPulsingOut = true;
                    if (originalColor.B > PulseColor.B)
                        BarColor.B += PulseIncrement;
                    else
                        BarColor.B -= PulseIncrement;
                }

                if (!stillPulsingOut)
                {
                    pulseIn = true;
                }
            }
        }

        private byte Adjust(byte current, byte colorTarget)
        {
            byte finalValue;

            int value = BarColor.R;
            int target = PulseIncrement;
            if (target + value > 255)
            {
                finalValue = 255;
            }
            else if (target + value < 0)
            {
                finalValue = 0;
            }
            else
            {
                bool greaterBefore = current > colorTarget;
                finalValue = (byte)(current + PulseIncrement);
                if (PulseIncrement % target > 0)
                {
                    // It's impossible to reach target by simple incrementing: round instead.
                    if (greaterBefore)
                    {
                        if (finalValue < colorTarget)
                            finalValue = colorTarget;
                    }
                    else
                    {
                        if (finalValue > colorTarget)
                            finalValue = colorTarget;
                    }
                }
            }

            return finalValue;
        }

        /// <summary>
        /// Update the bar, processing how much it should fade a bar.
        /// </summary>
        public void Update()
        {
            double seconds = ServiceManager.Game.DeltaTime;
            CheckForHold(seconds);

            // Do nothing if the bar is not being faded.
            if (state == FadingState.NONE)
                return;

            elapsed += (float)seconds;

            if (state == FadingState.IN)
            {
                if (elapsed > fadeInTime)
                {
                    StopFading();
                }
                else
                {
                    if (this.Opacity < inOpacity)
                    {
                        this.Opacity += .1f;
                    }
                    else
                    {
                        state = FadingState.NONE;
                        holdElapsed = 0f;
                        holding = true;
                    }
                }
            }
            else
            {
                if (elapsed > fadeOutTime)
                {
                    StopFading();
                }
                else
                {
                    if (this.Opacity > outOpacity)
                        this.Opacity -= .025f;
                }
            }

            if (IsPulsing)
            {
                DoPulse();
            }
        }

        /// <summary>
        /// Handler for keeping the bar opaque for a while before fading
        /// </summary>
        void CheckForHold(double secs)
        {
            if (holding)
            {
                holdElapsed += (float)secs;

                if (holdElapsed > holdTime)
                {
                    FadeOut();
                    holding = false;
                }
            }
        }
        #endregion

        /// <summary>
        /// Returns a standard left-oriented HUD_Bar located at the origin
        /// </summary>
        public static HUD_Bar getDefaultHUD_Bar()
        {
            HUD_Bar bar = new HUD_Bar();

            bar.Max = 100;
            bar.Min = 0;
            bar.Value = 100;
            bar.Scale = 1.0f;
            bar.Origin = new Vector2(0, 0);
            bar.BarColor = defaultColor;
            bar.Opacity = defaultOpacity;
            bar.Orientation = HUDOrientation.LEFT;
            bar.TextEnabled = true;
            bar.TOrientation = TextOrientation.BOTTOMLEFT;
            bar.holding = false;

            return bar;
        }

        #region Drawing Methods
        public void Draw()
        {
            if (Orientation == HUDOrientation.LEFT) 
            {
                x = (int)(winWidth * .38f);
                y = (int)((winHeight / 2f) - ((Outline.Height*Scale) / 2f));
                
                DrawLeftBar(x, y);
            }
            else 
            {
                x = (int)(winWidth * .57f);
                y = (int)((winHeight / 2f) - ((Outline.Height*Scale) / 2f));
                DrawRightBar(x, y);
            }
        }

        void DrawLeftBar(int x, int y)
        {
            ServiceManager.Game.Batch.Begin();
            if (TextEnabled)
            {
                ServiceManager.Game.Batch.DrawString(ServiceManager.Game.Font, Value.ToString(),
                        new Vector2(x - Origin.X * .9f * Scale, y - (Origin.Y * Scale) + (Outline.Height * 0.9f) * Scale),
                        Color.White);
            }

            ServiceManager.Game.Batch.Draw(Filler,
                new Rectangle(x,
                    (int)(Outline.Height * Scale) + y - (int)((Value * Outline.Height / 100f) * Scale),
                    (int)(Outline.Width * Scale), (int)(Outline.Height * Scale)),
                new Rectangle(0, Outline.Height - (int)(Value * Outline.Height / 100f), Outline.Width, Outline.Height),
                new Color(BarColor, Opacity), 0f, Origin,
                SpriteEffects.None, 1f);

            //Draw the box around the health bar
            ServiceManager.Game.Batch.Draw(Outline, new Rectangle(x, y, (int)(Outline.Width * Scale), (int)(Outline.Height * Scale)),
                null,
                Color.White, 0f, Origin, SpriteEffects.None, 1f);

            ServiceManager.Game.Batch.End();
        }

        void DrawRightBar(int x, int y)
        {
            ServiceManager.Game.Batch.Begin();
            if (TextEnabled)
            {
                ServiceManager.Game.Batch.DrawString(ServiceManager.Game.Font, Value.ToString(),
                        new Vector2(x - Origin.X * .9f * Scale, y - (Origin.Y * Scale) + (Outline.Height * 0.9f) * Scale),
                        Color.White);
            }

            ServiceManager.Game.Batch.Draw(Filler,
                new Rectangle(x,
                    (int)(Outline.Height * Scale) + y - (int)((Value * Outline.Height / 100f) * Scale),
                    (int)(Outline.Width * Scale), (int)(Outline.Height * Scale)),
                new Rectangle(0, Outline.Height - (int)(Value * Outline.Height / 100f), Outline.Width, Outline.Height),
                new Color(BarColor, Opacity), 0f, Origin,
                SpriteEffects.FlipHorizontally, 1f);

            //Draw the box around the health bar
            ServiceManager.Game.Batch.Draw(Outline, new Rectangle(x, y, (int)(Outline.Width * Scale), (int)(Outline.Height * Scale)),
                null,
                Color.White, 0f, Origin, SpriteEffects.FlipHorizontally, 1f);
            ServiceManager.Game.Batch.End();
        }
        #endregion

    }
}
