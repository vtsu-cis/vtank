using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Renderer.Utils;

namespace Renderer
{
    /// <summary>
    /// This class defines the graphics options that are used throughout the library.
    /// </summary>
    public class GraphicOptions
    {
        #region Members
        public static GraphicsDeviceManager graphics;
        public static SpriteBatch batch;
        public static ContentManager content;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new GraphicOptions object with a pre-existing GraphicsDeviceManager;
        /// </summary>
        /// <param name="game">The game that this GraphicObject is for.</param>
        public GraphicOptions(GraphicsDeviceManager _graphics, ContentManager _content)
        {
            graphics = _graphics;
            content = _content;
            batch = new SpriteBatch(graphics.GraphicsDevice);
            BackgroundColor = Color.LightGray;
            ShadowMaps = false;
            ShadingSupport = true;
        }
        #endregion

        /// <summary>
        /// Public get/set for the renderers ContentManager object.
        /// </summary>
        public static ContentManager Content
        {
            get {return content;}
            set { content = value; }
        }

        /// <summary>
        /// Get/Set for the Frame length in seconds
        /// </summary>
        public static float FrameLength
        {
            get;
            set;
        }

        /// <summary>
        /// If true, then lighting effects will render.
        /// </summary>
        public static Boolean ShadingSupport
        {
            get;
            set;
        }

        /// <summary>
        /// If true, then shadow maps will render.
        /// </summary>
        public static Boolean ShadowMaps
        {
            get;
            set;
        }

        /// <summary>
        /// If true, walls between the camera and player will render transparently. 
        /// </summary>
        public static bool TransparentWalls
        {
            get;
            set;
        }

        /// <summary>
        /// Get/set for the the Texture Quality
        /// </summary>
        public static TextureQuality TextureQuality
        {
            get;
            set;
        }

        /// <summary>
        /// Get/set for antialiasing. 
        /// </summary>
        public static Boolean AntiAliasing
        {
            get { return graphics.PreferMultiSampling; }
            set { graphics.PreferMultiSampling = value; Commit(); }
        }

        /// <summary>
        /// Get/set for fullscreen.
        /// </summary>
        public static Boolean FullScreen
        {
            get { return graphics.IsFullScreen; }
            set { graphics.IsFullScreen = value; Commit(); }
        }

        /// <summary>
        /// A static store for the Current Camera
        /// </summary>
        public static Renderer.SceneTools.Entities.Camera CurrentCamera
        {
            get;
            set;
        }

        /// <summary>
        /// Get/Set for the clear color of the screen. 
        /// </summary>
        public static Color BackgroundColor
        {
            get;
            set;
        }

        /// <summary>
        /// Applys any changes made to the graphics options.
        /// </summary>
        private static void Commit()
        {
            graphics.ApplyChanges();
        }

        /// <summary>
        /// Call this method to toggle between full-screen and windowed view. Call Commit() to apply changes.
        /// </summary>
        public void ToggleFullScreen()
        {
            graphics.ToggleFullScreen(); Commit();
        }


    }
}
