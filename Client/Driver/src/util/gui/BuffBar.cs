using System;
using System.Collections.Generic;
using System.Text;
using Client.src.service;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Renderer.SceneTools.Entities;

namespace Client.src.util.game
{
    /// <summary>
    /// The buffbar class keeps track of the buffs on a player, removes buffs that are expired.
    /// </summary>
    public class BuffBar
    {
        #region Constants
        private static readonly int textureWidth = 32;  //Using 32x32 pixel images
        private static readonly int textureHeight = 32;
        private static readonly int viewportWidth = ServiceManager.Game.GraphicsDevice.Viewport.Width;
        private static readonly int padding = 5;  //Number of pixels between each buff and the element right of it.
        private static readonly SpriteFont timerFont = ServiceManager.Game.Font;
        #endregion

        #region Fields
        private List<Buff> buffs;
        private float xPosition;
        private Vector2 texturePosition;
        private Vector2 countdownTextPosition;
        #endregion

        #region Constructors
        /// <summary>
        /// Default constructor for a buff bar.
        /// </summary>
        public BuffBar()
        {
            Initialize();
        }
        #endregion

        #region Properties
        /// <summary>
        /// Returns the number of buffs on the bar.
        /// </summary>
        public int Count 
        {
            get { return buffs.Count; }  
        }
        #endregion

        #region Helpers
        /// <summary>
        /// Initializes the variables in the buff bar.
        /// </summary>
        private void Initialize()
        {
            buffs = new List<Buff>();
            texturePosition = new Vector2();
            countdownTextPosition = new Vector2();
        }

        /// <summary>
        /// Add a buff to the buffbar
        /// </summary>
        /// <param name="buff"></param>
        public void AddBuff(Buff buff)
        {
            buffs.Add(buff);
        }

        /// <summary>
        /// Does maintenance on the buff bar, removes expired buffs.
        /// </summary>
        public void Update()
        {
            RemoveExpiredBuffs();            
        }

        /// <summary>
        /// Clears the collection of buffs.
        /// </summary>
        public void RemoveAllBuffs()
        {
            buffs.Clear();
        }

        /// <summary>
        /// Remove all buffs that have passed their expiration time.
        /// </summary>
        private void RemoveExpiredBuffs()
        {
            List<Buff> buffsToRemove = new List<Buff>();

            foreach (Buff buff in buffs)
            {
                if (buff.IsExpired)
                {
                    buffsToRemove.Add(buff);
                }
            }

            foreach (Buff buff in buffsToRemove)
            {
                buffs.Remove(buff);
            }

            buffsToRemove.Clear();
        }
        #endregion

        #region Drawing
        /// <summary>
        /// Draw all buff icons to the screen.
        /// </summary>
        public void Draw()
        {
            if (buffs.Count == 0)
                return;

            xPosition = (viewportWidth - (Minimap.miniMapWidth +30)); //Leave space for minimap and spacers

            Texture2D texture;
            Color iconColor = Color.White;

            int buffCount = 1;
            ServiceManager.Game.Batch.Begin();
            foreach (Buff buff in buffs)
            {
                texture = buff.BuffIcon;
                texturePosition.X = xPosition - (textureWidth * buffCount) - padding;
                texturePosition.Y = 0; // Top of the screen

                countdownTextPosition.X = xPosition - ((textureWidth * buffCount) - padding);
                countdownTextPosition.Y = textureHeight;
                iconColor.A = buff.Opacity; // .A is a color's alpha
                

                ServiceManager.Game.Batch.Draw(texture, texturePosition, iconColor);
                ServiceManager.Game.Batch.DrawString(timerFont, buff.CountDown.ToString(), countdownTextPosition, Color.White);
                buffCount++;
            }
            ServiceManager.Game.Batch.End();
        }
        #endregion
    }
}
