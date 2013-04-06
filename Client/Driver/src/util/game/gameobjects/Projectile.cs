/*!
    \file   Projectile.cs
    \brief  Defines a missile for VTank
    \author (C) Copyright 2008 by Vermont Technical College
*/
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
    /// <summary>
    /// Projectile defines a missile projectile that can be fired by the client
    /// </summary>
    public class Projectile : Entity, IDisposable
    {
        #region Members
        private const float gravity = -1500.0f;
        private static readonly float DEFAULT_CANNON_LENGTH = 60.0f;

        private float velocity;
        private float timeAlive;
        private float elapsed;        
        private BoundingSphere boundingSphere;
        private PlayerTank owner;
        private ProjectileData data;
        private Vector3 origin;

        public Object3 model;
        private int modelRenderID = -1;
        private int particleEmitterRenderID = -1;

        private bool usingLaunchAngle;
        private float[] tip;
        private float[] componentVelocity;
        private float elapsedDelta;
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the ID of the projectile.
        /// </summary>
        public int ID
        {
            get;
            private set;
        }
        /// <summary>
        /// Gets and sets the velocity of the missile
        /// </summary>
        public double Velocity
        {
            get { return velocity; }
            set { velocity = (float)value; }
        }

        /// <summary>
        /// Gets how long the missile can be alive for
        /// </summary>
        public float MaximumTimeAlive
        {
            get { return timeAlive; }
        }

        /// <summary>
        /// Gets and sets how long the missile has been alive
        /// </summary>
        public float Elapsed
        {
            get { return elapsed; }
            private set { elapsed = value; }
        }

        /// <summary>
        /// Returns the bounding sphere of the missile
        /// </summary>
        public new BoundingSphere BoundingSphere
        {
            get { return boundingSphere; }
        }

        /// <summary>
        /// Get the person who fired the projectile.
        /// </summary>
        public string FiredBy
        {
            get { return owner.Name; }
        }

        public PlayerTank Owner
        {
            get { return owner; }
        }

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

        public ProjectileData Data
        {
            get { return this.data; }
        }

        #endregion

        #region Constructor
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="_model">Projectile model</param>
        /// <param name="_position">Position of the missile</param>
        /// <param name="_angle">Angle at which the missile will be fired</param>
        /// <param name="_velocity">Velocity of the missile</param>
        /// <param name="_alive">Alive</param>
        /// <param name="_firedBy">Who fired the missile</param>
        public Projectile(Vector3 _position, VTankObject.Point target, double _angle, double _velocity,
            float _alive, PlayerTank _firedBy, ProjectileData projectileData)
        {
            this.data = projectileData;
            this.position = _position;          

            position = _position + Vector3.UnitZ; ;
            base.ZRotation = (float)_angle;
            velocity = (float)_velocity;
            timeAlive = _alive;
            elapsed = 0;
            this.boundingSphere.Radius = projectileData.CollisionRadius;
            SetBoundingSpherePosition();
            owner = _firedBy;
            RenderID = -1;
            ID = projectileData.ID;
            origin = position;

            if (_firedBy.Weapon.HasFiringArc)
            {
                this.usingLaunchAngle = true;

                float swivelAngle = ZRotation;
                float tiltAngle = _firedBy.Weapon.LaunchAngle;

                float projection = DEFAULT_CANNON_LENGTH * (float)Math.Cos(tiltAngle);
                float tipX = -projection * (float)Math.Cos(swivelAngle);
                float tipY = -projection * (float)Math.Sin(swivelAngle);
                float tipZ = Math.Abs(DEFAULT_CANNON_LENGTH * (float)Math.Sin(swivelAngle));
                tip = new float[] { tipX, tipY, tipZ };

                Vector3 newPosition = position + new Vector3(tipX, tipY, tipZ);
                // Calculate initial velocity based on the distance we travel.
                float distance = (float)Math.Sqrt(
                    Math.Pow(target.x - newPosition.X, 2) + 
                    Math.Pow(target.y - newPosition.Y, 2));
                float maxDistance = (int)projectileData.Range;
                if (distance > maxDistance)
                    distance = maxDistance;

                // TODO: This is a temporary work-around until we figure out what velocity component
                // is missing from the formula.
                float offset = 1.1f;
                if (tiltAngle > MathHelper.ToRadians(45.0f))
                    offset = 1.6f;

                float V  = (float)Math.Sqrt(-gravity * distance * offset);
                float Vx = -V * (float)(Math.Cos(tiltAngle) * Math.Cos(swivelAngle));
                float Vy = -V * (float)(Math.Cos(tiltAngle) * Math.Sin(swivelAngle));
                float Vz = V * (float)Math.Sin(tiltAngle);
                
                componentVelocity = new float[] { Vx, Vy, Vz };
                elapsedDelta = 0f;

                /*this.verticalVelocity = this.FindVerticalVelocity(_firedBy.Weapon.LaunchAngle, velocity);
                float flightTime = this.FindFlightTime(verticalVelocity, gravity);                
                float horizontalVelocity = this.FindHorizontalVelocity(
                    distance,
                    flightTime);
                velocity = horizontalVelocity;*/
            }

            Object3 turret = owner.Turret;
            ModelBoneCollection.Enumerator collection = turret.Model.Bones.GetEnumerator();
            List<ModelBone> emitters = new List<ModelBone>();
            while (collection.MoveNext())
            {
                if (collection.Current.Name.StartsWith("Emitter"))
                {
                    emitters.Add(collection.Current);
                }
            }

            if (emitters.Count == 0)
            {
                ServiceManager.Game.Console.DebugPrint(
                    "Warning: Can't attach to owner tank, no emitter exists.");
            }
            else
            {
                int emitter = _firedBy.Weapon.GetNextEmitterIndex();
                this.Attach(turret, emitters[emitter].Name);

                this.MimicRotation(turret);
                this.Unattach();
                Vector3 forward = emitters[emitter].Transform.Forward;
                forward.Z = Math.Abs(forward.Z);
                //position *= forward;
            }

            if (!String.IsNullOrEmpty(projectileData.Model))
            {
                model = new Object3(ServiceManager.Resources.GetModel("projectiles\\" + projectileData.Model), position);
                model.MimicPosition(this, Vector3.Zero);
                model.MimicRotation(this);
                modelRenderID = ServiceManager.Scene.Add(model, 3);
            }
            else
            {
                model = null;                
            }

            if (!String.IsNullOrEmpty(projectileData.ParticleEffect) && projectileData.Model == null)
            {
                ParticleEmitter.MimicPosition(this, Vector3.Zero);
                ParticleEmitter.MimicRotation(this);
                particleEmitterRenderID = ServiceManager.Scene.Add(ParticleEmitter, 3);
            }
            else if (!String.IsNullOrEmpty(projectileData.ParticleEffect))
            {
                ParticleEmitter = new ParticleEmitter(projectileData.ParticleEffect, this.Position);
                this.particleEmitterRenderID = ServiceManager.Scene.Add(ParticleEmitter, 3);
                ParticleEmitter.Follow(this);
            }
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Sets the bounding sphere center to the tip of the missile
        /// </summary>
        private void SetBoundingSpherePosition()
        {
            boundingSphere.Center.X = position.X + (float)Math.Cos(base.ZRotation) *
                boundingSphere.Radius;

            boundingSphere.Center.Y = position.Y + (float)Math.Sin(base.ZRotation) *
                boundingSphere.Radius;

            boundingSphere.Center.Z = position.Z;
        }

        #endregion

        #region Overridden Methods
        /// <summary>
        /// Performs logical updates on the projectile.
        /// </summary>
        /// <param name="updateToggle"></param>
        public override void Update(bool updateToggle)
        {
            if (base.HasUpdated != updateToggle)
            { 
                if (attached)
                {
                    Unattach();
                }
                
                base.Update(updateToggle);

                float delta = (float)ServiceManager.Game.DeltaTime;
                elapsed += delta;

                if (this.velocity < data.TerminalVelocity)
                {
                    velocity += data.Acceleration * delta;
                }

                if (this.usingLaunchAngle)
                {
                    float velX = componentVelocity[0];
                    float velY = componentVelocity[1];
                    float velZ = componentVelocity[2];

                    float tipX = tip[0];
                    float tipY = tip[1];
                    float tipZ = tip[2];

                    elapsedDelta += delta;

                    float X = tipX + (velX * elapsedDelta);
                    float Y = tipY + (velY * elapsedDelta);
                    float Z = tipZ + (velZ * elapsedDelta) + 0.5f * gravity * (float)Math.Pow(elapsedDelta, 2);
                    
                    position.X = origin.X + X;
                    position.Y = origin.Y + Y;
                    position.Z = origin.Z + Z;
                    //Translate(new Vector3(X, Y, Z));

                    /*this.verticalVelocity += gravity * delta;
                    float zMovement = this.verticalVelocity * delta;
                    TranslateRelativly(new Vector3(-1 * ((velocity * 1.05f) * delta), 0, zMovement));*/
                }
                else
                {
                    TranslateRelativly(Vector3.Left * (velocity * 1.05f) * delta);                   
                }

                SetBoundingSpherePosition();
            }
        }

        private float FindVerticalVelocity(float angle, float velocity)
        {
            float velocityFactor = MathHelper.ToDegrees(angle) / 90f;
            return (velocity * velocityFactor);
        }

        private float FindRoundTripTime(float verticalVelocity)
        {
            return Math.Abs(((2 * verticalVelocity) / gravity));
        }

        private float FindHorizontalVelocity(float distance, float _velocity)
        {
            return (distance/_velocity);
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

            if (model != null)
            {
                ServiceManager.Scene.Delete(modelRenderID);
            }
        }

        #endregion
    }
}
