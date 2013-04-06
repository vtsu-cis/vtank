using System;
using System.Collections.Generic;
using System.Text;
using Client.src.service;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.IO;

namespace Client.src.util
{
    /// <summary>
    /// The MouseCursor class is used to produce a custom cursor for the game.
    /// </summary>
    public class MouseCursor
    {
        #region Constants
        private readonly string pathToCursors = "textures\\Cursors\\";
        #endregion

        #region Fields
        private Texture2D cursorTexture;
        private MouseState mouseState;
        private Vector2 mouseLocation;
        private bool customCursorIsEnabled;
        #endregion

        #region Properties
        /// <summary>
        /// Indicates whether the custom cursor is currently being used.
        /// </summary>
        public bool CustomCursorIsEnabled 
        {
            get { return customCursorIsEnabled; }
            set { customCursorIsEnabled = value; } 
        }

        /// <summary>
        /// The width of the cursor's texture.
        /// </summary>
        private int TextureWidth
        {
            get { return cursorTexture.Height; }
        }

        /// <summary>
        /// The height of the cursor's texture.
        /// </summary>
        private int TextureHeight
        {
            get { return cursorTexture.Width; }
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Default empty constructor
        /// </summary>
        public MouseCursor()
        {
            this.customCursorIsEnabled = false;
        }

        /// <summary>
        /// MouseCursor constructor that takes a texture
        /// </summary>
        /// <param name="cursorTexture">The texture to use for the cursor</param>
        public MouseCursor(Texture2D cursorTexture) : base()
        {
            this.cursorTexture = cursorTexture;
        }

        /// <summary>
        /// MouseCursor constructor that takes the name of the texture
        /// </summary>
        /// <param name="textureName">The name of the texture to use.</param>
        public MouseCursor(string textureName) : base()
        {
            if (!File.Exists(pathToCursors + textureName))
            {
                // Resort to default cursor name.
                textureName = "crosshair_green_reddot";
            }

            this.cursorTexture = ServiceManager.Resources.GetTexture2D(pathToCursors + textureName);
        }

        #endregion

        #region Methods
        /// <summary>
        /// Enable the custom cursor
        /// </summary>
        public void EnableCustomCursor()
        {
            ServiceManager.Game.IsMouseVisible = false;
            this.customCursorIsEnabled = true;
        }

        /// <summary>
        /// Disable the custom cursor
        /// </summary>
        public void DisableCustomCursor()
        {
            ServiceManager.Game.IsMouseVisible = true;
            this.customCursorIsEnabled = false;
        }

        /// <summary>
        /// Update the location and state of the cursor.
        /// </summary>
        public void Update()
        {
            if (customCursorIsEnabled == false)
                return;

            mouseState = Mouse.GetState();
            mouseLocation.X = mouseState.X - this.TextureWidth/2;
            mouseLocation.Y = mouseState.Y - this.TextureHeight/2;
        }

        /// <summary>
        /// Draw the cursor (if enabled)
        /// </summary>
        public void Draw()
        {
            if (customCursorIsEnabled == false)
                return;

            ServiceManager.Game.Batch.Begin();
            ServiceManager.Game.Batch.Draw(cursorTexture, mouseLocation, Color.White);
            ServiceManager.Game.Batch.End();
        }
        #endregion
    }
}
