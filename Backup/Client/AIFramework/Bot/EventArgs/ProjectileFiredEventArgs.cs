using System;
using System.Collections.Generic;
using System.Text;
using AIFramework.Bot.Game;

namespace AIFramework.Bot.EventArgs
{
    public class ProjectileFiredEventArgs : IBotEvent
    {
        #region Properties
        /// <summary>
        /// Gets the player who fired the projectile.
        /// </summary>
        public Player PlayerWhoFired
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the information about the projectile.
        /// </summary>
        public Projectile Projectile
        {
            get;
            private set;
        }
        #endregion

        #region Constructor/Destructor
        /// <summary>
        /// 
        /// </summary>
        /// <param name="playerWhoFired"></param>
        /// <param name="projectile"></param>
        public ProjectileFiredEventArgs(Player playerWhoFired, Projectile projectile)
        {
            PlayerWhoFired = playerWhoFired;
            Projectile = projectile;
        }
        #endregion

        public override void Dispose()
        {
            PlayerWhoFired = null;
            Projectile = null;
        }
    }
}
