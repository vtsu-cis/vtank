/*!
    \file   State.cs
    \brief  Base State object, which all VTank states must inherit.
    \author (C) Copyright 2009 by Vermont Technical College
*/
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Client.src.service;
using Microsoft.Xna.Framework.Content;
using Client.src.util;
using Microsoft.Xna.Framework.Media;
using Client.src.util.game;

namespace Client.src.states.gamestate
{
    /// <summary>
    /// The State class is required to be inherited by any class who wishes to be a VTank state.
    /// </summary>
    public abstract class State
    {
        #region Members
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets whether the background should be drawn for this state.
        /// </summary>
        public bool DrawBackground
        {
            get;
            set;
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Simple construction which loads the background images.
        /// </summary>
        public State() : this(true) {}

        /// <summary>
        /// Constructor which lets the user configure whether or not the background
        /// should be drawn in this state.
        /// </summary>
        /// <param name="drawBackground"></param>
        public State(bool drawBackground)
        {
            DrawBackground = drawBackground;
            if (drawBackground)
            {
                LoadBackground();
            }
        }
        #endregion

        /// <summary>
        /// Loads the textures for drawing a background
        /// </summary>
        private void LoadBackground()
        {
            if (ServiceManager.Game.BackgroundMovie == null)
            {
                ServiceManager.Game.CreateBackgroundMovie();
            }
        }

        /// <summary>
        /// Draws the standard background
        /// </summary>
        public void DrawBack()
        {
            if (DrawBackground)
            {
                try
                {
                    Rectangle titleSafeArea = ServiceManager.Game.GraphicsDevice.Viewport.TitleSafeArea;
                    Texture2D frame = ServiceManager.Game.BackgroundMovie.CurrentFrame;
                    SpriteBatch batch = ServiceManager.Game.Batch;
                    batch.Begin(SpriteBlendMode.None,
                        SpriteSortMode.Immediate, SaveStateMode.None);
                    
                    batch.Draw(frame, titleSafeArea, Color.White);

                    batch.End();
                }
                catch (AccessViolationException ex)
                {
                    Console.Error.WriteLine("[Access Violation Exception in State#DrawBack()]");
                    Console.Error.WriteLine(ex.Message);
                }
            }
        }

        public abstract void LoadContent();

        public abstract void UnloadContent();

        public abstract void Initialize();

        public abstract void Update();

        public abstract void Draw();
    }
}
