using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Renderer.SceneTools.Entities.Particles;

namespace Renderer.SceneTools.Entities
{
    public class ParticleEmitter : Entity
    {
        #region Private Members
        private float extraTime;
        private Random random;
        private Vector3 previousPosition;
        private bool emitting, stopped;
        private Vector3 velocity;

        private ParticleSystem particles;
        private ParticleEmitterSettings settings;

        private float currentTime;
        #endregion

        #region Constructor
        /// <summary>
        /// Created a new particle Emitter at (0,0,0).
        /// </summary>
        /// <param name="particleEmitterSettingsName">The name of a settings object loaded in the RendererAssetPool.</param>
        public ParticleEmitter(string particleEmitterSettingsName)
            : this(particleEmitterSettingsName, Vector3.Zero)
        {
        }

        /// <summary>
        /// Creates a new ParticleEmitter at a specified position.
        /// </summary>
        /// <param name="particleEmitterSettingsName">The name of a settings object loaded in the RendererAssetPool.</param>
        /// <param name="position">The position to create the ParticleEmitter at.</param>
        public ParticleEmitter(string particleEmitterSettingsName, Vector3 position)
        {
            this.position = position;
            if (RendererAssetPool.ParticleEmitterSettings.ContainsKey(particleEmitterSettingsName))
            {
                LoadParticleEmitterSettings(RendererAssetPool.ParticleEmitterSettings[particleEmitterSettingsName]);
            }
            else
            {
                LoadParticleEmitterSettings(RendererAssetPool.ParticleEmitterSettings["Sample"]);
            }

            this.Update(true);
        }

        /// <summary>
        /// Created a new particle Emitter. By default all particles come from position...?
        /// </summary>
        /// <param name="_settings">A fully specified ParticleEmitterSettings object.</param>
        public ParticleEmitter(ParticleEmitterSettings _settings, Vector3 position) : this(_settings)
        {
            this.position = position;
        }

        /// <summary>
        /// Created a new particle Emitter. By default all particles come from position.
        /// </summary>
        /// <param name="_settings">A fully specified ParticleEmitterSettings object.</param>
        public ParticleEmitter(ParticleEmitterSettings _settings)
        {
            if (_settings.Initialized)
            {
                LoadParticleEmitterSettings(_settings);
            }
            else
            {
                LoadParticleEmitterSettings(RendererAssetPool.ParticleEmitterSettings["Sample"]);
            }
        }

        /// <summary>
        /// Loads the ParticleEmitterSettings object.
        /// </summary>
        /// <param name="_settings">The name of the ParticleEmitterSettings object to load from the RendererAssetPool</param>
        private void LoadParticleEmitterSettings(ParticleEmitterSettings _settings)
        {
            settings = _settings;
            if (RendererAssetPool.ParticleSystemSettings.ContainsKey(settings.ParticleSystemName))
            {
                particles = new ParticleSystem(Renderer.RendererAssetPool.ParticleSystemSettings[settings.ParticleSystemName]);
            }
            else
            {
                particles = new ParticleSystem(Renderer.RendererAssetPool.ParticleSystemSettings["Sample"]);
            }
            random = new Random();
            bounds = new BoundingSphere(position, settings.Radius);
            emitting = true;
            stopped = false;
        }
        #endregion

        #region Propeties
        /// <summary>
        /// The settings that this particle emitter operates under.
        /// </summary>
        public ParticleEmitterSettings Settings
        {
            get { return settings; }
            set { settings = value; }
        }
        /// <summary>
        /// If true then particles are activly being drawn from the particle system.
        /// </summary>
        public bool Emitting
        {
            get { return emitting; }
            set { emitting = value; }
        }

        /// <summary>
        /// Returns true if no particles are being added to or being drawn in the particle system.
        /// </summary>
        public bool Exhausted
        {
            get { return ((particles.Exhausted && !emitting && !Settings.Continuous) || (stopped && particles.Exhausted)); }
        }
        #endregion

        /// <summary>
        /// Adds particles to the particle emitter at the specified time interval.
        /// </summary>
        /// <param name="time">The elapsed time of the past frame.</param>
        /// <param name="updateToggle">Makes sure that this entity only updated once per global Update() call.</param>
        public override void Update(bool updateToggle)
        {
            if (updateToggle != HasUpdated)
            {
                currentTime += GraphicOptions.FrameLength;
                if ((!Settings.Continuous && (currentTime > Settings.LifeSpan)) || stopped)
                {
                    emitting = false;
                }

                previousPosition = Position;
                base.Update(updateToggle);

                if (GraphicOptions.FrameLength > 0 && Emitting)
                {
                    velocity = (Position - previousPosition) / GraphicOptions.FrameLength;
                    float totalTime = extraTime + GraphicOptions.FrameLength;
                    float particleTime = -extraTime;
                    
                    while (totalTime > Settings.IntervalBetweenParticles)
                    {
                        particleTime += Settings.IntervalBetweenParticles;
                        totalTime -= Settings.IntervalBetweenParticles;

                        float delta = particleTime / GraphicOptions.FrameLength;

                        Vector3 partPosition = Vector3.Lerp(previousPosition, position, delta);

                        if (Settings.VaryX)
                        {
                            partPosition += Vector3.UnitX * random.Next(-Settings.Radius, Settings.Radius);
                        }
                        if (Settings.VaryY)
                        {
                            partPosition += Vector3.UnitY * random.Next(-Settings.Radius, Settings.Radius);
                        }
                        if (Settings.VaryZ)
                        {
                            partPosition += Vector3.UnitZ * random.Next(-Settings.Radius, Settings.Radius);
                        }

                        particles.AddParticle(partPosition, velocity, this.Front);
                    }

                    extraTime = totalTime;
                }
                particles.Update();
            }
        }

        /// <summary>
        /// Draws all particles originating from this emitter.
        /// </summary>
        public override void Draw(EffectTechnique technique)
        {
            particles.Draw();
        }

        /// <summary>
        /// A much cheaper Draw() function that keeps particles in the buffer but does not 
        /// draw them.
        /// </summary>
        public void OutOfFrustumDraw()
        {
            particles.OutOfFrustumDraw();
        }

        /// <summary>
        /// Starts the emitter emitting if it is stopped.
        /// </summary>
        public void Start()
        {
            stopped = false;
            emitting = true;
        }

        /// <summary>
        /// Immediatly stops an emitter.
        /// </summary>
        public void Stop()
        {
            stopped = true;
        }

    }
}
