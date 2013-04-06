using System;
using System.Collections.Generic;

using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Renderer.SceneTools.Animation
{
    /// <summary>
    /// An animation for an Object3. NOT CURRENTLY USED.
    /// </summary>
    public class Animation3D
    {
        #region Members
        private Matrix transform;
        private string meshName;
        private float duration;
        private Matrix currentState;
        private double totalTime;
        #endregion

        /// <summary>
        /// Creates a new Animation3D Object with one mesh transform and a duration. The mesh will linerarly interpret to the transform over the duration. 
        /// </summary>
        /// <param name="_duration">The total length of time for the animaiton.</param>
        /// <param name="_mesh">The name of the mesh to animate.</param>
        /// <param name="_transform">The transformation to apply to the mesh.</param>
        public Animation3D(float _duration, Matrix _transform, string _mesh)
        {
            duration = _duration;
            transform = _transform;
            meshName = _mesh;
            currentState = Matrix.Identity;
        }

        #region Properties
        /// <summary>
        /// Gets the name of the mesh to apply this animation to.
        /// </summary>
        public string MeshName
        {
            get { return meshName; }
        }
        #endregion

        public Matrix Run(Matrix currentState)
        {
            totalTime += GraphicOptions.FrameLength;

            if (totalTime >= duration)
            {
                totalTime = 0;
                return Matrix.CreateTranslation(0,0,0);
            }

            Matrix output = Matrix.Lerp(currentState, transform, duration);
            return output;
        }

    }
}
