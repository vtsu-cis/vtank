using System;
using System.Collections.Generic;
using System.Text;

namespace Client.src.util.game
{
    /// <summary>
    /// Class for a VTank projectile, as described by its XML file.
    /// </summary>
    public class ProjectileData
    {
        #region Members
        Random randNumber = new Random();
        int id;
        string name;
        string model;
        float projectileScale;
        string particleEffectName;
        string impactParticleName;
        string impactSoundEffect;
        string expirationParticleName;
        List<Weapon.ParticleEmitter> emitters;
        List<Weapon.ModelAnimation> animations;
        int areaOfEffectRadius;
        bool areaOfEffectUsesCone;
        float areaOfEffectDecay;
        int coneRadius;
        int coneOriginWidth;
        bool coneDamagesEntireArea;
        int minDamage;
        int maxDamage;
        bool isInstantaneous;
        int initialVelocity;
        int terminalVelocity;
        int acceleration;
        int range;
        int rangeVariation;
        int jumpRange;
        float jumpDamageDecay;
        int jumpCount;
        int environmentEffectID;
        bool hasScatter;
        float collisionRadius;

        EnvironmentProperty environmentEffect;

        #endregion

        #region Properties
        public int ID { get { return this.id; } }
        public string Name { get { return this.name; } }
        public int InitialVelocity { get { return this.initialVelocity; } }
        public int TerminalVelocity { get { return this.terminalVelocity; } }
        public string Model { get { return this.model; } }
        public int Range { get { return this.range; } }
        public int AverageDamage { get { return (minDamage + maxDamage) / 2; } }
        public int Acceleration { get { return this.acceleration; } }
        public EnvironmentProperty EnvProperty
        {
            get
            {
                return this.environmentEffect;
            }
        }
        public bool HasScatter
        {
            get { return this.hasScatter; }
        }
        public string ExpirationParticleName
        {
            get { return this.expirationParticleName; }
        }
        public string ImpactParticleName
        {
            get { return this.impactParticleName; }
        }
        public string ImpactSoundEffect
        {
            get { return this.impactSoundEffect; }
        }
        public string ParticleEffect
        {
            get { return this.particleEffectName; }
        }
        public bool IsInstantaneous
        {
            get { return this.isInstantaneous; }
        }
        public int AreaOfEffectRadius
        {
            get { return this.areaOfEffectRadius; }
        }

        public float CollisionRadius
        {
            get { return this.collisionRadius; }
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Constructor for ProjectileData.  This class is meant to be instantiated by the XML loader.
        /// </summary>
        public ProjectileData(int id, string name, string model, float projectileScale, string particleEffectName,
            string impactParticleName, string impactSoundEffect, string expirationParticleName, List<Weapon.ParticleEmitter> emitters, List<Weapon.ModelAnimation> animations,
            int areaOfEffectRadius, bool areaOfEffectUsesCone, float areaOfEffectDecay, int coneRadius, int coneOriginWidth, bool coneDamagesEntireArea,
            int minDamage, int maxDamage, bool isInstantaneous, int initialVelocity, int terminalVelocity, int acceleration, int range,
            int rangeVariation, int jumpRange, float jumpDamageDecay, int jumpCount, int environmentEffectID, float radius, EnvironmentProperty envProperty)
        {
            this.id = id;
            this.name = name;
            this.model = model;
            this.projectileScale = projectileScale;
            this.particleEffectName = particleEffectName;
            this.impactParticleName = impactParticleName;
            this.impactSoundEffect = impactSoundEffect;
            this.expirationParticleName = expirationParticleName;
            this.emitters = emitters;
            this.animations = animations;
            this.areaOfEffectRadius = areaOfEffectRadius;
            this.areaOfEffectUsesCone = areaOfEffectUsesCone;
            this.areaOfEffectDecay = areaOfEffectDecay;
            this.coneRadius = coneRadius;
            this.coneOriginWidth = coneOriginWidth;
            this.coneDamagesEntireArea = coneDamagesEntireArea;
            this.minDamage = minDamage;
            this.maxDamage = maxDamage;
            this.isInstantaneous = isInstantaneous;
            this.initialVelocity = initialVelocity;
            this.terminalVelocity = terminalVelocity;
            this.acceleration = acceleration;
            this.range = range;
            this.rangeVariation = rangeVariation;
            this.jumpRange = jumpRange;
            this.jumpDamageDecay = jumpDamageDecay;
            this.jumpCount = jumpCount;
            this.environmentEffectID = environmentEffectID;
            this.collisionRadius = radius;
            this.environmentEffect = envProperty;

            if (this.coneRadius != 0 && this.areaOfEffectUsesCone == false)
            {
                this.hasScatter = true;
            }
            else
            {
                this.hasScatter = false;
            }
        }
        #endregion
    }
}
