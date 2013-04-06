using System;
using System.Collections.Generic;
using System.Text;

namespace AIFramework.Bot.Game
{
    /// <summary>
    /// Class for a VTank environment effect, as described by its XML file.
    /// </summary>
    public class EnvironmentProperty
    {
        #region Member
        int id;
        string name;
        bool triggersUponImpactWithEnvironment;
        bool triggersUponImpactWithPlayer;
        bool triggersUponExpiration;

        string particleEffect;
        float duration;
        float interval;
        float areaOfEffectRadius;
        float areaOfEffectDecay;

        int minDamage;
        int maxDamage;
        #endregion

        #region Properties
        public int ID { get { return id; } }
        #endregion

        #region Constructors
        public EnvironmentProperty(int id, string name, bool triggersUponImpactWithEnvironment,
            bool triggersUponImpactWithPlayer, bool triggersUponExpiration, string particleEffect,
            float duration, float interval, float areaOfEffectRadius, float areaOfEffectDecay,
            int minDamage, int maxDamage)
        {
            this.id = id;
            this.name = name;
            this.triggersUponImpactWithEnvironment = triggersUponImpactWithEnvironment;
            this.triggersUponImpactWithPlayer = triggersUponImpactWithPlayer;
            this.triggersUponExpiration = triggersUponExpiration;
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
        public override string ToString()
        {
            return String.Format("id: {0}, name: {1}, triggersUponImpactWithEnvironment: {2} \n" +
                "triggersUponImpactWithPlayer: {3}, triggersUponExpiration: {4}, particleEffect: {5}, \n" +
                "duration: {6}, interval: {7}, areaOfEffectRadius: {8} \n" +
                "areaOfEffectDecay: {9}, minDamage: {10}, maxDamage: {11}", id, name, triggersUponImpactWithEnvironment,
                triggersUponImpactWithPlayer, triggersUponExpiration, particleEffect, duration, interval, 
                areaOfEffectRadius, areaOfEffectDecay, minDamage, maxDamage);
        }
        #endregion
    }
}
