using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

namespace Client.src.util.game
{
    /// <summary>
    /// Class for a VTank weapon, as described by its XML file.
    /// </summary>
    public class Weapon
    {
        #region Structs
        /// <summary>
        /// Represents a mount point and associated particle emitter to use on it.
        /// </summary>
        public struct ParticleEmitter
        {
            public string name;
            public string particleName;
        }

        /// <summary>
        /// Describes an animation that this weapon has.
        /// </summary>
        public struct ModelAnimation
        {
            public string name;
            public int startingFrame;
            public int endingFrame;
            public bool isContinuous;
            public string handlerClass;
        }
        #endregion

        #region Members
        int id;
        string name;
        bool customColor;
        Color customColorValue;
        string weaponFireSound;
        string colorMesh;
        string model;
        string deadModel;
        string muzzleEffectName;
        List <ParticleEmitter> emitters;
        List <ModelAnimation> animations;
        int projectileId;
        ProjectileData projectile = null;
        float cooldown;
        float launchAngle;
        float maxChargeTime = -1;
        int projectilesPerShot;
        float intervalBetweenEachProjectile;
        float overheatTime;
        float overheatAmountPerShot;
        float overheatRecoverySpeed;
        float overheatRecoveryStartTime;

        int emitterIndex = 0;
        bool isOverheating = false;
        #endregion

        #region Constructors
        /// <summary>
        /// Constructor for a Weapon object, meant to be instantiated by by the XML loader.
        /// </summary>
        public Weapon(int id, string name, bool customColor, Color customColorValue, string weaponFireSound, string colorMesh,
            string model, string deadModel, string muzzleEffectName, List<ParticleEmitter> emitters, 
            List<ModelAnimation> animations, int projectileId, float cooldown, float launchAngle, float maxChargeTime,
            int projectilesPerShot, float intervalBetweenEachProjectile, float overheatTime, float overheatAmountPerShot,
            float overheatRecoverySpeed, float overheatRecoveryStartTime)
        {
            this.id = id;
            this.name = name;
            this.customColor = customColor;
            this.weaponFireSound = weaponFireSound;
            this.customColorValue = customColorValue;
            this.colorMesh = colorMesh;
            this.model = model;
            this.deadModel = deadModel;
            this.muzzleEffectName = muzzleEffectName;
            this.emitters = emitters;
            this.animations = animations;
            this.projectileId = projectileId;
            this.cooldown = cooldown;
            this.launchAngle = launchAngle;
            this.maxChargeTime = maxChargeTime;
            this.projectilesPerShot = projectilesPerShot;
            this.intervalBetweenEachProjectile = intervalBetweenEachProjectile;
            this.overheatTime = overheatTime;
            this.overheatAmountPerShot = overheatAmountPerShot;
            this.overheatRecoverySpeed = overheatRecoverySpeed;
            this.overheatRecoveryStartTime = overheatRecoveryStartTime;

            this.OverheatAmount = 0f;
        }
        #endregion

        #region Properties
        public int ID { get { return this.id; } }
        public int ProjectileID { get { return this.projectileId; } }
        public float Cooldown { get { return this.cooldown; } }
        public string Model { get { return this.model; } }
        public string DeadModel { get { return this.deadModel; } }
        public string Name { get { return this.name; } }
        public bool CanCharge
        {
            get
            {
                if (this.maxChargeTime < 0)
                    return false;
                else if (this.maxChargeTime > 0)
                    return true;
                else
                    return false;
            }
        }
        public string MuzzleEffectName
        {
            get { return this.muzzleEffectName; }
        }
        public ProjectileData Projectile
        {
            get 
            {
                if (this.projectile == null)
                    this.projectile = WeaponLoader.GetProjectile(this.projectileId);

                return this.projectile; 
            }
        }
        public string FiringSound
        {
            get { return this.weaponFireSound; }
        }
        public float LaunchAngle
        {
            get { return launchAngle; }
        }
        public bool HasFiringArc
        {
            get
            {
                if (this.launchAngle != 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        public bool UsesOverHeat
        {
            get
            {
                if (this.overheatTime != 0)
                    return true;
                else
                    return false;
            }
        }
        public float OverheatAmountPerShot
        {
            get { return this.overheatAmountPerShot; }
        }
        public float OverheatTime
        {
            get { return this.overheatTime; }
        }
        public float OverheatRecoverySpeed
        {
            get { return this.overheatRecoverySpeed; }
        }
        public float OverheatRecoveryStartTime
        {
            get
            {
                return this.overheatRecoveryStartTime;
            }
        }
        /// <summary>
        /// Find out whether a tank's weapon is currently overheating.
        /// </summary>
        public bool IsOverheating
        {
            get { return isOverheating; }
            set
            {
                isOverheating = value;

                if (isOverheating)
                {
                    TimeOverheatOccurred = Network.Util.Clock.GetTimeMilliseconds();
                }
            }
        }

        /// <summary>
        /// Get the time a tank overheated his weapon.
        /// </summary>
        public long TimeOverheatOccurred
        {
            get;
            private set;
        }
        public float OverheatAmount
        {
            get;
            set;
        }
        public float MaxChargeTime
        {
            get { return this.maxChargeTime; }
        }
        #endregion

        #region Methods
        public override string ToString()
        {
            return String.Format("id: {0}, name: {1}, customColor: {2}, customColorValue: {3}, colorMesh: {4} \n" +
                "model: {5}, deadModel: {6}, muzzleEffectName: {7}, emitters: {8}, animations: {9}\n" + 
                "projectileId: {10}, cooldown: {11}, launchAngle: {12}, maxChargeTime: {13}\n" +
                "projectilesPerShot: {14}, intervalBetweenEachProjectile: {15}, overheatTime: {15}, overheatAmountPerShot: {16}\n" + 
                "overheatRecoverySpeed: {17}, overheatRecoveryStartTime: {18}", id, name, customColor, customColorValue,
                colorMesh, model, deadModel, muzzleEffectName, emitters.ToString(), animations.ToString(), projectileId,
                cooldown, launchAngle, maxChargeTime, projectilesPerShot, intervalBetweenEachProjectile, overheatTime,
                overheatAmountPerShot, overheatRecoverySpeed, overheatRecoveryStartTime);
        }

        /// <summary>
        /// Get emitters in sequence for projectiles to attach themselves to.
        /// </summary>
        /// <returns>Emitter index.</returns>
        public int GetNextEmitterIndex() {
           if (emitterIndex >= emitters.Count) {
               emitterIndex = 0;
           }

           int index = emitterIndex++;

           return index;
        }
        #endregion
    }
}
