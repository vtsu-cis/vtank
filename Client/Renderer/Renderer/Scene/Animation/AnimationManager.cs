using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

using System.Text;
namespace Renderer.SceneTools.Animation
{
    /// <summary>
    /// Manages the 3d animations for an object3. NOT CURRENT USED 
    /// </summary>
    public class AnimationManager
    {
        #region Members
        Dictionary<string, Animation3D> animations;
        #endregion

        /// <summary>
        /// Creates a new AnimationManager.
        /// </summary>
        public AnimationManager()
        {
            animations = new Dictionary<string, Animation3D>();

            HardCode(); // Temporary way to get animations into the renderer until an import function is written
        }

        /// <summary>
        /// Gets an animation given it's name
        /// </summary>
        /// <param name="animationName">The name of the animation to get.</param>
        /// <returns>The Animation3D object.</returns>
        public Animation3D GetAnimation(string animationName)
        {
            return animations[animationName];
        }

        /// <summary>
        /// Loads some sample animations for testing purposes. 
        /// </summary>
        private void HardCode()
        {
            Animation3D tmp;

            tmp = new Animation3D(1f, Matrix.CreateTranslation(Vector3.Left * 10), "Cannon0");
            animations.Add("Cannon0_Fire", tmp);
            tmp = new Animation3D(.1f, Matrix.CreateTranslation(Vector3.Zero), "Cannon0");
            animations.Add("Cannon0_Cooldown", tmp);
            tmp = new Animation3D(.1f, Matrix.CreateTranslation(Vector3.Left * 10), "Barrel1");
            animations.Add("Cannon1_Fire", tmp);
            tmp = new Animation3D(1, Matrix.CreateTranslation(Vector3.Right * 10), "Barrel1");
            animations.Add("Cannon1_Cooldown", tmp);
        }

    }
}
