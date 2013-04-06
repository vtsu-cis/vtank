using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Renderer.SceneTools.Entities;
using Renderer.SceneTools.Entities.Particles;
using Microsoft.Xna.Framework.Graphics;
using Client.src.service;
using Client.src.util.game;
using VTankObject;
using Network.Util;

namespace Client.src.util
{
    #region Hover Tank Helper Class
    /// <summary>
    /// Helper class that provides the vector to translate hover tanks with to produce the
    /// bobbing effect.
    /// </summary>
    public class HoverTranslation
    {
        #region Members
        private float maxHoverRadius;
        private float currentHoverPosition = 0f;

        // These two variables define the upper and lower bounds of the
        // translation.
        private float upperThreshold;
        private float lowerThreshold;

        private float accelerationAmount;
        private float accelerationIncrement;
        private float velocity = 0f;
        private float deltaTime;

        //Max speed of 5.0 pixels/second
        private float maxVelocity = .5f;

        #endregion

        #region Constructors
        /// <summary>
        /// Default constructor that provides reasonable (albeit arbitrary) values.
        /// </summary>
        public HoverTranslation() : this(3, .05f, .5f) {}

        /// <summary>
        /// Full constructor for a HoverTranslation
        /// </summary>
        /// <param name="maxHoverRadius">The amount the tank should hover upward/downward from its initial position</param>
        /// <param name="maxVelocity">The maximum speed at which to move up or down.</param>
        /// <param name="accelerationIncrement">The ammount to accelerate the velocity by.</param>
        public HoverTranslation(float maxHoverRadius, float maxVelocity, float accelerationIncrement)
        {
            this.maxHoverRadius = maxHoverRadius;
            this.maxVelocity = maxVelocity;
            this.accelerationIncrement = accelerationIncrement;

            this.upperThreshold = maxHoverRadius;
            this.lowerThreshold = -maxHoverRadius;
        }

        #endregion

        #region Properties
        /// <summary>
        /// Get the vector used to translate with.
        /// </summary>
        public Vector3 AsVector3
        {
            get;
            private set;
        }

        /// <summary>
        /// Get the amount the tank is currently hovering off of the original offset.
        /// </summary>
        public float CurrentHoverAmount
        {
            get { return this.currentHoverPosition; }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Update the working values of this HoverTranslation
        /// </summary>
        public void Update()
        {
            this.deltaTime = (float)ServiceManager.Game.DeltaTime;

            this.UpdateAcceleration();
            this.UpdateVelocity();
            this.currentHoverPosition += velocity;

            this.currentHoverPosition = (currentHoverPosition >= upperThreshold) ? upperThreshold : 
                (currentHoverPosition <= lowerThreshold) ? lowerThreshold : currentHoverPosition;

            this.AsVector3 = new Vector3(0, 0, velocity);
        }

        /// <summary>
        /// Update the velocity
        /// </summary>
        private void UpdateVelocity()
        {
            if (this.currentHoverPosition >= this.upperThreshold || 
                this.currentHoverPosition <= this.lowerThreshold)
            {
                velocity = 0f;
            }

            if (Math.Abs(velocity) <= maxVelocity)
            {
                velocity += accelerationAmount*deltaTime;
            }
            
            if (Math.Abs(velocity) > maxVelocity)
            {
                velocity = maxVelocity * Math.Sign(velocity);
            }
        }

        /// <summary>
        /// Application of cumulative acceleration?
        /// </summary>
        private void UpdateAcceleration()
        {
            if (this.currentHoverPosition >= this.upperThreshold)
            {
                accelerationAmount = -accelerationIncrement;
            }
            else if (this.currentHoverPosition <= this.lowerThreshold)
            {
                accelerationAmount = accelerationIncrement;
            }
            else
            {
                accelerationAmount += (Math.Sign(accelerationAmount) == 1) ? accelerationIncrement : -accelerationIncrement;
            }
        }

        #endregion
    }
    #endregion

    /// <summary>
    /// Represent a player in game.  This class is used by both the game logic for the 
    /// accounting an maintenance of a given player, as well as the renderer for drawing
    /// the tank.
    /// </summary>
    public class PlayerTank : Object3
    {
        #region Members
        public static readonly int MaxHealth = Constants.MAX_PLAYER_HEALTH;
        private VTankObject.TankAttributes attributes;
        private Client.src.util.game.Weapon weapon;
        private List<VTankObject.Utility> utilities;
        private List<ActiveUtility> activeUtilities;
        private List<int> assistList;
        private bool alive = true;
        private Object3 turret;
        private VTankObject.Direction directionMovement;
        private VTankObject.Direction directionRotation;
        private double elapsed = 0;
        private BoundingSphere frontSphere;
        private BoundingSphere backSphere;
        private bool isCharging;
        private HoverTranslation hoverValue;
        private List<ParticleEmitter> emitters;
        #endregion

        #region Properties
        /// <summary>
        /// Access this tank's turret object.
        /// </summary>
        public Object3 Turret
        {
            get 
            { 
                return turret; 
            }

            set
            {
                turret = value;
                ApplyTurretSkin();
            }
        }

        /// <summary>
        /// Find out whether a tank is currently charging his weapon.
        /// </summary>
        public bool IsCharging
        {
            get { return isCharging; }
            set 
            {
                isCharging = value;
                if (isCharging)
                {
                    TimeChargingBegan = Clock.GetTimeMilliseconds();
                }
            }
        }      

        /// <summary>
        /// Get the time a tank started charging his weapon.
        /// </summary>
        public long TimeChargingBegan
        {
            get;
            private set;
        }

        public List<VTankObject.Utility> Utilities
        {
            get { return utilities; }
        }

        /// <summary>
        /// Get the amount of time since this tank fired.
        /// </summary>
        public double TimeSinceFiring
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the time remaining before the tank can fire
        /// </summary>
        public float Remaining 
        {
           get 
           { 
               float i = (float)(weapon.Cooldown - elapsed);
               return (i < 0) ? 0 : i;
           }
        }

        /// <summary>
        /// Gets or sets the health bar that this tank uses.
        /// </summary>
        public HealthBar HealthBar
        {
           get;
           private set;
        }
        /// <summary>
        /// Gets or sets the text object that displays the tanks name.
        /// </summary>
        public TextObject NameObject
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the ID of the tank. The ID in this case is referring to how the server knows
        /// the tank.
        /// </summary>
        public int ID
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets the ID of the tank in the renderer.
        /// </summary>
        public int RenderID
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the ID of the turret render ID.
        /// </summary>
        public int TurretRenderID
        {
            get;
            set;
        }

        public int FlagRenderID
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the ID of the health bar object.
        /// </summary>
        public int HealthBarRenderID
        {
            get;
            set;
        }
        /// <summary>
        /// Gets or sets the ID of the name object.
        /// </summary>
        public int NameObjectRenderID
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the name of the tank, which is handed to us by the server.
        /// </summary>
        public String Name 
        { 
            get 
            { 
                return attributes.name; 
            }
        }

        /// <summary>
        /// Gets the front sphere of the tank. Used for collision detection.
        /// </summary>
        public BoundingSphere FrontSphere
        {
            get
            {
                return frontSphere;
            }

            private set
            {
                frontSphere = value;
            }
        }

        /// <summary>
        /// Gets the rear sphere of the tank. Used for collision detection.
        /// </summary>
        public BoundingSphere BackSphere
        {
            get
            {
                return backSphere;
            }

            private set
            {
                backSphere = value;
            }
        }

        /// <summary>
        /// Gets or sets the angle of the tank. When the tank's angle is changed via this
        /// property, it performs a RotateZ on the object.
        /// </summary>
        public float Angle 
        { 
            get 
            {
                return base.ZRotation;
            }
            
            set 
            {
                base.ZRotation = value;
            } 
        }

        /// <summary>
        /// Gets or sets the angle of the tank's turret.
        /// </summary>
        public float TurretAngle
        {
            get
            {
                return Turret.ZRotation;
            }

            set
            {
                Turret.ZRotation = value;
            }
        }

        /// <summary>
        /// Gets or sets the position of the tank.
        /// </summary>
        public new Vector3 Position
        {
            get
            {
                 return base.Position;
            }

            set
            {
                 base.Position = value;
            }
        }

        /// <summary>
        /// Gets or sets whether this tank is moving forward, in reverse, or not at all.
        /// </summary>
        public VTankObject.Direction DirectionMovement
        {
            get
            {
                return directionMovement;
            }

            set
            {
                if (value == VTankObject.Direction.LEFT || 
                    value == VTankObject.Direction.RIGHT)
                {
                    throw new Exception(String.Format(
                        "DirectionMovement must only take {0}, {1}, or {2}.",
                        "Direction.FORWARD", "Direction.REVERSE", "Direction.NONE"));
                }

                directionMovement = value;
            }
        }

        /// <summary>
        /// For synchronization purposes, it records the last known direction movement.
        /// </summary>
        public VTankObject.Direction LastDirectionMovement
        {
            get;
            set;
        }

        /// <summary>
        /// For synchronization and predictive movement purposes, the tank tracks whether or
        /// not it recently had a collision.
        /// </summary>
        public bool PreviouslyCollidied
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets whether this tank is rotating left, right, or not at all.
        /// </summary>
        public VTankObject.Direction DirectionRotation
        {
            get
            {
                return directionRotation;
            }

            set
            {
                if (value == VTankObject.Direction.FORWARD ||
                    value == VTankObject.Direction.REVERSE)
                {
                    throw new Exception(String.Format(
                        "DirectionRotation must only take {0}, {1}, or {2}.",
                        "Direction.LEFT", "Direction.RIGHT", "Direction.NONE"));
                }

                LastDirectionMovement = directionMovement;
                directionRotation = value;
            }
        }

        /// <summary>
        /// Gets the attributes associated to this tank.
        /// </summary>
        public VTankObject.TankAttributes Attributes 
        { 
            get 
            { 
                return attributes; 
            } 
        }

        /// <summary>
        /// Gets the health of the tank. To modify this property, call InflictDamage or
        /// RestoreHealth.
        /// </summary>
        public int Health 
        { 
            get 
            { 
                return attributes.health; 
            } 
            
            private set 
            { 
                attributes.health = value;
                if (HealthBar != null)
                {
                    HealthBar.Health = value;
                }
            } 
        }

        /// <summary>
        /// Gets or sets whether or not this tank is alive.
        /// </summary>
        public bool Alive
        {
            get
            {
                return alive;
            }

            set
            {
                alive = value;
                if (alive)
                {
                    base.Model = ServiceManager.Resources.GetModel(
                        "tanks\\" + attributes.model);
                    Turret.Model = ServiceManager.Resources.GetModel(
                        "weapons\\" + weapon.Model);
                    attributes.skin = attributes.skin.Replace("_dents", "");
                    ApplyTankSkin();
                    ApplyTurretSkin();

                    if (PlayerManager.LocalPlayerName == Name)
                    {
                        Turret.TrackToCursor();
                    }

                    foreach (ParticleEmitter emitter in emitters)
                    {
                        emitter.Emitting = true;
                    }

                    if (attributes.model == "Icarus")
                      this.Translate(new Vector3(0, 0, 20 + hoverValue.CurrentHoverAmount));
                    
                }
                else
                {
                    base.Model = ServiceManager.Resources.GetModel(
                        "tanks\\" + attributes.model + "_dead");
                    Turret.Model = ServiceManager.Resources.GetModel(
                        "weapons\\" + weapon.DeadModel);
                    attributes.skin = attributes.skin + "_dents";
                    ApplyTankSkin();
                    ApplyTurretSkin();

                    if (PlayerManager.LocalPlayerName == Name)
                    {
                        Turret.StopTracking();
                    }

                    RemoveAllUtilities();

                    if (attributes.model == "Icarus")
                        this.Translate(new Vector3(0, 0, -20 - hoverValue.CurrentHoverAmount));

                    ParticleEmitter explosion = new ParticleEmitter("TankExplosion", Position);
                    ServiceManager.Scene.Add(explosion, 3);
                    explosion.Position = Position;

                    ParticleEmitter tankFire = new ParticleEmitter("TankFire",
                        Position + new Vector3(0, 0, 9));
                    ServiceManager.Scene.Add(tankFire, 3);


                    foreach (ParticleEmitter emitter in emitters)
                    {
                        emitter.Emitting = false;
                    }
                }
            }
        }

        public Client.src.util.game.Weapon Weapon
        {
            get { return this.weapon; }
        }

        /// <summary>
        /// Gets the team that this player bats for.
        /// </summary>
        public GameSession.Alliance Team
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets the active weapon sound this player has.
        /// </summary>
        public Audio.SoundCue ActiveSound
        {
            get;
            set;
        }

        #endregion Properties

        #region Constructor
        /// <summary>
        /// Constructs a new player tank, which also initializes it's inner Object3 class.
        /// </summary>
        /// <param name="_tankModel">Model to load into the tank.</param>
        /// <param name="_attr"></param>
        /// <param name="_pos"></param>
        /// <param name="_alive"></param>
        /// <param name="_angle"></param>
        /// <param name="_id"></param>
        /// <param name="_team"></param>
        public PlayerTank(Model _tankModel,
                VTankObject.TankAttributes _attr,
                Vector3 _pos,
                bool _alive,
                float _angle,
                int _id,
                GameSession.Alliance _team)
            : base(_tankModel, _pos)
        {
            hoverValue = new HoverTranslation();
            activeUtilities = new List<ActiveUtility>();
            utilities = new List<Utility>();
            assistList = new List<int>();
            emitters = new List<ParticleEmitter>();
            attributes = _attr;
            position = _pos;

            if (_attr.model == "Icarus")
            {
                this.Translate(new Vector3(0, 0, 20));
                this.AddEmitters();
            }                

            alive = _alive;
            Angle = _angle;
            ID = _id;
            RenderID = -1;
            TurretRenderID = -1;
            HealthBarRenderID = -1;
            Team = _team;
            weapon = WeaponLoader.GetWeapon(attributes.weaponID);
            elapsed = weapon.Cooldown;
            LastDirectionMovement = VTankObject.Direction.NONE;
            PreviouslyCollidied = false;
            IsCharging = false;
            /*AlternateModelTexture = ServiceManager.Resources.Load<Texture2D>(
                @"models\tanks\skins\camo-forest");
             */
            
            if (_attr.name != PlayerManager.LocalPlayerName)
            {
                HealthBar = new HealthBar(position, attributes.health);
                NameObject = new TextObject();
                NameObject.SetText(attributes.name);
            }

            if (Team == GameSession.Alliance.BLUE)
            {
                meshColor = Color.Blue;

            }
            else if (Team == GameSession.Alliance.RED)
            {
                meshColor = Color.Red;
            }
            else
            {
                meshColor = Toolkit.GetColor(attributes.color);
            }

            SetUpSphereBoundaries();
            ApplyTankSkin();
        }
        #endregion

        #region Private Methods

        /// <summary>
        /// Add the emitters if using a hover tank.
        /// </summary>
        private void AddEmitters()
        {
            /*
            ModelBoneCollection.Enumerator collection = this.Model.Bones.GetEnumerator();

            while (collection.MoveNext())
            {
                if (collection.Current.Name.StartsWith("Emitter"))
                {
                    ParticleEmitter _emitter = new ParticleEmitter("Hover");
                    _emitter.Position = this.position;
                    _emitter.Attach(this, collection.Current.Name);
                    emitters.Add(_emitter);
                    ServiceManager.Scene.Add(_emitter,3);
                }
            }
             */
        }

        /// <summary>
        /// For the purposes of collision detection, set up bounding spheres.
        /// </summary>
        private void SetUpSphereBoundaries()
        {
            FrontSphere = new BoundingSphere();
            BackSphere = new BoundingSphere();

            frontSphere.Center = new Vector3((float)(position.X + Math.Cos(Angle) * 15),
                    (float)(position.Y + Math.Sin(Angle) * 15), 0);
            frontSphere.Radius = 26f;

            backSphere.Center = new Vector3((float)(position.X - Math.Cos(Angle) * 15),
                (float)(position.Y - Math.Sin(Angle) * 15), 0);
            backSphere.Radius = 24f;
        }


        /// <summary>
        /// Perform the steps to move the tank.
        /// </summary>
        private void Move()
        {
            if (directionMovement != VTankObject.Direction.NONE)
            {
                float delta = (float)ServiceManager.Game.DeltaTime;
                float speed = (Constants.TANK_VELOCITY * this.Attributes.speedFactor) 
                    * delta;

                if (utilities.Count > 0)
                    foreach (Utility util in utilities)
                    {
                        speed += speed * util.speedFactor;
                    }

                if (delta > 1.0f)
                {
                    System.Console.Error.WriteLine(
                        "Warning: delta is too high (PlayerTank#Move()): {0}", delta);
                }

                TranslateRelativly(
                    (directionMovement == VTankObject.Direction.FORWARD
                        ? Vector3.Right : Vector3.Left) 
                    * speed);
            }
        }

        /// <summary>
        /// Perform the steps to rotate the tank.
        /// </summary>
        private void Rotate()
        {
            if (directionRotation != VTankObject.Direction.NONE)
            {
                float delta = (float)ServiceManager.Game.DeltaTime;
                float speed = (Constants.TANK_ANGULAR_VELOCITY * this.Attributes.speedFactor) 
                    * delta;

                if (utilities.Count > 0)
                    foreach (Utility util in utilities)
                    {
                        speed += speed * util.speedFactor;
                    }

                if (delta > 1.0f)
                {
                    System.Console.Error.WriteLine(
                        "Warning: delta is too high (PlayerTank#Rotate()): {0}", delta);
                }

                if (directionRotation == VTankObject.Direction.RIGHT)
                {
                    speed = -speed;
                }

                Angle += speed;
            }
        }

        /// <summary>
        /// Perform the steps to adjust the collision detection spheres.
        /// </summary>
        private void AdjustSpheres()
        {
            frontSphere.Center.X = (float)(position.X + Math.Cos(Angle) * 15);
            frontSphere.Center.Y = (float)(position.Y + Math.Sin(Angle) * 15);
            backSphere.Center.X = (float)(position.X - Math.Cos(Angle) * 15);
            backSphere.Center.Y = (float)(position.Y - Math.Sin(Angle) * 15);
        }

        /// <summary>
        /// Remove all utilities currently on the tank.
        /// </summary>
        public void RemoveAllUtilities()
        {
            foreach (ActiveUtility activeUtility in activeUtilities)
            {
                if (activeUtility.Emitter != null)
                    activeUtility.Emitter.Stop();
            }
            activeUtilities = new List<ActiveUtility>();
            utilities = new List<Utility>();


        }

        /// <summary>
        /// Remove utilities that have exceeded their duration.
        /// </summary>
        private void RemoveExpiredUtilities()
        {
            List<ActiveUtility> utilitiesToRemove = new List<ActiveUtility>();
            foreach (ActiveUtility activeUtility in activeUtilities)
            {
                if (activeUtility.IsExpired)
                    utilitiesToRemove.Add(activeUtility);
            }

            foreach (ActiveUtility activeUtility in utilitiesToRemove)
            {
                if (activeUtility.Emitter != null)
                    activeUtility.Emitter.Stop();
                Utilities.Remove(activeUtility.utility);
                activeUtilities.Remove(activeUtility);
            }
        }

        #endregion

        #region Public Methods
        /// <summary>
        /// Set the object which acts as this tank's turret.
        /// </summary>
        /// <param name="turretObject">Turret object to add.</param>
        public void SetTurret(int id, Object3 turretObject)
        {
            TurretRenderID = id;
            if (Team == GameSession.Alliance.BLUE)
            {
                turretObject.MeshColor = Color.Blue;

            }
            else if (Team == GameSession.Alliance.RED)
            {
                turretObject.MeshColor = Color.Red;
            }
            else
            {
                turretObject.MeshColor = Toolkit.GetColor(attributes.color);
            }

            Turret = turretObject;
        }

        /// <summary>
        /// Apply the skin to the tank.
        /// </summary>
        private void ApplyTankSkin()
        {
            ClearMeshSkins();

            Texture2D skinTexture = Toolkit.GetSkin(attributes.skin);
            const int ARBITRARY_CEILING = 2000;
            for (int i = 0; i < ARBITRARY_CEILING; ++i)
            {
                if (!AddMeshSkin("Body" + i, skinTexture))
                    break;
            }

            
        }

        /// <summary>
        /// Apply the skin to the turret.
        /// </summary>
        public void ApplyTurretSkin()
        {
            turret.ClearMeshSkins();

            Texture2D skinTexture = Toolkit.GetSkin(attributes.skin);
            const int ARBITRARY_CEILING = 2000;
            for (int i = 0; i < ARBITRARY_CEILING; ++i)
            {
                if (!turret.AddMeshSkin("Body" + i, skinTexture))
                    break;
            }
        }

        /// <summary>
        /// Apply a new skin to the tank.
        /// </summary>
        /// <param name="skinName"></param>
        public void ApplySkin(Texture2D skinTexture)
        {
            ClearMeshSkins();
            turret.ClearMeshSkins();

            const int ARBITRARY_CEILING = 2000;
            for (int i = 0; i < ARBITRARY_CEILING; ++i)
            {
                if (!AddMeshSkin("Body" + i, skinTexture))
                    break;
            }

            for (int i = 0; i < ARBITRARY_CEILING; ++i)
            {
                if (!turret.AddMeshSkin("Body" + i, skinTexture))
                    break;
            }
        }

        /// <summary>
        /// Convert an Ice position object to it's equivalent 3D vector.
        /// </summary>
        /// <param name="point">Position to set.</param>
        public void SetPosition(VTankObject.Point point)
        {
            this.Position = new Vector3((float)point.x, (float)point.y, this.Position.Z);
            if (HealthBar != null)
            {
                HealthBar.Position = new Vector3((float)point.x, (float)point.y, this.Position.Z);
            }
        }

        /// <summary>
        /// Inflict damage on the tank. Note that the damage is expected to be a positive value.
        /// If the damage is negative, it instead restores the tank's health.
        /// </summary>
        /// <param name="damage">Damage to inflict.</param>
        /// <returns>True if the tank was killed by this damage; false otherwise.</returns>
        public bool InflictDamage(int damage, bool killingBlow)
        {
            Health -= damage;

            if (killingBlow)
            {
                Alive = false;
                Health = 0;
                IsCharging = false;
            }

            // Check for de-sync condition.
            if (killingBlow && Health > 0)
            {
                Health = 1;
            }

            if (HealthBar != null)
            {
                HealthBar.Health = Health;
            }

            return !alive;
        }

        /// <summary>
        /// TODO: Document me.
        /// </summary>
        /// <returns></returns>
        public float GetRateOfFire()
        {
            float multiplier = 0;

            if (utilities.Count > 0)
            {
                foreach (Utility utility in utilities)
                {
                    multiplier += utility.rateFactor;
                }
            }

            return multiplier;
        }

        /// <summary>
        /// Add an assister (person who helps kill this person) to the "assist list".
        /// People on this list will be given points towards assisting kills if this player
        /// dies.
        /// 
        /// Note that the assister is not added if he is already in the list.
        /// </summary>
        /// <param name="assisterID">Player's temporary ID.</param>
        public void AddAssist(int assisterID)
        {
            if (!assistList.Contains(assisterID))
            {
                assistList.Add(assisterID);
            }
        }

        /// <summary>
        /// Remove assister from the assist list.
        /// </summary>
        /// <param name="assisterID"></param>
        /// <returns></returns>
        public bool RemoveAssister(int assisterID)
        {
            if (!assistList.Contains(assisterID))
            {
                return false;
            }

            assistList.Remove(assisterID);
            return true;
        }

        /// <summary>
        /// Clear everyone added to this player's assist list.
        /// </summary>
        public void ClearAssists()
        {
            assistList.Clear();
        }

        /// <summary>
        /// Get all those who helped damage this vehicle.
        /// </summary>
        /// <returns></returns>
        public List<int> GetAssisters()
        {
            return assistList;
        }

        /// <summary>
        /// Ask if this tank is capable of firing projectiles. It's incapable when the cooldown
        /// restricts it from doing so, or if the player is dead.
        /// </summary>
        /// <returns></returns>
        public bool CanFire()
        {

            if (this.Weapon.UsesOverHeat && this.Weapon.IsOverheating)
                return false;

            //if (this.Weapon.UsesOverHeat && this.Weapon.OverheatAmount >= this.weapon.OverheatTime)
            //{
              //  return false;
            //}

            return elapsed >= (weapon.Cooldown - (weapon.Cooldown 
                * this.GetRateOfFire())) && Alive;
        }

        /// <summary>
        /// Sets this tank's weapon cooldown back to zero, allowing it to "cool down".
        /// </summary>
        public void ResetWeaponCooldown()
        {
            elapsed = 0;
        }

        /// <summary>
        /// Tell the tank to set it's health to the maximum and to shift it's position.
        /// </summary>
        /// <param name="where">Position to respawn at.</param>
        public void Respawn(VTankObject.Point where)
        {
            Health = MaxHealth;
            if (HealthBar != null)
            {
                HealthBar.Health = MaxHealth;
            }
            else
            {
                // This player is the local player.
                // TODO: This is done as a temporary fix to the 'server doesn't know where you are' bug.
                ServiceManager.Theater.Move(where.x, where.y, Direction.NONE);
            }

            SetPosition(where);
            Alive = true;
            elapsed = weapon.Cooldown;
        }

        /// <summary>
        /// Apply a utility to the tank.
        /// </summary>
        /// <param name="utility">The utility</param>
        public void ApplyUtility(VTankObject.Utility utility)
        {
            Utilities.Add(utility);
            ActiveUtility autil = new ActiveUtility(utility);

            ParticleEmitterSettings pset = Renderer.RendererAssetPool.ParticleEmitterSettings["Utility"];
            pset.LifeSpan = utility.duration;
            pset.ParticleSystemName = utility.model;
            autil.Emitter = new ParticleEmitter(pset);
            autil.Emitter.Position = Position;
            autil.Emitter.MimicPosition(this, Vector3.Zero);
            ServiceManager.Scene.Add(autil.Emitter, 3);

            activeUtilities.Add(autil);
        }

        /// <summary>
        /// Apply an instant health utility to the tank
        /// </summary>
        /// <param name="utility">The utility</param>
        public void ApplyInstantHealth(VTankObject.Utility utility)
        {
            int healthBonus = (int)(MaxHealth * utility.healthFactor) + utility.healthIncrease;

            if (this.Health + healthBonus > MaxHealth)
            {
                //For updating the healthbar, inflictdamage heals with negative values
                this.InflictDamage(-(MaxHealth - Health), false);
            }
            else
            {
                this.InflictDamage(-healthBonus, false);
            }
            ParticleEmitterSettings pset = Renderer.RendererAssetPool.ParticleEmitterSettings["Utility"];
            pset.ParticleSystemName = utility.model;
            ParticleEmitter pemit1 = new ParticleEmitter(pset);
            pemit1.Position = Position;
            pemit1.MimicPosition(this, Vector3.Zero);
            pemit1.MimicRotation(this);
            ServiceManager.Scene.Add(pemit1, 3);
        }

        #endregion

        #region Overridden Methods
        /// <summary>
        /// Update the player object, transforming it's position according to whether or not
        /// it's currently moving or rotating. This method is called by the renderer.
        /// </summary>
        /// <param name="updateToggle">Toggle known to the renderer.</param>
        public override void Update(bool updateToggle)
        {
            this.TimeSinceFiring += ServiceManager.Game.DeltaTime;

            if (Alive)
            {
                if (this.attributes.model == "Icarus")
                {
                    hoverValue.Update();
                    this.Translate(hoverValue.AsVector3);
                }

                if (base.HasUpdated != updateToggle)
                {
                    Move();
                    Rotate();
                    AdjustSpheres();

                    if (elapsed < weapon.Cooldown)
                    {
                        elapsed += ServiceManager.Game.DeltaTime;
                    }

                    if (PreviouslyCollidied)
                    {
                        DirectionMovement = LastDirectionMovement;
                    }

                    if (this.HealthBar != null)
                    {
                        if (this.HealthBar.Position != this.Position)
                        {
                            this.HealthBar.Position = this.Position;
                        }
                    } 
                    if (this.NameObject != null)
                    {
                       this.NameObject.Update(updateToggle);
                       this.NameObject.Position = this.Position + Vector3.UnitZ*120;
                    }

                    this.RemoveExpiredUtilities();
                }
            }
            
            base.Update(updateToggle);
        }
        #endregion
    }
}
