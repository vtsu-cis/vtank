using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using Renderer.SceneTools;
using Renderer;
using Microsoft.Xna.Framework.Graphics;

namespace Renderer.Utils
{
    /// <summary>
    /// A mouse pointer that world in game world coordinates. 
    /// </summary>
    public class WorldMouse
    {
        #region Members
        private static MouseState mouseState;
        private static Vector3 CurrentMousePosition;
        private static Ray worldRay;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new WorldMouse object.
        /// </summary>
        public WorldMouse()
        {
            
            mouseState = new MouseState();
        }
        #endregion

        #region Properties
        /// <summary>
        /// Public get for the mouse's world position at a depth of .9885
        /// </summary>
        public static Vector3 Position
        {
            get 
            { 
                Update(); 
                return CurrentMousePosition; 
            }
        }

        /// <summary>
        /// Public get for the ray that originates at the cursor and projects into the screen.
        /// </summary>
        public static Ray WorldRay
        {
            get
            {
                Update();
                return worldRay;
            }
        }


        #endregion

        /// <summary>
        /// Saves the new CurrentMousePosition and worldRay based on the position of the cursor.
        /// </summary>
        public static void Update()
        { 
            mouseState = Mouse.GetState();
            Vector3 pos = new Vector3(mouseState.X, mouseState.Y, .9885f); // Note: .9885f is the correct z value to get tracking to work with the VTank overhead camera.
            CurrentMousePosition = GraphicOptions.graphics.GraphicsDevice.Viewport.Unproject(pos, GraphicOptions.CurrentCamera.Projection, GraphicOptions.CurrentCamera.View, Matrix.Identity);
            Vector3 pos1 = GraphicOptions.graphics.GraphicsDevice.Viewport.Unproject(new Vector3(mouseState.X, mouseState.Y, 0), GraphicOptions.CurrentCamera.Projection, GraphicOptions.CurrentCamera.View, Matrix.Identity);
            Vector3 pos2 = GraphicOptions.graphics.GraphicsDevice.Viewport.Unproject(new Vector3(mouseState.X, mouseState.Y, 1), GraphicOptions.CurrentCamera.Projection, GraphicOptions.CurrentCamera.View, Matrix.Identity);
            worldRay = new Ray(pos1, Vector3.Normalize(pos2 - pos1));
        }
    }
}
