using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Renderer;
using Renderer.SceneTools.Entities.Particles;
using Client.src.service;

namespace Client.src.util
{
    public static class RendererAssetLoader
    {
        #region Members
        private static bool hasInitialized = false;
        #endregion

        #region Properties
        public static bool HasInitialized
        {
            get { return hasInitialized; }
        }
        #endregion

        /// <summary>
        /// Add a call to your Initialization method here
        /// </summary>
        #region Initialize
        public static void Initialize()
        {
            hasInitialized = true;
            LoadEffect();
            LoadParticles();
            LoadEmitterSettings();
            LoadParticleSystemSettings();
        }
        #endregion

        #region Load Helper
        private static void LoadEffect()
        {
            Effect effect = ServiceManager.Resources.GetEffect("ParticleEffect");
            RendererAssetPool.ParticleEffect = effect;
        }

        private static void LoadParticles()
        {   string dir = Path.Combine(System.Reflection.Assembly.GetExecutingAssembly().Location, "../Content/particles");
            DirectoryInfo di = new DirectoryInfo(dir);
            FileInfo[] files = di.GetFiles("*.xnb");
            foreach (System.IO.FileInfo file in files)
            {
                string name = Path.GetFileNameWithoutExtension(file.Name);
                RendererAssetPool.Particles.Add(name, ServiceManager.Resources.GetParticle(name));
            }
        }

        private static void LoadEmitterSettings()
        {
            string dir = Path.Combine(System.Reflection.Assembly.GetExecutingAssembly().Location, "../Content/resources/particleEmitterSettings");
            DirectoryInfo di = new DirectoryInfo(dir);
            FileInfo[] files = di.GetFiles("*.vtpes");
            foreach (System.IO.FileInfo file in files)
            {
                StreamReader read = new StreamReader(file.FullName);
                ParticleEmitterSettings peset = new ParticleEmitterSettings();
                peset.Load(read);
                read.Close();
                RendererAssetPool.ParticleEmitterSettings.Add(Path.GetFileNameWithoutExtension(file.Name), peset);
            }
        }

        private static void LoadParticleSystemSettings()
        {
            string dir = System.IO.Path.Combine(System.Reflection.Assembly.GetExecutingAssembly().Location, "../Content/resources/particleSystemSettings");
            System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(dir);
            System.IO.FileInfo[] files = di.GetFiles("*.vtpss");
            foreach (System.IO.FileInfo file in files)
            {
                StreamReader read = new StreamReader(file.FullName);
                ParticleSystemSettings psset = new ParticleSystemSettings();
                psset.Load(read);
                read.Close();
                RendererAssetPool.ParticleSystemSettings.Add(Path.GetFileNameWithoutExtension(file.Name), psset);
            }
        }
        #endregion

    }
}

