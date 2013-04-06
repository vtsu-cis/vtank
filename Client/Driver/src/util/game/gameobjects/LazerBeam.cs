using System;
using System.Collections.Generic;
using System.Text;
using Renderer.SceneTools.Entities;
using Microsoft.Xna.Framework.Graphics;
using Renderer.SceneTools.Effects;
using Renderer;
using Client.src.service;
using Client.src.states.gamestate;
using Microsoft.Xna.Framework;

namespace Client.src.util.game
{
    public class MobileEmitter : ParticleEmitter
    {
        #region Members
        int emitterRenderID;

        float velocity;
        float angle;
        long flightTime;
        long startTime;
        Vector3 start;
        Vector3 end;
        #endregion

        #region Constructors
        /// <summary>
        /// Constructor for a MobileEmitter
        /// </summary>
        /// <param name="velocity">The speed at which the emitter should travel.</param>
        /// <param name="emitter">The particle emitter it should become.</param>
        /// <param name="startPosition">The point at which the emitter should originate.</param>
        /// <param name="endPosition">The point at which the emitter should expire.</param>
        public MobileEmitter(float velocity, ParticleEmitter emitter, Vector3 startPosition, Vector3 endPosition)
            : base(emitter.Settings, startPosition)
        {
            this.angle = (float)Math.Atan2((startPosition.Y - endPosition.Y), (startPosition.X - endPosition.X));
            base.ZRotation = this.angle;

            float distance = (float)Math.Abs(Math.Sqrt(
                       Math.Pow(endPosition.X - startPosition.X, 2) +
                       Math.Pow(endPosition.Y - startPosition.Y, 2)));
            this.flightTime = (long)((distance / velocity)*1000);
            this.startTime = Network.Util.Clock.GetTimeMilliseconds();

            this.velocity = velocity;
            this.start = startPosition;
            this.end = endPosition;

            this.emitterRenderID = -1;

            base.Update(true);
        }
        #endregion

        #region Properties
        public int RenderID
        {
            get { return emitterRenderID; }
            set { emitterRenderID = value; }
        }
        #endregion

        #region Methods
        public override void Update(bool updateToggle)
        {
            if (this.IsExpired())
            {
                this.Emitting = false;
                if (this.emitterRenderID != -1)
                {
                   // ServiceManager.Scene.Delete(emitterRenderID);
                }
            }

            float delta = (float)ServiceManager.Game.DeltaTime;

            TranslateRelativly(Vector3.Left * (velocity * 1.05f) * delta);
            //base.Update(updateToggle);
        }

        /// <summary>
        /// Check if this mobile emitter should expire.
        /// </summary>
        /// <returns></returns>
        private bool IsExpired()
        {
             if (Network.Util.Clock.GetTimeMilliseconds() > startTime + flightTime)
             {
                 return true;
             }
             else return false;
        }
        #endregion
    }

    public class LazerBeamManager : VertexPositionColorTextureGroup
    {
        #region Members
        GamePlayState game;
        float currentTime;
        const float maxTime = 1f;
        #endregion

        public LazerBeamManager(GamePlayState _game)
            : base()
        {
            game = _game;
            currentTime = 0;
            Technique = RendererAssetPool.UniversalEffect.Techniques.LazerBeam;
        }
        public void AddLazer(Vector3 startPos, Vector3 endPos, Color color)
        {
            VertexPositionColorTexture start = new VertexPositionColorTexture();
            VertexPositionColorTexture end = new VertexPositionColorTexture();
            start.Position = startPos;
            start.Color = color;
            start.TextureCoordinate = Vector2.UnitX * currentTime;
            end.Position = endPos;
            end.Color = color;
            end.TextureCoordinate = Vector2.UnitX * currentTime;
            vertices.Add(start); //1
            vertices.Add(end);   //2
            start.Position.X += 1; end.Position.X += 1;
            vertices.Add(start); //3
            vertices.Add(end);   //4
            start.Position.X -= 2; end.Position.X -= 2;
            vertices.Add(start); //5
            vertices.Add(end);   //6
            start.Position.Y += 1; end.Position.Y += 1;
            vertices.Add(start); //7
            vertices.Add(end);   //8
            start.Position.Y -= 2; end.Position.Y -= 2;
            vertices.Add(start); //9
            vertices.Add(end);   //10
        }

        public override void Update(bool updateToggle)
        {
            currentTime += (float)ServiceManager.Game.DeltaTime;

            while (vertices.Count > 0 && vertices[0].TextureCoordinate.X < currentTime - maxTime)
            {
                vertices.RemoveRange(0, 10);
            }

            if (vertices.Count == 0)
            {
                currentTime = 0;
            }
            RendererAssetPool.UniversalEffect.WorldParameters.CurrentTime = currentTime;
        }
    }
}
