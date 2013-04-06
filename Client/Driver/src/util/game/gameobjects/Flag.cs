using System;
using System.Collections.Generic;
using System.Text;
using Renderer.SceneTools.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Client.src.service;

namespace Client.src.util
{
    public class Flag : Object3, IDisposable
    {
        #region Members
        private float VrotationAngle;
        private float TrotationAngle;
        private ModelBone VBone;
        private ModelBone TBone;
        private Matrix VT, TT;
        private bool emitting;
        #endregion

        #region Constructor
        public Flag(Model _model, Vector3 _position) :
            base(_model, _position)
        {
            SpawnPosition = _position;
            Hidden = true;
            VrotationAngle = (float)Math.PI;
            TrotationAngle = 0f;
            VBone = _model.Bones["FlagV"];
            TBone = _model.Bones["FlagT"];
            VT = VBone.Transform;
            TT = TBone.Transform;
            emitting = true;
        }
        #endregion

        #region Properties

        /// <summary>
        /// If true, this flag will not draw
        /// </summary>
        public bool Hidden
        {
            get;
            set;
        }
        /// <summary>
        /// Gets or sets the ID of this flag inside of the renderer.
        /// </summary>
        public int RenderID
        {
            get;
            set;
        }
        /// <summary>
        /// Gets or sets the ID of the base associated with this flag inside of the renderer.
        /// </summary>
        public int BaseRenderID
        {
            get;
            set;
        }

        public ParticleEmitter ParticleEmitter1
        {
            get;
            set;
        }
        public ParticleEmitter ParticleEmitter0
        {
            get;
            set;
        }

        public Vector3 SpawnPosition
        {
            get;
            set;
        }


        #endregion

        public override void Draw(EffectTechnique technique)
        {
            if (!Hidden)
            {
                VBone.Transform = Matrix.CreateRotationZ(VrotationAngle) * VT;
                TBone.Transform = Matrix.CreateRotationZ(TrotationAngle) * TT;
                Model.CopyAbsoluteBoneTransformsTo(boneTransforms);
                base.Draw(technique);
            }      
        }

        public override void Update(bool updateToggle)
        {
            VrotationAngle += 5 * (float)ServiceManager.Game.DeltaTime % MathHelper.ToRadians(360);
            TrotationAngle = -TrotationAngle;
            TrotationAngle += 5 * (float)ServiceManager.Game.DeltaTime % MathHelper.ToRadians(360);
            TrotationAngle = -TrotationAngle;

            bool isRunningSlowly = ServiceManager.Game.IsRunningSlowly;
            if (emitting && isRunningSlowly)
            {
                emitting = false;
                ParticleEmitter0.Emitting = false;
                ParticleEmitter1.Emitting = false;
            }
            else if (!emitting && !isRunningSlowly)
            {
                emitting = true;
                ParticleEmitter0.Emitting = true;
                ParticleEmitter1.Emitting = true;
            }

            base.Update(updateToggle);
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (ParticleEmitter0 != null)
            {
                ParticleEmitter0.Position = base.Position2D;
                ParticleEmitter0.Stop();
            }
            if (ParticleEmitter1 != null)
            {
                ParticleEmitter1.Position = base.Position2D;
                ParticleEmitter1.Stop();
            }

            emitting = false;
        }

        #endregion
    }
}
