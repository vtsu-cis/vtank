using System;
using System.Collections.Generic;
using System.Text;

namespace AIFramework.Bot.EventArgs
{
    public class MapRotationEventArgs
    {
        #region Properties
        public string MapName
        {
            get;
            private set;
        }

        public VTankObject.GameMode GameMode
        {
            get;
            private set;
        }
        #endregion

        #region Constructor
        public MapRotationEventArgs(string mapName, VTankObject.GameMode gameMode)
        {
            MapName = mapName;
            GameMode = gameMode;
        }
        #endregion
    }
}
