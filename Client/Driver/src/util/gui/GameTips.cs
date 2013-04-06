using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Client.src.service;
using Network.Util;
using Client.src.util.game.data;
using Client.src.util.helpers;

namespace Client.src.util.gui
{
    /// <summary>
    /// Component for automatically displaying ingame tips.
    /// 
    /// Tips fade in and out sequentially at a given interval.
    /// The tips are scrambled during the constructor.
    /// </summary>
    public class GameTips
    {
        public enum TipMode
        {
            FullRotating,
            ContextRotating,
            OnDeath
        }

        #region Fields
        readonly long messageFrequency = 30000; // 30 seconds between tips
        readonly double timeToDisplay = 2.0;   //2 seconds display duration

        Queue<string> tips;
        Rectangle targetRect;
        Texture2D tipBackground;
        SpriteBatch batch;
        string currentTip;
        string prepend = "Tip: ";               //prepend this in front of messages
        long lastRotation;

        TipMode mode;
        bool fadingIn;
        bool fadingOut;
        bool displaying;
        double opacity;
        double displayTimer;
        GameContext context;

        Random rand = new Random();
        #endregion 

        #region Properties
        /// <summary>
        /// Determines whether tips are enabled
        /// 
        /// Setting to false effectively disables this module
        /// </summary>
        public bool Enabled
        {
            get;
            set;
        }

        /// <summary>
        /// Tells us whether the current tip is in a 'displaying state'
        /// 
        /// Toggles itself off when it exceeds the time to display.
        /// </summary>
        private bool Displaying
        {
            get 
            {
                if (displaying == true)
                {
                    displayTimer -= ServiceManager.Game.DeltaTime;
                    if (displayTimer <= 0.0)
                    {
                        displaying = false;
                        fadingOut = true;
                    }
                }
 
                return displaying; 
            }

            set 
            { 
                if (value == true)
                    displayTimer = timeToDisplay;

                displaying = value;                
            }
        }

        #endregion

        #region Constructors/Init

        /// <summary>
        /// Create the default rotating game tip module
        /// </summary>
        public GameTips()
        {
            tips = TipLoader.GetTips();

            mode = TipMode.FullRotating;
            Scramble(new List<string>(tips.ToArray()));
            Initialize();
        }

        /// <summary>
        /// Create a context sensitive rotating tip module that displays on death
        /// </summary>
        /// <param name="context">The game's context</param>
        public GameTips(GameContext context)
        {
            this.context = context;

            tips = TipLoader.GetTips(context);

            mode = TipMode.OnDeath;
            Scramble(new List<string>(tips.ToArray()));
            Initialize();
        }

        /// <summary>
        /// Create a tip module with the specified tip display mode
        /// </summary>
        /// <param name="context">The game's context</param>
        /// <param name="mode">The mode to display tips with</param>
        public GameTips(GameContext context, TipMode mode)
        {
            this.context = context;

            if (mode == TipMode.FullRotating)
                tips = TipLoader.GetTips();
            else
                tips = TipLoader.GetTips(context);

            this.mode = mode;
            Scramble(new List<string>(tips.ToArray()));
            Initialize();
        }

        /// <summary>
        /// Load variables and such
        /// </summary>
        private void Initialize()
        {
            this.Enabled = true;
            currentTip = tips.Dequeue();

            lastRotation = Clock.GetTimeMilliseconds();
            fadingIn = true;
            fadingOut = false;
            Displaying = false;
            opacity = 0;
            batch = ServiceManager.Game.Batch;
            tipBackground = ServiceManager.Resources.GetTexture2D("misc\\background\\tipBackground");
            targetRect = new Rectangle(200, 100, 15, 5);    
        }

        #endregion

        #region Public Methods
        public void ShowNext()
        {
            RotateMessage();
        }

        #endregion

        #region Private Methods
        /// <summary>
        /// Scramble queue of tips
        /// </summary>
        /// <param name="messagesList">The queue as a list</param>
        private void Scramble(List<string> messagesList)
        {
            if (messagesList == null)
            {
                messagesList = new List<string>(tips.ToArray());
            }

            if (messagesList.Count == 0)
                return;

            string current = messagesList[rand.Next(messagesList.Count)];
            tips.Dequeue();
            tips.Enqueue(current);
            messagesList.Remove(current);

            Scramble(messagesList);
        }

        /// <summary>
        /// Rotate the current message out, and set the next message as the current
        /// 
        /// Causes the tip to fade in
        /// </summary>
        private void RotateMessage()
        {
            tips.Enqueue(currentTip);
            currentTip = tips.Dequeue();
            fadingIn = true;
            lastRotation = Clock.GetTimeMilliseconds();
        }

        /// <summary>
        /// Set the position and dimensions of the target rectangle
        /// </summary>
        private void SetRectangle()
        {
            int screenHeight = ServiceManager.Game.Height;
            int screenWidth = ServiceManager.Game.Width;
            int textWidth = (int)ServiceManager.Game.Font.MeasureString(currentTip + prepend).X;
            int textHeight = (int)ServiceManager.Game.Font.MeasureString("W").Y;

            int xPosition = screenWidth / 2 - textWidth/2;
            int yPosition = (int)(screenHeight * .72f);

            targetRect.X = xPosition;
            targetRect.Y = yPosition;
            targetRect.Width = textWidth;
            targetRect.Height = textHeight;
        }

        /// <summary>
        /// Update the opacity of the tip/background box
        /// </summary>
        private void UpdateOpacity()
        {
            if (fadingIn)
            {
                opacity += ServiceManager.Game.DeltaTime;

                if (opacity >= 1.0)
                {
                    opacity = 1.0;
                    fadingIn = false;
                }
            }
            else if (fadingOut)
            {
                opacity -= ServiceManager.Game.DeltaTime;

                if (opacity <= 0)
                {
                    opacity = 0.0;
                    fadingOut = false;
                }
            }
        }

        private bool TimeForNextRotation()
        {
            return Clock.GetTimeMilliseconds() > lastRotation + messageFrequency;
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Update the tips module.
        /// 
        /// Does nothing if tips are disabled.
        /// </summary>
        public void Update()
        {
            if (this.Enabled == false) //Tips are disabled, do nothing
                return;

            if (opacity == 1.0 && Displaying == false)
                Displaying = true;

            UpdateOpacity();
            SetRectangle();

            if (TimeForNextRotation() && mode == TipMode.OnDeath && context.PlayerIsDead())
                RotateMessage();
            else if (TimeForNextRotation() && (mode == TipMode.FullRotating || mode == TipMode.ContextRotating))
                RotateMessage();
        }

        /// <summary>
        /// Draw the current tip (if necessary)
        /// 
        /// Will return if the tip isn't active or tips are
        /// disabled
        /// </summary>
        public void Draw()
        {
            if (this.Enabled == false) //Tips are disabled, do nothing
                return;

            if (opacity <= 0)
                return;

            Color color = Color.White;
            color.A = (byte)(opacity * 255);

            batch.Begin(SpriteBlendMode.AlphaBlend);

            batch.Draw(tipBackground, targetRect, color);
            batch.DrawString(
                ServiceManager.Game.Font, 
                prepend + currentTip, 
                new Vector2(targetRect.X, targetRect.Y), 
                color);

            batch.End();
        }
        #endregion
    }
}
