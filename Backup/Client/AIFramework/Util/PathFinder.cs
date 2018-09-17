using System;
using System.Collections.Generic;
using System.Text;
using AIFramework.Util.PathAlgorithms;
using AIFramework.Bot.Game;

namespace AIFramework.Util
{
    public static class PathFinder
    {
        #region Properties
        public static IPathfindingAlgorithm Algorithm
        {
            get;
            set;
        }
        #endregion

        #region Constructor
        static PathFinder()
        {
            Algorithm = new AStarAlgorithm();
        }
        #endregion

        #region Methods
        public static Path FindPath(Map map, Player player,
            int destinationPixelsX, int destinationPixelsY)
        {
            return Path.Optimize(
                Algorithm.FindPath(map, player, destinationPixelsX, destinationPixelsY));
        }
        #endregion
    }
}
