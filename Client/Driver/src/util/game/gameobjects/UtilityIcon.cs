using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Client.src.util.game;
using Renderer.SceneTools.Entities;
using Client.src.service;
using Client.src.states.gamestate;

namespace Client.src.util
{
    public class UtilityIcon : Object3, IDisposable
    {

        #region Constructor
        public UtilityIcon(Model _model, Vector3 _position) :
            base(_model, _position)
        {
            CastsShadow = false;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the ID of this projectile inside of the renderer.
        /// </summary>
        public int RenderID
        {
            get;
            set;
        }

        public ParticleEmitter ParticleEmitter
        {
            get;
            set;
        }
        #endregion

        #region Overridden Methods

        public override void Update(bool updateToggle)
        {
            if (base.HasUpdated != updateToggle)
            {
                float delta = (float)ServiceManager.Game.DeltaTime;
                this.RotateZ(delta * 3.14f); //180 degrees per second

                base.Update(updateToggle);                
            }
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            if (ParticleEmitter != null)
            {
                ParticleEmitter.Position = base.Position2D;
                ParticleEmitter.Stop();
            }
        }

        #endregion
    }
}
