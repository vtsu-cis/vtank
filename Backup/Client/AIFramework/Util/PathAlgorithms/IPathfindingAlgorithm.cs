using System;
using AIFramework.Bot.Game;

namespace AIFramework.Util.PathAlgorithms
{
    public interface IPathfindingAlgorithm
    {
        Path FindPath(Map map, Player player, int destinationX, int destinationY);
    }
}
