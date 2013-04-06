using System;
using System.Collections.Generic;
using System.Text;
using AIFramework.Bot.Game;

namespace AIFramework.Bot.EventArgs
{
    public class PlayerRespawnEventArgs : IBotEvent
    {
        #region Properties
        /// <summary>
        /// Gets the player who has respawned.
        /// </summary>
        public Player Player
        {
            get;
            private set;
        }
        #endregion

        #region Constructor
        public PlayerRespawnEventArgs(Player player)
        {
            Player = player;
        }
        #endregion

        public override void Dispose()
        {
            Player = null;
        }
    }
}
