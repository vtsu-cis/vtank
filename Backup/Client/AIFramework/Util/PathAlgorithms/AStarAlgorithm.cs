using System;
using System.Collections.Generic;
using System.Text;
using AIFramework.Bot.Game;

namespace AIFramework.Util.PathAlgorithms
{
    /// <summary>
    /// Represents the A* pathfinding algorithm. This algorithm is similar to
    /// Dijkstra's algorithm, except it's more suitable in this case: Dijkstra's
    /// algorithm is more suitable in cases where we do not know where the target
    /// is located. We know the target, so it's faster to use A*.
    /// </summary>
    public class AStarAlgorithm : IPathfindingAlgorithm
    {
        #region Members
        #endregion

        #region Constructor
        public AStarAlgorithm()
        {
        }
        #endregion

        #region IPathfindingAlgorithm Members
        /// <summary>
        /// Find the shortest path to the destination using the A* algorithm.
        /// </summary>
        /// <param name="map"></param>
        /// <param name="player"></param>
        /// <param name="destinationX">The destination x (in "pixels").</param>
        /// <param name="destinationY">The destination y (in "pixels").</param>
        /// <exception cref="InvalidOperationException">Thrown if the target destination
        /// is unreachable.</exception>
        public Path FindPath(Map map, Player player, int destinationX, int destinationY)
        {
            MinotaurPathfinder.Pathfinder pathfinder = new MinotaurPathfinder.Pathfinder(map);
            pathfinder.Pathfind(map, player, destinationX, destinationY);

            Path path = new Path();
            foreach (MinotaurPathfinder.MetaCompleteSquare square in pathfinder.FullPath())
            {
                path.AppendNode(new Node(square.X, square.Y));
            }

            pathfinder.Dispose();

            return path;
        }
        #endregion
    }
}
