using System;
using System.Collections.Generic;
using System.Text;

namespace AIFramework.Bot.Game
{
    /// <summary>
    /// Class for a VTank projectile, as described by its XML file.
    /// </summary>
    public class ProjectileData
    {
        #region Members
        int id;
        string name;
        string model;
        float projectileScale;
        string particleEffectName;
        string impactParticleName;
        string expirationParticleName;
        List<Weapon.ParticleEmitter> emitters;
        List<Weapon.ModelAnimation> animations;
        int areaOfEffectRadius;
        bool areaOfEffectUsesCone;
        float areaOfEffectDecay;
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

        EnvironmentProperty environmentEffect;

        #endregion

        #region Properties
        public int ID { get { return this.id; } }
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
        #endregion

        #region Constructors
        public ProjectileData(int id, string name, string model, float projectileScale, string particleEffectName,
            string impactParticleName, string expirationParticleName, List<Weapon.ParticleEmitter> emitters, List<Weapon.ModelAnimation> animations,
            int areaOfEffectRadius, bool areaOfEffectUsesCone, float areaOfEffectDecay, int coneOriginWidth, bool coneDamagesEntireArea,
            int minDamage, int maxDamage, bool isInstantaneous, int initialVelocity, int terminalVelocity, int acceleration, int range,
            int rangeVariation, int jumpRange, float jumpDamageDecay, int jumpCount, int environmentEffectID, EnvironmentProperty envProperty)
        {
            this.id = id;
            this.name = name;
            this.model = model;
            this.projectileScale = projectileScale;
            this.particleEffectName = particleEffectName;
            this.impactParticleName = impactParticleName;
            this.expirationParticleName = expirationParticleName;
            this.emitters = emitters;
            this.animations = animations;
            this.areaOfEffectRadius = areaOfEffectRadius;
            this.areaOfEffectUsesCone = areaOfEffectUsesCone;
            this.areaOfEffectDecay = areaOfEffectDecay;
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
            this.environmentEffect = envProperty;
        }
        #endregion

        #region Methods

        public override string ToString()
        {
            return String.Format("id: {0}, name: {1}, model: {2}, projectileScale: {3}, particleEffectName: {4} \n" +
                "impactParticleName: {5}, expirationParticleName: {6}, areaOfEffectRadius: {7}, areaOfEffectUsesCone: {8} \n" +
                "areaOfEffectDecay: {9}, coneOriginWidth: {10}, coneDamagesEntireArea: {11}, minDamage: {12}, maxDamage: {13} \n" +
                "isInstantaneous: {14}, initialVelocity: {15}, terminalVelocity: {16}, acceleration: {17}, range: {18} \n" +
                "rangeVariation: {19}, jumpRange: {20}, jumpDamageDecay: {21}, jumpCount: {22}, environmentEffectID: {23}", id,
                name, model, projectileScale, particleEffectName, impactParticleName, expirationParticleName, areaOfEffectRadius,
                areaOfEffectUsesCone, areaOfEffectDecay, coneOriginWidth, coneDamagesEntireArea, minDamage,
                maxDamage, isInstantaneous, initialVelocity, terminalVelocity, acceleration, range, rangeVariation, jumpRange,
                jumpDamageDecay, jumpCount, environmentEffectID);
        }
        #endregion
    }
}
