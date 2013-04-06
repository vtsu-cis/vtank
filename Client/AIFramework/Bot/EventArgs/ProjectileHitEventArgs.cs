using System;
using System.Collections.Generic;
using System.Text;
using AIFramework.Bot.Game;

namespace AIFramework.Bot.EventArgs
{
    public class ProjectileHitEventArgs : IBotEvent
    {
        #region Properties
        /// <summary>
        /// Gets the player who has been hit.
        /// </summary>
        public Player Victim
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the amount of damage dealt.
        /// </summary>
        public int DamageDealt
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets whether the shot killed the player.
        /// </summary>
        public bool KillingBlow
        {
            get;
            private set;
        }
        #endregion

        #region Constructor
        public ProjectileHitEventArgs(Player victim, int damage, bool killingBlow)
        {
            Victim = victim;
            DamageDealt = damage;
            KillingBlow = killingBlow;
        }
        #endregion

        public override void Dispose()
        {
            Victim = null;
        }
    }
}
