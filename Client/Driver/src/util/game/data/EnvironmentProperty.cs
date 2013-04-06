using System;
using System.Collections.Generic;
using System.Text;
using Client.src.service;

namespace Client.src.util.game
{
    /// <summary>
    /// Class for a VTank environment property, as described by its XML file.
    /// </summary>
    public class EnvironmentProperty
    {
        #region Member
        int id;
        string name;
        bool triggersUponImpactWithEnvironment;
        bool triggersUponImpactWithPlayer;
        bool triggersUponExpiration;

        string soundEffect;
        string particleEffect;
        float duration;
        float interval;
        float areaOfEffectRadius;
        float areaOfEffectDecay;

        int minDamage;
        int maxDamage;

        long creationTime = -1;
        #endregion

        #region Data Properties
        public int ID { get { return id; } }
        public bool TriggersUponImpactWithEnvironment
        {
            get { return this.triggersUponImpactWithEnvironment; }
        }
        public bool TriggersUponImpactWithPlayer
        {
            get { return this.triggersUponImpactWithPlayer; }
        }
        public bool TriggersUponExpiration
        {
            get { return this.triggersUponExpiration; }
        }
        public string ParticleEffectName
        {
            get { return particleEffect; }
        }
        public string SoundEffect
        {
            get { return this.soundEffect; }
        }
        public float Duration
        {
            get { return duration; }
        }
        public float AoERadius
        {
            get { return this.areaOfEffectRadius; }
        }
        #endregion

        #region Active Properties
        /*
         *These should probably be moved to a different class altogether. 
         */

        /// <summary>
        /// Indicates whether this environment effect is expired.
        /// </summary>
        public bool Expired
        {
            get
            {
                long now = Network.Util.Clock.GetTimeMilliseconds();
                if (now > (this.duration * 1000) + this.creationTime)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// The render ID of this environment property.
        /// </summary>
        public int RenderID
        {
            get;
            set;
        }

        /// <summary>
        /// The owner of an instance of this property (used when it is active).
        /// </summary>
        public PlayerTank Owner
        {
            get;
            set;
        }        
        #endregion

        #region Constructors
        /// <summary>
        /// Constructor for an environment property, meant to be instantiated by the WeaponLoader class from
        /// the XML file for environment properties.
        /// </summary>
        public EnvironmentProperty(int id, string name, bool triggersUponImpactWithEnvironment,
            bool triggersUponImpactWithPlayer, bool triggersUponExpiration, string soundEffect, string particleEffect,
            float duration, float interval, float areaOfEffectRadius, float areaOfEffectDecay,
            int minDamage, int maxDamage)
        {
            this.id = id;
            this.name = name;
            this.triggersUponImpactWithEnvironment = triggersUponImpactWithEnvironment;
            this.triggersUponImpactWithPlayer = triggersUponImpactWithPlayer;
            this.triggersUponExpiration = triggersUponExpiration;
            this.soundEffect = soundEffect;
            this.particleEffect = particleEffect;
            this.duration = duration;
            this.interval = interval;
            this.areaOfEffectRadius = areaOfEffectRadius;
            this.areaOfEffectDecay = areaOfEffectDecay;
            this.minDamage = minDamage;
            this.maxDamage = maxDamage;            
        }
        #endregion

        #region Methods
        /// <summary>
        /// Sets the creation time of this environment effect to the current time.
        /// </summary>
        public void SetCreationTime()
        {
            this.creationTime = Network.Util.Clock.GetTimeMilliseconds();
        }
        #endregion
    }
}
