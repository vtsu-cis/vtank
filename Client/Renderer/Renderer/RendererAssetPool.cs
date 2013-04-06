using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Renderer.SceneTools.Entities.Particles;
using Renderer.SceneTools.Effects;
using Renderer.SceneTools.ShadowMapping;

namespace Renderer
{
    /// <summary>
    /// Keeps track of graphics resources that the renderer is currently or will soon use. 
    /// </summary>
    public static class RendererAssetPool
    {
       
        private static Dictionary<string, Texture2D> textures2D = new Dictionary<string, Texture2D>();
        private static Dictionary<string, ParticleSystemSettings> particleSystemSettings = new Dictionary<string, ParticleSystemSettings>();
        private static Dictionary<string, ParticleEmitterSettings> particleEmitterSettings = new Dictionary<string, ParticleEmitterSettings>();
        private static Dictionary<string, Texture2D> particles = new Dictionary<string, Texture2D>();

        public static UniversalEffect UniversalEffect
        {
            get;
            set;
        }
        public static Effect ParticleEffect
        {
            get;
            set;
        }

        /// <summary>
        /// Public get/set for the dictionary of Texture2D objects.
        /// </summary>
        public static Dictionary<string, Texture2D> Textures2D
        {
            get { return textures2D; }
            set { value = textures2D; }
        }

        /// <summary>
        /// Public get/set for the dictionary of ParticleSystemSettings objects.
        /// </summary>
        public static Dictionary<string, ParticleSystemSettings> ParticleSystemSettings
        {
            get { return particleSystemSettings; }
            set { particleSystemSettings = value; }
        }

        /// <summary>
        /// Public get/set for the dictionary of ParticleEmitterSettings objects.
        /// </summary>
        public static Dictionary<string, ParticleEmitterSettings> ParticleEmitterSettings
        {
            get { return particleEmitterSettings; }
            set { particleEmitterSettings = value; }
        }

        /// <summary>
        /// Public get/set for the dictionary of Texture2D objects that are used in particle emitters.
        /// </summary>
        public static Dictionary<string, Texture2D> Particles
        {
            get { return particles; }
            set { particles = value; }
        }

        public static bool DrawShadows
        {
            get;
            set;
        }
    }
}
