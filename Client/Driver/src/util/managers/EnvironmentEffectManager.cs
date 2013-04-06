using System;
using System.Collections.Generic;
using System.Text;
using Client.src.util.game;
using Client.src.service;
using Microsoft.Xna.Framework;
using Renderer.SceneTools.Entities;
using Renderer.SceneTools.Entities.Particles;

namespace Client.src.util
{
    /// <summary>
    /// Manager class that tracks and handles active environment effects.
    /// </summary>
    public class EnvironmentEffectManager
    {
        #region Members
        private Dictionary<int, EnvironmentProperty> envProps;
        #endregion

        #region Constructors
        /// <summary>
        /// Default constructor for the manager.
        /// </summary>
        public EnvironmentEffectManager()
        {
            envProps = new Dictionary<int, EnvironmentProperty>();
        }
        #endregion

        #region Methods
        /// <summary>
        /// Add an effect to the collection of EnvironmentProperties
        /// </summary>
        /// <param name="envProp">The environment property.</param>
        /// <param name="id">The environment property's Id</param>
        /// <param name="position">The position to add it in the scene.</param>
        public void AddEffect(EnvironmentProperty envProp, int id, Vector3 position)
        {
            if (envProps.ContainsKey(id))
                RemoveEffect(id);
            
            ParticleEmitter emitter = new ParticleEmitter(envProp.ParticleEffectName);
            ParticleEmitterSettings settings = emitter.Settings;

            settings.Radius = (int)Math.Floor(envProp.AoERadius);
            ParticleEmitter __emitterCopy = new ParticleEmitter(settings);

            __emitterCopy.Position = position;

            envProp.RenderID = ServiceManager.Scene.Add(__emitterCopy, 3);
            envProp.SetCreationTime();
            envProps[id] = envProp;
        }

        /// <summary>
        /// Remove an effect from the collection of EnvironmentProperties
        /// </summary>
        /// <param name="id">The effect's Id</param>
        public bool RemoveEffect(int id)
        {
            if (envProps.ContainsKey(id))
            {
                ServiceManager.Scene.Delete(envProps[id].RenderID);
                return envProps.Remove(id);
            }

            return false;
        }

        /// <summary>
        /// Get an environment effect from the collection.
        /// </summary>
        /// <param name="effectId">The id of the effect.</param>
        /// <returns>The environmentProperty object.</returns>
        public EnvironmentProperty GetEffect(int effectId)
        {
            if (envProps.ContainsKey(effectId))
            {
                return envProps[effectId];
            }
            
            return null;
        }

        /// <summary>
        /// Remove all environment properties from the collection and the scene.
        /// </summary>
        public void RemoveAllEffects()
        {
            int[] keys = new int[envProps.Count];
            int keyCount = 0;
            foreach (int key in envProps.Keys)
                keys[keyCount++] = envProps[key].ID;

            for (int j = 0; j < keys.Length; ++j)
                RemoveEffect(keys[j]);
        }

        /// <summary>
        /// Perform maintenance on the collection.
        /// </summary>
        public void Update()
        {
            this.RemoveExpiredEffects();
        }
        
        /// <summary>
        /// Remove effects that have passed their expiration time.
        /// </summary>
        private void RemoveExpiredEffects()
        {
            Stack<int> removeList = new Stack<int>();
            foreach (KeyValuePair<int,EnvironmentProperty> envProp in envProps)
            {
                if (envProp.Value.Expired)
                {
                    removeList.Push(envProp.Key);
                }
            }

            while (removeList.Count > 0)
            {
                RemoveEffect(removeList.Pop());
            }
        }
        #endregion
    }
}
