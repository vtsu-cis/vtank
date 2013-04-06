using System;
using System.Collections.Generic;
using System.Text;
using Renderer.SceneTools.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Client.src.service;
using Renderer.SceneTools.Entities.Particles;
using Client.src.service.services;
using Client.src.states.gamestate;

namespace Client.src.util
{
    public class Base : Object3
    {
        #region Constants
        private static readonly Vector3 DEAD_FIRE_OFFSET = new Vector3(0, 0, 20);
        #endregion

        #region Enum

        /// <summary>
        /// Keeps track of whether the arrow is moving up or down.
        /// </summary>
        private enum ArrowBobState
        {
            Up,
            Down
        }

        /// <summary>
        /// Tracks the damage status of this base.
        /// </summary>
        public enum DamageState
        {
            None,
            Minor,
            Moderate,
            Critical,
            Destroyed
        }

        #endregion

        #region Members
        private Object3 arrow;
        private Object3 swordOrShield;
        private GameSession.Alliance originalOwner;
        private GameSession.Alliance baseColor;
        private int health;
        private int eventId;
        private bool destroyed;
        private bool inConflict = false;
        private int destroyedFireEffectId = -1;

        private int swordOrShieldRenderId = -1;
        
        private int arrowRenderId = -1;
        private int arrowVerticalPosition = 100;
        private Vector3 upVector = new Vector3(0, 0, 1);
        private Vector3 downVector = new Vector3(0, 0, -1);
        private ArrowBobState bobstate = ArrowBobState.Down;

        private DamageState previousDamageState; //Used for checking when to attach emitters

        // Rotation speed: 270 degrees/second.
        const float ROTATION_VELOCITY = (float)((Math.PI * 2f) - ((Math.PI * 2f) / 4f));
        #endregion

        #region Constructors
        /// <summary>
        /// Constructor for a Base
        /// </summary>
        /// <param name="color">The team color of the base</param>
        /// <param name="position">The position to place the base at</param>
        /// <param name="eventId">The event ID of the base</param>
        /// <param name="_model">The model of the base.</param>
        public Base(GameSession.Alliance color, Vector3 position, int eventId, Model _model) : base(_model, position)
        {
            this.originalOwner = color;
            this.BaseColor = color;
            this.Position = position;
            this.eventId = eventId;
            this.health = Constants.MAX_BASE_HEALTH;
            this.destroyed = false;
            
            previousDamageState = DamageState.None;

            ParticleEmitter0 = new ParticleEmitter("DamagedBaseFire");
            ParticleEmitter1 = new ParticleEmitter("DamagedBaseFire");
            ParticleEmitter2 = new ParticleEmitter("DamagedBaseFire");
            ParticleEmitter3 = new ParticleEmitter("DamagedBaseFire");

            StopEmitters();

            ServiceManager.Scene.Add(ParticleEmitter0, 3);
            ServiceManager.Scene.Add(ParticleEmitter1, 3);
            ServiceManager.Scene.Add(ParticleEmitter2, 3);
            ServiceManager.Scene.Add(ParticleEmitter3, 3);
        }
        #endregion

        #region Properties
        /// <summary>
        /// The color of a given base.  Also sets the color of its mesh when set.
        /// </summary>
        public GameSession.Alliance BaseColor
        {
            get { return baseColor; }
            set { 
                    baseColor = value;
                    if (baseColor == GameSession.Alliance.RED)
                    {
                        this.meshColor = Color.Red;
                    }
                    else if (baseColor == GameSession.Alliance.BLUE)
                    {
                        this.meshColor = Color.Blue;
                    }
                    else
                    {
                        this.meshColor = Color.White;
                    }
                }
        }

        /// <summary>
        /// The amount of health a base has.
        /// </summary>
        public int Health
        {
            get { return health; }
            set { health = value; }
        }

        /// <summary>
        /// This base's event ID, its main identifier.
        /// </summary>
        public int EventId
        {
            get { return eventId; }
        }

        /// <summary>
        /// Get whether or not the base is currently destroyed.  Also sets the base's model when its status changes.
        /// </summary>
        public bool Destroyed
        {
            get 
            {
                return destroyed;
            }
            set
            {
                destroyed = value;

                if (destroyed)
                {
                    this.handleDestruction();

                }
                else if (destroyed == false)
                {
                    this.handleReset();
                }
            }
        }

        /// <summary>
        /// This base's ID in the renderer.
        /// </summary>
        public int RenderID
        {
            get;
            set;
        }

        /// <summary>
        /// Get the damage status of this base.  Should return None, Minor, Moderate Critical, or Destroyed,
        /// based on the amount of health the base has.
        /// </summary>
        public DamageState DamageStatus
        {
            get 
            {
                if (this.Health == Constants.MAX_BASE_HEALTH)
                {
                    return DamageState.None;
                }
                else if (this.Health >= Constants.MAX_BASE_HEALTH * .70 &&
                    this.Health < Constants.MAX_BASE_HEALTH)
                {
                    return DamageState.Minor;
                }
                else if (this.Health < Constants.MAX_BASE_HEALTH * .70 && 
                    this.Health >= Constants.MAX_BASE_HEALTH * .30)
                {
                    return DamageState.Moderate;
                }
                else if (this.Health < Constants.MAX_BASE_HEALTH * .30 
                    && !this.Destroyed)
                {
                    return DamageState.Critical;
                }
                else if (this.Destroyed || this.Health == 0)
                {
                    return DamageState.Destroyed;
                }
                else //You should never get here.
                {
                    return DamageState.None;
                }

            
            }
        }

        /// <summary>
        /// Our four emitters for the fires that appear on damaged bases are below.
        /// </summary>
        public ParticleEmitter ParticleEmitter0
        {
            get;
            set;
        }
        public ParticleEmitter ParticleEmitter1
        {
            get;
            set;
        }
        public ParticleEmitter ParticleEmitter2
        {
            get;
            set;
        }
        public ParticleEmitter ParticleEmitter3
        {
            get;
            set;
        }

        /// <summary>
        /// The arrow that bobs up and down
        /// </summary>
        public Object3 Arrow
        {
            get
            {
                if (arrowRenderId != -1)

                {
                    return arrow;
                }
                else
                    return null;
            }
            private set 
            {
                arrow = value; 
            }
        }

        /// <summary>
        /// The sword or shield hovering over this base if it's in conflict.
        /// </summary>
        public Object3 SwordOrShield
        {
            get
            {
                if (swordOrShieldRenderId != -1)
                {
                    return swordOrShield;
                }
                else
                    return null;
            }
            private set { swordOrShield = value; }
        }

        /// <summary>
        /// Property that indicates whether the base is in conflict.
        /// Adds a sword or shield if true.
        /// </summary>
        public bool InConflict
        {
            get 
            {
                
                return inConflict; 
            }
            set 
            {
                
                inConflict = value;

                if (inConflict == true)
                {
                    this.AddSwordOrShield();
                }
                else if (inConflict == false)
                {
                    this.RemoveSwordOrShield();
                }
            }
        }

        #endregion

        #region Methods
        /// <summary>
        /// Handle the resetting of a destroyed base.
        /// </summary>
        private void handleReset()
        {
            this.Model = ServiceManager.Resources.GetModel("events\\base");

            if (destroyedFireEffectId != -1)
            {
                ServiceManager.Scene.Delete(destroyedFireEffectId);
                destroyedFireEffectId = -1;
            }

            if (arrowRenderId != -1)
            {
                this.RemoveArrow();
            }

            if (this.swordOrShieldRenderId == -1 && this.inConflict == true)
            {
                this.AddSwordOrShield();
            }

            this.StopEmitters();
        }

        /// <summary>
        /// Handle the events that should take place when a base is destroyed.
        /// </summary>
        private void handleDestruction()
        {
            this.StopEmitters();

            base.Model = ServiceManager.Resources.GetModel("events\\base_dead");
            ParticleEmitter explosion = new ParticleEmitter("BaseExplosion", Position);
            ServiceManager.Scene.Add(explosion, 3);

            ParticleEmitter baseFire = new ParticleEmitter("DeadBaseFire",
                Position + DEAD_FIRE_OFFSET);
            destroyedFireEffectId = ServiceManager.Scene.Add(baseFire, 3);

            this.AddArrow();

            if (this.swordOrShieldRenderId != -1 && this.inConflict == true)
            {
                this.RemoveSwordOrShield();
            }
        }

        /// <summary>
        /// Reset a base's health to full.
        /// </summary>
        public void ResetToFullHealth()
        {
            Health = Constants.MAX_BASE_HEALTH;

            if (this.Destroyed)
            {
                this.Destroyed = false;
            }

            if (this.arrowRenderId != -1)
            {
                this.RemoveArrow();
            }

            this.StopEmitters();
        }

        /// <summary>
        /// Captures a base for the given team.  Resets the base to undestroyed, and
        /// sets the base's color to its new team.
        /// </summary>
        /// <param name="newColor">The new color of the base.</param>
        public void CaptureBase(GameSession.Alliance newColor)
        {
            if (this.Destroyed)
            {
                this.Destroyed = false;
            }

            if (this.arrowRenderId != -1)
            {
                this.RemoveArrow();
            }

            this.Health = Constants.MAX_BASE_HEALTH;
            this.BaseColor = newColor;
        }

        /// <summary>
        /// Remove sword or shield if present.
        /// </summary>
        private void RemoveSwordOrShield()
        {
            if (swordOrShieldRenderId != -1)
            {
                ServiceManager.Scene.Delete(swordOrShieldRenderId);
                swordOrShieldRenderId = -1;
                SwordOrShield = null;
            }
        }

        /// <summary>
        /// Remove the bobbing arrow if present.
        /// </summary>
        private void RemoveArrow()
        {
            if (arrowRenderId != -1)
            {
                ServiceManager.Scene.Delete(arrowRenderId);
                arrowRenderId = -1;
                Arrow = null;
            }
        }

        /// <summary>
        /// Reset a base to its original owner, and reset health and destroyed status.
        /// </summary>
        public void ResetBaseToOriginalOwner()
        {
            if (this.Destroyed)
            {
                this.Destroyed = false;
            }

            this.Health = Constants.MAX_BASE_HEALTH;
            this.BaseColor = this.originalOwner;
            this.StopEmitters();
        }

        /// <summary>
        /// Set fires on the base depending on its damage state.
        /// </summary>
        public void SetEmitters()
        {
            if (this.DamageStatus == DamageState.Minor)
            {

                if (previousDamageState != this.DamageStatus)
                {                 
                    ParticleEmitter0.Attach(this, "Emitter0");
                    ParticleEmitter0.Emitting = true;

                    //In the case of an incrementing DamageState, we need to ensure the other emitters
                    // are off.
                    ParticleEmitter1.Emitting = false;
                    ParticleEmitter2.Emitting = false;
                    ParticleEmitter3.Emitting = false;
                }
            }
            else if (this.DamageStatus == DamageState.Moderate)
            {
                if (previousDamageState != this.DamageStatus)
                {                                      
                    ParticleEmitter1.Attach(this, "Emitter2");
                    ParticleEmitter1.Emitting = true;

                    ParticleEmitter2.Emitting = false;
                    ParticleEmitter3.Emitting = false;
                }
            }
            else if (this.DamageStatus == DamageState.Critical)
            {
                if (previousDamageState != this.DamageStatus)
                {                 
                    ParticleEmitter2.Attach(this, "Emitter3");
                    ParticleEmitter3.Attach(this, "Emitter1");
                    ParticleEmitter2.Emitting = true;
                    ParticleEmitter3.Emitting = true;
                }
            }

            if (previousDamageState != this.DamageStatus)
            {
                previousDamageState = this.DamageStatus;
            }
        }

        /// <summary>
        /// Stop our particle emitters to remove any fires.
        /// </summary>
        private void StopEmitters ()
        {
            ParticleEmitter0.Unattach();
            ParticleEmitter1.Unattach();
            ParticleEmitter2.Unattach();
            ParticleEmitter3.Unattach();

            ParticleEmitter0.Emitting = false;
            ParticleEmitter1.Emitting = false;
            ParticleEmitter2.Emitting = false;
            ParticleEmitter3.Emitting = false;
        }

        /// <summary>
        /// Rotate the arrow or swords/shield if present.
        /// </summary>
        public void RotateObjects()
        {
            float deltaTime = (float)ServiceManager.Game.DeltaTime;

            if (this.arrowRenderId != -1)
            {
                if (Arrow == null)
                    Arrow = ServiceManager.Scene.Access3D(arrowRenderId);
                
                Arrow.RotateZ(ROTATION_VELOCITY * deltaTime);
                
                if (arrowVerticalPosition >= 120)
                {
                    this.bobstate = ArrowBobState.Down;
                }
                else if (arrowVerticalPosition <= 80)
                {
                    this.bobstate = ArrowBobState.Up;
                }

                if (this.bobstate == ArrowBobState.Up)
                {
                    Arrow.Translate(upVector);
                    this.arrowVerticalPosition++;
                }
                else if (this.bobstate == ArrowBobState.Down)
                {
                    Arrow.Translate(downVector);
                    this.arrowVerticalPosition--;
                }
            }
            
            if ( this.swordOrShieldRenderId != -1)
            {
                if (SwordOrShield == null)
                    SwordOrShield = ServiceManager.Scene.Access3D(swordOrShieldRenderId);

                this.SwordOrShield.RotateZ(ROTATION_VELOCITY * deltaTime);
            }
        }

        /// <summary>
        /// Adds an arrow above the destroyed base.
        /// </summary>
        private void AddArrow()
        {
            Vector3 arrowPosition = this.position;
            arrowPosition.Z += arrowVerticalPosition;
            arrowRenderId = ServiceManager.Scene.Add(ServiceManager.Resources.GetModel("events\\arrow"), arrowPosition, 0);
            if (this.BaseColor == GameSession.Alliance.BLUE)
                ServiceManager.Scene.Access3D(arrowRenderId).MeshColor = Color.Red;
            else if (this.BaseColor == GameSession.Alliance.RED)
                ServiceManager.Scene.Access3D(arrowRenderId).MeshColor = Color.Blue;

            ServiceManager.Scene.Access3D(arrowRenderId).CastsShadow = false;
        }

        /// <summary>
        /// Adds a rotating sword or shield above this tower, depending on who owns it.
        /// </summary>
        private void AddSwordOrShield()
        {
            Vector3 swordOrShieldPosition = this.position;
            swordOrShieldPosition.Z += 280;

            if (this.BaseColor == ((GamePlayState)ServiceManager.StateManager.CurrentState).LocalPlayer.Team)

                swordOrShieldRenderId = ServiceManager.Scene.Add(ServiceManager.Resources.GetModel("events\\shield"), swordOrShieldPosition, 0);
            else
                swordOrShieldRenderId = ServiceManager.Scene.Add(ServiceManager.Resources.GetModel("events\\sword"), swordOrShieldPosition, 0);

            ServiceManager.Scene.Access3D(swordOrShieldRenderId).CastsShadow = false;
         }

        #endregion

    }
}
