using System;
using System.Collections.Generic;
using System.Text;
using AIFramework.Bot.Game;

namespace AIFramework.Bot.EventArgs
{
    public class PlayerLeftEventArgs : IBotEvent
    {
        #region Properties
        /// <summary>
        /// Gets the player who left.
        /// </summary>
        public Player Player
        {
            get;
            private set;
        }
        #endregion

        #region Constructor
        /// <summary>
        /// 
        /// </summary>
        /// <param name="player"></param>
        public PlayerLeftEventArgs(Player player)
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
