using System;
using System.Collections.Generic;
using System.Text;
using Renderer.SceneTools.Entities;
using Renderer;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Renderer.SceneTools.Entities.Particles;
using Microsoft.Xna.Framework.Content;

namespace ModelViewer
{
    /// <summary>
    /// Explosion is a class that helps with animating and automatically removing an explosion
    /// after it finishes the animation.
    /// </summary>
    public class Explosion : ParticleEmitter
    {
        #region Members
        ContentManager content;
        float currentTime;
        float lifeSpan;
        #endregion

        public Explosion(ContentManager _content) : base(null, 0.007f, 10)
        {
            content = _content;
            position = Vector3.Zero;
            //GraphicOptions.FrameLength = 1 / 60;
            LoadParticleSettings();
            currentTime = 0;
            lifeSpan = .5f;
        }

        public override void Update(bool updateToggle)
        {
            currentTime += GraphicOptions.FrameLength;
            if (currentTime > lifeSpan)
            {
                emitting = false;
            }
            base.Update(updateToggle);
        }

        private void LoadParticleSettings()
        {
            ParticleSystemSettings settings = new ParticleSystemSettings();

            settings.Capacity = 100;
            settings.Duration = TimeSpan.FromSeconds(.5f);
            settings.DurationRandomness = 0;

            settings.GlobalForce = Vector3.Zero;
            settings.MinForceSensitivity = 0.75f;
            settings.MaxForceSensitivity = 1;
            settings.InertiaSensitivity = 1;

            settings.MinStartHorizontalVelocity = -600;
            settings.MaxStartHorizontalVelocity = 600;
            settings.MinStartVerticalVelocity = -600;
            settings.MaxStartVerticalVelocity = 600;

            settings.MinEndHorizontalVelocity = 20;
            settings.MaxEndHorizontalVelocity = 20;
            settings.MinEndVerticalVelocity = 20;
            settings.MaxEndVerticalVelocity = 20;

            settings.MinStartSize = 1;
            settings.MaxStartSize = 1;
            settings.MinEndSize = 1;
            settings.MaxEndSize = 1;

            settings.MinRotateSpeed = 0;
            settings.MaxRotateSpeed = 50;

            settings.MinColor = Color.DarkGray;
            settings.MaxColor = Color.Gray;

            settings.SourceBlend = Blend.SourceAlpha;
            settings.DestinationBlend = Blend.One;

            Texture2D texture = content.Load<Texture2D>("explosion");

            particles = new ParticleSystem(texture, settings);
        }
    }
}
