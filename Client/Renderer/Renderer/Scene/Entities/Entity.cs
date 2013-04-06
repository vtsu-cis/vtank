using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Renderer.SceneTools.Effects;
using Renderer.Utils;

namespace Renderer.SceneTools.Entities
{
    /// <summary>
    /// RendererEntity refers to any object that can be moved around in a scene, visible or not.
    /// </summary>
    public class Entity
    {
        #region Members
        /// <summary>
        /// Value representing 2 * PI converted to floating point form for use in calculations.
        /// TODO: This should perhaps be in a global constant area.
        /// </summary>
        private static readonly float TwoPI = (float)(Math.PI * 2);
        protected WorldMouse mouse;
        protected Vector3 position;
        protected Vector3 targetPosition;
        protected Vector3 attachedPosition;
        protected Entity targetEntity;
        protected Object3 attachedTo;
        protected string mountMeshName;
        protected Boolean updatable;
        protected Boolean mountable;
        protected float xRot, yRot, zRot;
        protected Matrix rotation;
        protected Matrix mountTransform;
        protected Vector3 translation;
        protected Vector3 targetOffset, followingVector;
        protected float scale;
        protected float orbitSpeed, orbitDistance;
        protected Boolean following, mimicPosition, mimicRotation, attached, orbiting, tracking, trackingCursor, trackingEntity;
        protected BoundingSphere bounds;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new RendererEntity
        /// </summary>
        public Entity()
        {
            CastsShadow = true;
            following = false;
            mimicPosition = false;
            attached = false;
            orbiting = false;
            tracking = false;
            trackingCursor = false;
            trackingEntity = false;
            updatable = true;
            TransparencyEnabled = false;
            AutoDelete = false;

            rotation = Matrix.Identity;
            bounds = new BoundingSphere(position, 10);

            HasUpdated = false;
        }
        #endregion

        #region Properties

        public bool CastsShadow
        {
            get;
            set;
        }

        /// <summary>
        /// If true, the Entity will be deleted in the Update function of its containing Scene.
        /// </summary>
        public bool AutoDelete
        {
            get;
            protected set;
        }
        /// <summary>
        /// Access to this entity's position
        /// </summary>
        public virtual Vector3 Position
        {
            get { return position; }
            set { position = value; }
        }

        /// <summary>
        /// Returns the current position of this entity in 2D screen space.
        /// </summary>
        public virtual Vector3 Position2D
        {
            get
            {
                Vector3 center = GraphicOptions.graphics.GraphicsDevice.Viewport.Project(position,
                                 GraphicOptions.CurrentCamera.Projection, GraphicOptions.CurrentCamera.View, Matrix.Identity);

                return center;

            }
        }

        /// <summary>
        /// Public get for the position this entity is aiming at.
        /// </summary>
        public virtual Vector3 TargetPosition
        {
            get { return targetPosition; }
            set { targetPosition = value; }
        }

        /// <summary>
        /// Public get/set for is this entity can become transparent.
        /// </summary>
        public bool TransparencyEnabled
        {
            get;
            set;
        }

        /// <summary>
        /// Public get for the BoundingSphere associated with this entity.
        /// </summary>
        public BoundingSphere BoundingSphere
        {
            get { return bounds; }
            protected set {bounds = value;}
        }

        /// <summary>
        /// If true, the entity will not rotate for any reason
        /// </summary>
        public Boolean LockRotation
        {
            get;
            set;
        }

        /// <summary>
        /// Gets set to true if Update has been called on this entity by another entity
        /// </summary>
        public Boolean HasUpdated
        {
            get;
            set;
        }

        /// <summary>
        /// Simple get/set for updatable
        /// </summary>
        public Boolean Updatable
        {
            get { return updatable; }
            set { updatable = value; }
        }

        /// <summary>
        /// Gets the Front vector relative to this entity
        /// </summary>
        public Vector3 Front
        {
            get { return Vector3.Normalize(rotation.Right); }
        }

        /// <summary>
        /// Gets the Right vector relative to this entity
        /// </summary>
        public Vector3 Right
        {
            get { return Vector3.Normalize(rotation.Down); }
        }

        /// <summary>
        /// Gets the Up vector relative to this entity. 
        /// </summary>
        public Vector3 Up
        {
            get { return Vector3.Normalize(rotation.Backward); }
        }

        /// <summary>
        /// Gets or sets the new X rotation value for this entity in radians.
        /// </summary>
        public float XRotation
        {
            get
            {
                return xRot;
            }

            set
            {
                xRot = (value + TwoPI) % TwoPI;
                rotation = Matrix.CreateFromYawPitchRoll(yRot, xRot, zRot);
            }
        }

        /// <summary>
        /// Gets or sets the new Y rotation value for this entity in radians.
        /// </summary>
        public float YRotation
        {
            get
            {
                return yRot;
            }

            set
            {
                yRot = (value + TwoPI) % TwoPI;
                rotation = Matrix.CreateFromYawPitchRoll(yRot, xRot, zRot);
            }
        }

        /// <summary>
        /// Gets or sets the new Z rotation value for this entity in radians.
        /// </summary>
        public float ZRotation
        {
            get
            {
                return zRot;
            }

            set
            {
                zRot = (value + TwoPI) % TwoPI;
                rotation = Matrix.CreateFromYawPitchRoll(yRot, xRot, zRot);
            }
        }
        #endregion

        #region Update/Draw

        /// <summary>
        /// Rotates, scales, then translates the entity. 
        /// </summary>
        public virtual void Update(Boolean updateToggle)
        {
            if (HasUpdated != updateToggle && updatable)
            {
                
                rotation = Matrix.CreateFromYawPitchRoll(yRot, xRot, zRot);
                if (following)
                {
                    if (targetEntity.HasUpdated != updateToggle)
                      {
                          targetEntity.Update(updateToggle);
                      }
                    Vector3 wantedPosition = targetEntity.Position + followingVector;
                    translation = wantedPosition - position;
                    targetPosition = targetEntity.Position;
                }

                if (mimicPosition)
                {
                    if (targetEntity.HasUpdated != updateToggle)
                    {
                        targetEntity.Update(updateToggle);
                    }
                    position = targetEntity.Position + targetOffset;
                }
                if (mimicRotation)
                {
                    if (targetEntity.HasUpdated != updateToggle)
                    {
                        targetEntity.Update(updateToggle);
                    }
                    rotation = targetEntity.rotation;
                }
                if (attached)
                {
                    if (attachedTo.HasUpdated != updateToggle)
                    {
                        attachedTo.Update(updateToggle);
                    }
                    position = attachedTo.MountPosition(mountMeshName);
                }

                if (tracking)
                {
                    if (trackingEntity)
                    {
                        if (targetEntity.HasUpdated != updateToggle)
                        {
                            targetEntity.Update(updateToggle);
                        }
                        targetPosition = targetEntity.Position;
                    }
                    else if (trackingCursor)
                    {
                        Plane p = new Plane(Vector3.UnitZ, -Position.Z);
                        Ray r = WorldMouse.WorldRay;
                        float denominator = Vector3.Dot(p.Normal, r.Direction);
                        float numerator = Vector3.Dot(p.Normal, r.Position) + p.D;
                        float t = -(numerator / denominator);
                        targetPosition = r.Position + r.Direction*t;
                        if (t < 0)
                        {
                            targetPosition = position + (position - targetPosition);
                        }
                    }

                    Aim(targetPosition, 1);
                }

                if (orbiting)
                {
                    UpdateOrbit();
                }
                Commit();
                HasUpdated = updateToggle;
            }

        }

        /// <summary>
        /// Saves the new values of the position and rotation
        /// </summary>
        protected virtual void Commit()
        {
            position = position + translation;
            bounds.Center = position;
            translation = Vector3.Zero;
            if (LockRotation)
            {
                rotation = Matrix.Identity;
            }

        }

        /// <summary>
        /// Draws the model (TODO)
        /// </summary>
        public virtual void Draw(EffectTechnique technique)
        {
        }
        #endregion


        #region Animations
        protected virtual void UpdateAnimations()
        {
        }
        #endregion

        #region Movement

        #region Translation
        /// <summary>
        /// Moves the RendererEntity across the scene. A 2D object can only be moved in one plane.
        /// </summary>
        /// <param name="direction">The motion relative to the entities current position</param>
        public void Translate(Vector3 _translation)
        {
            translation = translation + _translation;
        }

        /// <summary>
        /// Moves the RendererEntity relative to its rotation
        /// </summary>
        /// <param name="_translation"></param>
        public void TranslateRelativly(Vector3 _translation)
        {
            translation = translation + 
                (_translation.X * rotation.Right) + 
                (_translation.Y * rotation.Up) + 
                (_translation.Z * rotation.Backward);
        }
        #endregion

        #region Rotation
        /// <summary>
        /// Rotate entire entity around the global X-axis.
        /// </summary>
        /// <param name="radians">The rotation in radians.</param>
        public void RotateX(float radians)
        {
            xRot = (xRot + radians) % TwoPI;
        }

        /// <summary>
        /// Rotate entire entity around the global Y-axis.
        /// </summary>
        /// <param name="radians">The rotation in radians.</param>
        public void RotateY(float radians)
        {
            yRot = (yRot + radians) % TwoPI;
        }

        /// <summary>
        /// Rotate entire entity around global Z-axis. 
        /// </summary>
        /// <param name="radians">The rotation in radians</param>
        public void RotateZ(float radians)
        {
            zRot = (zRot + radians) % TwoPI;
        }

        /// <summary>
        /// Rotates the entity around all three axis.
        /// </summary>
        /// <param name="yaw">The roatation around the X axis in radians.</param>
        /// <param name="pitch">The rotation around the Y axis in radians.</param>
        /// <param name="roll">The rotation around the Z axis in radians.</param>
        public void Rotate(float _xRot, float _yRot, float _zRot)
        {
            XRotation = _xRot;
            YRotation = _yRot;
            ZRotation = _zRot;
        }

        #endregion
        
        #region Special
        /// <summary>
        /// This Entity will follow another from the same general position.
        /// </summary>
        /// <param name="entity">The entity to follow.</param>
        public virtual void Follow(Entity entity)
        {
            following = true;
            mimicPosition = false;
            targetEntity = entity;
            followingVector = position - targetEntity.Position;
        }

        /// <summary>
        /// This entity will follow another from the same place relative to its rotation
        /// </summary>
        /// <param name="entity">The entity to follow.</param>
        public virtual void MimicPosition(Entity entity, Vector3 _targetoffset)
        {
            following = false;
            mimicPosition = true;
            targetOffset = _targetoffset;
            targetEntity = entity;
        }

        /// <summary>
        /// This entity will adopt the rotation value of another given entity.
        /// </summary>
        /// <param name="entity">The entity whose rotation will be mimiced.</param>
        public virtual void MimicRotation(Entity entity)
        {
            following = false;
            mimicRotation = true;
            targetEntity = entity;
        }
        /// <summary>
        /// Attaches the Root transform of this entity to another. An entity can only attach to one other.
        /// TODO: Since Renderer2DObjects are also entities, how is it possible for Renderer2DObjects to use this?
        /// </summary>
        /// <param name="entity">The entity to attach to</param>
        public virtual void Attach(Object3 entity, string mesh)
        {
            position = entity.MountPosition(mesh);
            attachedTo = entity;
            mountMeshName = mesh;
            following = false;
            attached = true;
            orbiting = false;
        }

        /// <summary>
        /// Unattach a model from it's attachee.
        /// </summary>
        public virtual void Unattach()
        {
            if (attached)
            {
                attachedTo = null;
                attached = false;
            }
            if (mimicPosition || mimicRotation)
            {
                mimicRotation = false;
                mimicPosition = false;
                targetEntity = null;
            }
        }

        /// <summary>
        /// Unattaches the entity
        /// </summary>
        public virtual void Split()
        {
            attached = false;
        }

        /// <summary>
        /// Aims entity at another
        /// </summary>
        /// <param name="entity">The entity to aim at</param>
        public void TrackTo(Entity _entity)
        {
            trackingEntity = true;
            trackingCursor = false;
            targetEntity = _entity;
            TrackTo(_entity.position);
        }

        /// <summary>
        /// Track an entity to a specific point.
        /// </summary>
        /// <param name="_target">The Vector3 point to track to.</param>
        public void TrackTo(Vector3 _target)
        {
            tracking = true;
            targetPosition = _target;
        }

      
        /// <summary>
        /// Aims entity at cursor location
        /// </summary>
        public void TrackToCursor()
        {
            trackingCursor = true;
            trackingEntity = false;
            tracking = true;
        }

        /// <summary>
        /// Stops an entity from tracking another object.
        /// </summary>
        public void StopTracking()
        {
            tracking = false;
        }


        /// <summary>
        /// Causes entity to orbit around another
        /// </summary>
        /// <param name="entity">The entity to orbit</param>
        /// <param name="distance">The distance to keep</param>
        /// <param name="speed">The speed of orbit</param>
        public void Orbit(Entity entity, float distance, float speed)
        {
            orbiting = true;
            TrackTo(entity);
            orbitSpeed = speed;
            orbitDistance = distance;
        }

        /// <summary>
        /// Updates the current orbit TODO3 fix this
        /// </summary>
        private void UpdateOrbit()
        {
            Aim(targetPosition, 1);
            TranslateRelativly(Vector3.Right * orbitSpeed);
            float difference = Vector3.Distance(position, targetEntity.position) - orbitDistance;
            TranslateRelativly(Vector3.Forward * difference);
        }

        /// <summary>
        /// Stops this entity from orbiting.
        /// </summary>
        public void StopOrbit()
        {
            orbiting = false;
        }

        /// <summary>
        /// Computes the rotation matrix for tracking and targeting functions
        /// </summary>
        /// <param name="_target">The Vector3 to track to</param>
        public void Aim(Vector3 _target, float speed)
        {

            if (position == _target)
            {
                _target = _target + Vector3.Up;
            }
            _target = _target - position;
            rotation.Right = Vector3.Lerp(rotation.Right, _target, speed);//_target - position;
            rotation.Right = Vector3.Normalize(rotation.Right);
            rotation.Backward = Vector3.Cross(rotation.Down, Vector3.Left);
            rotation.Backward = Vector3.Normalize(rotation.Backward);
            rotation.Backward = Vector3.UnitZ;
            rotation.Down = Vector3.Cross(rotation.Forward, rotation.Right);
            rotation.Down = Vector3.Normalize(rotation.Down);

        }
        #endregion
        #endregion

    }
}