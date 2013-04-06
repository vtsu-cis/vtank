using System;
using AIFramework.Bot.Game;

namespace AIFramework.Bot.EventArgs
{
    public class PlayerJoinedEventArgs : IBotEvent
    {
        #region Properties
        /// <summary>
        /// Gets the player who has joined the game.
        /// </summary>
        public Player Player
        {
            get;
            private set;
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates event arguments about a player.
        /// </summary>
        /// <param name="playerID">The player's ID number.</param>
        public PlayerJoinedEventArgs(Player player)
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
