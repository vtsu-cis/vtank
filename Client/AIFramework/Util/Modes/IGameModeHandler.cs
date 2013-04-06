using System;
using AIFramework.Bot.Game;

namespace AIFramework.Util.Modes
{
    /// <summary>
    /// Interface which game mode handlers (e.g. capture the flag handler) must implement.
    /// </summary>
    public abstract class GameModeHandler : IDisposable
    {
        protected delegate void DisposeEventHandler();
        protected event DisposeEventHandler OnDispose;

        #region Properties
        /// <summary>
        /// Gets the current map.
        /// </summary>
        public Map CurrentMap
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the current game mode.
        /// </summary>
        public VTankObject.GameMode CurrentGameMode
        {
            get;
            private set;
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Constructs the game mode.
        /// </summary>
        /// <param name="currentMap"></param>
        /// <param name="currentGameMode"></param>
        public GameModeHandler(Map currentMap, VTankObject.GameMode currentGameMode)
        {
            CurrentMap = currentMap;
            CurrentGameMode = currentGameMode;
        }
        #endregion

        #region Overrideable
        public abstract void Update(GameTracker game, double deltaTimeSeconds);
        #endregion

        #region IDisposable Members

        /// <summary>
        /// Disposes of the game mode.
        /// </summary>
        public virtual void Dispose()
        {
            if (OnDispose != null)
                OnDispose.Invoke();

            OnDispose = null;
            CurrentMap = null;
        }

        #endregion
    }
}
