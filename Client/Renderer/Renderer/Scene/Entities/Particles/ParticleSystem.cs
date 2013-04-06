using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Renderer.SceneTools.Entities.Particles;
using Renderer.SceneTools.Effects;

namespace Renderer.SceneTools.Entities
{
    /// <summary>
    /// This class represents a group of Particle objects.
    /// </summary>
    public class ParticleSystem
    {
        #region Members
        private Particle[] particles;
        private Texture2D texture;
        int firstCurrentlyDrawingParticle;
        int firstNewUnuploadedParticle;
        int firstUnusedParticle;
        int firstUndrawnButStoredParticle;

        protected ParticleSystemSettings settings;
        Effect particleEffect;

        DynamicVertexBuffer vertBuffer;
        VertexDeclaration vertDeclaration;

        float currentTime;
        int drawCounter;
        Random random;
        private bool exhausted;
        #endregion

        #region Properties
        /// <summary>
        /// Returns true if the system has no particles in it.
        /// </summary>
        public bool Exhausted
        {
            get { return exhausted; }
        }
        #endregion

        #region Constructor / Initialize
        /// <summary>
        /// Creates a new particle system from a ParticleSystemSettings object.
        /// </summary>
        /// <param name="_settings">The ParticleSystemSettings object.</param>
        public ParticleSystem(ParticleSystemSettings _settings)
        {
            if (_settings.Initialized)
            {
                settings = _settings;
            }
            else
            {
                settings = RendererAssetPool.ParticleSystemSettings["Sample"];
            }
            texture = settings.Texture;
            particles = new Particle[settings.Capacity];
            random = new Random();
            currentTime = 0;

            vertDeclaration = new VertexDeclaration(GraphicOptions.graphics.GraphicsDevice,
                                                     Particle.VertexElements);

            vertBuffer = new DynamicVertexBuffer(GraphicOptions.graphics.GraphicsDevice,
                                                settings.Capacity * Particle.SizeInBytes,
                                                BufferUsage.WriteOnly | BufferUsage.Points);

            LoadEffect();

        }

        #endregion

        #region Load Effect

        /// <summary>
        /// Loads the particle effect shader from the RendererAssetPool.
        /// </summary>
        private void LoadEffect()
        {
            particleEffect = RendererAssetPool.ParticleEffect.Clone(GraphicOptions.graphics.GraphicsDevice);
            EffectParameterCollection parameters = particleEffect.Parameters;

            parameters["ViewportHeight"].SetValue(GraphicOptions.graphics.GraphicsDevice.Viewport.Height);
            parameters["Texture"].SetValue(texture);

            parameters["Duration"].SetValue((float)settings.Duration.TotalSeconds);
            parameters["DurationRandomness"].SetValue(settings.DurationRandomness);

            parameters["GlobalForce"].SetValue(settings.GlobalForce);

            parameters["ForceSensitivity"].SetValue(
                new Vector2(settings.MinForceSensitivity, settings.MaxForceSensitivity));

            parameters["EndHorizontalVelocity"].SetValue(
                new Vector2(settings.MinEndHorizontalVelocity, settings.MaxEndHorizontalVelocity));
            parameters["EndVerticalVelocity"].SetValue( 
                new Vector2(settings.MinEndVerticalVelocity, settings.MaxEndVerticalVelocity));

            parameters["MinColor"].SetValue(settings.MinColor.ToVector4());
            parameters["MaxColor"].SetValue(settings.MaxColor.ToVector4());

            parameters["StartSize"].SetValue(
                new Vector2(settings.MinStartSize, settings.MaxStartSize));
            parameters["EndSize"].SetValue(
                new Vector2(settings.MinEndSize, settings.MaxEndSize));

            parameters["RotateSpeed"].SetValue(new Vector2(settings.MinRotateSpeed, settings.MaxRotateSpeed));

            if ((settings.MinRotateSpeed == 0) && (settings.MaxRotateSpeed == 0))
                particleEffect.CurrentTechnique = particleEffect.Techniques["NoRotation"];
            else
                particleEffect.CurrentTechnique = particleEffect.Techniques["Rotation"];
            
        }
        #endregion

        #region Update
        /// <summary>
        /// Updated the View and Projection of the effect and puts particles through their lifecycle.
        /// </summary>
        public void Update()
        {
            currentTime += GraphicOptions.FrameLength;

            StoreUnwantedDrawingParticles();
            RemoveExpiredStoredParticles();

            if (firstCurrentlyDrawingParticle == firstUnusedParticle)
            {
                currentTime = 0;
                exhausted = true;
            }
            else
                exhausted = false;

            if (firstUndrawnButStoredParticle == firstNewUnuploadedParticle)
                drawCounter = 0;
        }

        #region Update Helper
        /// <summary>
        /// Puts 'dead' particles into storage where they cannot be removed from the GPU while it is drawing.
        /// </summary>
        private void StoreUnwantedDrawingParticles()
        {
            float particleDuration = (float)settings.Duration.TotalSeconds;

            while (firstCurrentlyDrawingParticle != firstNewUnuploadedParticle)
            {
                float age = currentTime - particles[firstCurrentlyDrawingParticle].Time;

                if (age < particleDuration)
                    break;

                particles[firstCurrentlyDrawingParticle].Time = drawCounter;

                firstCurrentlyDrawingParticle = (firstCurrentlyDrawingParticle + 1) % (particles.Length-1);
            }
        }

        /// <summary>
        /// Removes particles from the buffer after they have been dead for 3 frames. 
        /// </summary>
        private void RemoveExpiredStoredParticles()
        {
            while (firstUndrawnButStoredParticle != firstCurrentlyDrawingParticle)
            {
                int age = drawCounter = (int)particles[firstUndrawnButStoredParticle].Time;

                if (age < 3)
                    break;

                firstUndrawnButStoredParticle = (firstUndrawnButStoredParticle + 1) % (particles.Length-1);
            }
        }

        #endregion
        #endregion

        #region Draw
        /// <summary>
        /// A much cheaper Draw() function that keeps particles in the buffer but does not 
        /// draw them.
        /// </summary>
        public void OutOfFrustumDraw()
        {
            if (vertBuffer.IsContentLost)
            {
                vertBuffer.SetData(particles);
            }

            if (firstNewUnuploadedParticle != firstUnusedParticle)
            {
                UploadNewParticleToVertexBufer();
            }
            drawCounter++;
        }
        /// <summary>
        /// Adds new vertecies to the vertex buffer, sets the renderstate and draws.
        /// </summary>
        public void Draw()
        {
            if (vertBuffer.IsContentLost)
            {
                vertBuffer.SetData(particles);
            }

            if (firstNewUnuploadedParticle != firstUnusedParticle)
        {
                UploadNewParticleToVertexBufer();
            }

            if (firstCurrentlyDrawingParticle != firstUnusedParticle)
            {
                //VertexDeclaration tmpVD = GraphicOptions.graphics.GraphicsDevice.VertexDeclaration;

                SetParticleRenderStates();

                particleEffect.Parameters["View"].SetValue(GraphicOptions.CurrentCamera.View);
                particleEffect.Parameters["Projection"].SetValue(GraphicOptions.CurrentCamera.Projection);
                particleEffect.Parameters["ViewportHeight"].SetValue(GraphicOptions.graphics.GraphicsDevice.Viewport.Height);
                particleEffect.Parameters["CurrentTime"].SetValue(currentTime);

                GraphicOptions.graphics.GraphicsDevice.Vertices[0].
                    SetSource(vertBuffer, 0, Particle.SizeInBytes);

                GraphicOptions.graphics.GraphicsDevice.VertexDeclaration = vertDeclaration;
                
                    
                particleEffect.Begin();
                foreach (EffectPass pass in particleEffect.CurrentTechnique.Passes)
                {
                    pass.Begin();
                    if (firstCurrentlyDrawingParticle < firstUnusedParticle)
                    {
                        GraphicOptions.graphics.GraphicsDevice.
                            DrawPrimitives(PrimitiveType.PointList, firstCurrentlyDrawingParticle, firstUnusedParticle - firstCurrentlyDrawingParticle);
                    }
                    else
                    {
                        GraphicOptions.graphics.GraphicsDevice.
                            DrawPrimitives(PrimitiveType.PointList,
                                         firstCurrentlyDrawingParticle,
                                         particles.Length - firstCurrentlyDrawingParticle);

                        if (firstUnusedParticle > 0)
                        {
                            GraphicOptions.graphics.GraphicsDevice.
                                DrawPrimitives(PrimitiveType.PointList, 0, firstUnusedParticle);
                        }
                    }
                    pass.End();

                }
                particleEffect.End();
                GraphicOptions.graphics.GraphicsDevice.RenderState.SourceBlend = Blend.One;
                GraphicOptions.graphics.GraphicsDevice.RenderState.DestinationBlend = Blend.Zero;
                GraphicOptions.graphics.GraphicsDevice.RenderState.AlphaFunction = CompareFunction.Always;
                GraphicOptions.graphics.GraphicsDevice.RenderState.AlphaBlendEnable = false;

                //GraphicOptions.graphics.GraphicsDevice.VertexDeclaration = tmpVD;
                GraphicOptions.graphics.GraphicsDevice.RenderState.PointSpriteEnable = false;
                GraphicOptions.graphics.GraphicsDevice.RenderState.DepthBufferWriteEnable = true;
            }
            drawCounter++;
        }

        #region Draw Helpers
        /// <summary>
        /// Uploads any recently added particles to the vertex buffer.
        /// </summary>
        private void UploadNewParticleToVertexBufer()
        {

            int size = Particle.SizeInBytes;

            if (firstNewUnuploadedParticle < firstUnusedParticle)
            {
                vertBuffer.SetData(firstNewUnuploadedParticle * size, particles,
                    firstNewUnuploadedParticle, firstUnusedParticle - firstNewUnuploadedParticle, size, SetDataOptions.NoOverwrite);
            }

            else
            {
                vertBuffer.SetData(firstNewUnuploadedParticle * size, particles,
                    firstNewUnuploadedParticle, particles.Length - firstNewUnuploadedParticle, size, SetDataOptions.NoOverwrite);
                if (firstUnusedParticle > 0)
                {
                    vertBuffer.SetData(0, particles,
                        0, firstUnusedParticle, size, SetDataOptions.NoOverwrite);
                }
            }

            firstNewUnuploadedParticle = firstUnusedParticle;
        }

        /// <summary>
        /// Configures the RenderState to draw particles
        /// </summary>
        /// <param name="renderState"></param>
        private void SetParticleRenderStates()
        {
            RenderState renderState = GraphicOptions.graphics.GraphicsDevice.RenderState;

            renderState.PointSpriteEnable = true;
            renderState.PointSizeMax = 256;

            renderState.AlphaBlendEnable = true;
            renderState.AlphaBlendOperation = BlendFunction.Add;
            renderState.SourceBlend = settings.SourceBlend;
            renderState.DestinationBlend = settings.DestinationBlend;

            renderState.AlphaTestEnable = true;
            renderState.AlphaFunction = CompareFunction.Greater;
            renderState.ReferenceAlpha = 0;

            renderState.DepthBufferEnable = true;
            renderState.DepthBufferWriteEnable = false;

        }
        #endregion
        #endregion

        #region Methods

        /// <summary>
        /// Adds a new particle to the system with some value randomization
        /// </summary>
        /// <param name="_position">The position created by the emitter.</param>
        /// <param name="_velocity">The emitter's velocity.</param>
        public void AddParticle(Vector3 _position, Vector3 _velocity, Vector3 _direction)
        {
            int nextUnusedParticle = (firstUnusedParticle + 1) % (particles.Length-1);

            if (nextUnusedParticle == firstUndrawnButStoredParticle)
            {
                return;//The system is full
            }

            _velocity *= settings.InertiaSensitivity;
            if (settings.DirFromRotation)
                _velocity += _direction * settings.InitalSpeed;
            else
                _velocity += settings.Direction* settings.InitalSpeed;

            double horizontalAngle = random.NextDouble() * MathHelper.TwoPi;

            _velocity.X += MathHelper.Lerp(settings.MinStartHorizontalVelocity, 
                                          settings.MaxStartHorizontalVelocity, 
                                          (float)random.NextDouble())
                                  * (float)Math.Cos(horizontalAngle);
            _velocity.Y += MathHelper.Lerp(settings.MinStartHorizontalVelocity,
                                          settings.MaxStartHorizontalVelocity,
                                          (float)random.NextDouble())
                                  * (float)Math.Sin(horizontalAngle);

            _velocity.Z += MathHelper.Lerp(settings.MinStartVerticalVelocity,
                                           settings.MaxStartVerticalVelocity,
                                           (float)random.NextDouble());

            Color randomValues = new Color((byte)random.Next(255),
                                           (byte)random.Next(255),
                                           (byte)random.Next(255),
                                           (byte)random.Next(255));

            particles[firstUnusedParticle].Position = _position;
            particles[firstUnusedParticle].Velocity = _velocity;
            particles[firstUnusedParticle].RandomValues = randomValues;              
            particles[firstUnusedParticle].Time = currentTime;

            firstUnusedParticle = nextUnusedParticle;
        }
        #endregion



    }
}
