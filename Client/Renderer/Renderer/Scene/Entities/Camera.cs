/*!
    \file   Camera.cs
    \brief  Game camera used to define the 3D perspective
    \author (C)Copyright 2009 by Vermont Technical College
*/
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Storage;
using Renderer;
using Renderer.Utils;

namespace Renderer.SceneTools.Entities
{
    /// <summary>
    /// The camera is an entity that can move around the scene like any other. This class contains all 
    /// of the Camera specific methods that enable viewing the scene.
    /// </summary>
    public class Camera : Entity
    {
        #region Members
        private Matrix projection;
        private Vector3 up;
        private Entity lockedTo;
        Vector3 relativeVector, relativeTarget;
        bool locked;
        private float zLockLerpValue = 0.2f;
        #endregion
        
        #region Constructors
        /// <summary>
        /// Creates a camera with a default view
        /// </summary>
        public Camera()
        {
            locked = false;
            up = Vector3.Up;
            position       =  Vector3.Backward*800;
            targetPosition     =  Vector3.Zero;
            float aspectRatio = (float)GraphicOptions.graphics.GraphicsDevice.Viewport.Width /
                (float)GraphicOptions.graphics.GraphicsDevice.Viewport.Height;
            projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, aspectRatio, 10f, 10000.0f);
            ZLockLerpValue = 0.2f;
            SetView();
        }

        /// <summary>
        /// Creates a custom camera
        /// </summary>
        /// <param name="_pos">The position the camera will be created at.</param>
        /// <param name="_target">The direction the camera will be pointing</param>
        /// <param name="_projection">The projection matrix used by the camera</param>
        public Camera(Vector3 _pos, Vector3 _target, Matrix _projection)
        {
            locked = false;
            up = Vector3.Up;
            position = _pos;
            targetPosition = _target;
            projection = _projection;
            SetView();
        }
        #endregion


        #region Properties
        /// <summary>
        /// Gets and sets the up direction of the camera;
        /// </summary>
        public Vector3 CameraUp
        {
            get { return up; }
            set { up = value; SetView(); }
        }
        /// <summary>
        /// Gets the view matrix for the camera
        /// </summary>
        public Matrix View
        {
            get { return rotation;}
            set { rotation = value; }
        }

        /// <summary>
        /// Gets the projection matrix of the camera
        /// </summary>
        public Matrix Projection
        {
            get { return projection; }
            set { projection = value; SetView(); }
        }

        /// <summary>
        /// Overridden get/set for the camera's position. The difference is that setting the position of a camera will update the camera's current view.
        /// </summary>
        public override Vector3 Position
        {
            get { return position; }
            set { position = value; SetView(); }
        }

        /// <summary>
        /// Public get/set for the camera's target position. 
        /// </summary>
        override public Vector3 TargetPosition
        {
            get { return targetPosition; }
            set { targetPosition = value; SetView(); }
        }

        /// <summary>
        /// Public get/set for the value to use as the linear interpolation value between the current camera position and the locked camera postion.
        /// </summary>
        public float ZLockLerpValue
        {
            get { return zLockLerpValue; }
            set { zLockLerpValue = value; }
        }
        #endregion
        
        #region Methods
        /// <summary>
        /// Keeps the camera at and aiming at the same position relatice to a given entity.
        /// </summary>
        /// <param name="_entity">The entity the camera moves relativly to. </param>
        /// <param name="_relativeVector">The position vector that is added to _entity's position to get the position of the camera.</param>
        /// <param name="_relativeTarget">The position vector that is added to _entity's position to get the target of the camera.</param>
        public void LockTo(Entity _entity, Vector3 _relativeVector, Vector3 _relativeTarget)
        {
            lockedTo = _entity;
            locked = true;
            relativeVector = _relativeVector;
            relativeTarget = _relativeTarget;
        }

        /// <summary>
        /// Will move the camera immediatly to its locked position. 
        /// </summary>
        public void ForceToLockedPosition()
        {
            if(locked)
            {
                Position = lockedTo.Position + (lockedTo.Front * relativeVector.X)
                                             + (lockedTo.Right * relativeVector.Y)
                                             + (lockedTo.Up * relativeVector.Z);
                TargetPosition = lockedTo.Position + (lockedTo.Front * relativeTarget.X)
                                                   + (lockedTo.Right * relativeTarget.Y)
                                                   + (lockedTo.Up * relativeTarget.Z);
            }
        }

        /// <summary>
        /// If the camera is following, Zoom will move the camera forward and backward along the following vector.
        /// </summary>
        /// <param name="zoom"></param>
        public void Zoom(float zoom)
        {
            Vector3 dir = followingVector;
            dir.Normalize();
            followingVector += dir * zoom;
            if (followingVector.Length() <= 500f) followingVector = dir * 500;
            if (followingVector.Length() >= 3000f) followingVector = dir *3000;
        }
        /// <summary>
        /// Detached the camera from an eneity that is is tracking because of LockTo(...)
        /// </summary>
        public void Unlock()
        {
            locked = false;
        }

        /// <summary>
        /// Updates the position and view of the camera.
        /// </summary>
        /// <param name="updateToggle"></param>
        public override void Update(Boolean updateToggle)
        {
            if (HasUpdated != updateToggle && Updatable)
            {
                base.Update(updateToggle);
                if (locked)
                {
                    if (lockedTo.HasUpdated != updateToggle) { lockedTo.Update(updateToggle); }
                    Vector3 wantedPosition = lockedTo.Position + (lockedTo.Front * relativeVector.X)
                                                               + (lockedTo.Right * relativeVector.Y)
                                                               + (lockedTo.Up * relativeVector.Z);
                    Vector3 newPositionDirection = Vector3.Lerp(Position, wantedPosition, ZLockLerpValue);
                    Vector3 actualFollowingVector = lockedTo.Position - newPositionDirection;
                    Vector3 rightVector = Vector3.Cross(actualFollowingVector, Vector3.UnitZ);

                    rightVector.Normalize();
                    actualFollowingVector.Z = 0.0f;
                    actualFollowingVector.Normalize();
                    Vector3 newPosition = lockedTo.Position + (actualFollowingVector * relativeVector.X)
                                                 + (rightVector * relativeVector.Y)
                                                 + (Vector3.UnitZ * relativeVector.Z);
                    if (Math.Abs((newPosition - Position).Length()) > 0.1)
                    {
                        Position = newPosition;
                    }
                    else
                    {
                        Position = wantedPosition;
                    }

                    TargetPosition = lockedTo.Position + (lockedTo.Front * relativeTarget.X)
                                                 + (lockedTo.Right * relativeTarget.Y)
                                                 + (lockedTo.Up * relativeTarget.Z);
                    
                }
                SetView();
            }
        }

        /// <summary>
        /// Creates a new view from all changes made to the camera.
        /// </summary>
        public void SetView()
        {
            View = Matrix.CreateLookAt(position, targetPosition, up);
        }
        #endregion
    }
}
