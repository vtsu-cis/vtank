using System;
using System.Collections.Generic;
using System.Text;
using AIFramework.Bot.Game;
using System.IO;
using Network.Main;

namespace AIFramework.Util
{
    static class MapDownloader
    {
        private static string cachedMapName = null;
        private static Map cachedMap = null;
        private static object locker = new object();

        /// <summary>
        /// Get a map in a thread-safe way. It's expected that multiple bots will try
        /// to grab the same map simultaneously. This prevents them from:
        /// a) allocating more than one Map object;
        /// b) reading the same file simultaneously, causing locking issues; and
        /// c) downloading the map more than once.
        /// </summary>
        /// <param name="filename">File name of the map.</param>
        /// <param name="saveTo">The location of the map on the hard drive.</param>
        /// <param name="main">Main communicator.</param>
        /// <returns>The map object.</returns>
        public static Map GetMap(string filename, string saveTo, MasterCommunicator main)
        {
            lock (locker)
            {
                Map playableMap = null;

                FileInfo file = new FileInfo(saveTo);
                if (!file.Exists)
                {
                    playableMap = AttemptDownload(filename, main);
                }
                else
                {
                    playableMap = new Map(saveTo);
                    string hash = playableMap.SHA1Hash;
                    if (!main.GetProxy().HashIsValid(filename, hash))
                    {
                        Debugger.Write("Map {0} is outdated; downloading it...", filename);
                        playableMap = AttemptDownload(filename, main);
                    }
                }

                cachedMap = playableMap;
                cachedMapName = filename;
                return playableMap;
            }
        }

        /// <summary>
        /// Repeatedly attempt to download a map.
        /// </summary>
        /// <param name="currentMapName"></param>
        private static Map AttemptDownload(string currentMapName, MasterCommunicator main)
        {
            Map playableMap = null;
            bool done = false;
            int tries = 0;
            const int MaxTries = 3;
            while (!done)
            {
                playableMap = DownloadMap(currentMapName, main);
                playableMap.SaveMap();
                string hash = playableMap.SHA1Hash;

                if (!main.GetProxy().HashIsValid(currentMapName, hash))
                {
                    if (tries++ >= MaxTries)
                    {
                        throw new Exception(String.Format(
                            "The download for the map file {0} keeps being corrupted.",
                            currentMapName));
                    }

                    Debugger.Write("Download of map {0} is corrupted -- {1} more {2}...",
                        currentMapName, MaxTries - tries, tries == 1 ? "try" : "tries");
                }
                else
                {
                    done = true;
                }
            }

            Debugger.Write("Downloaded and saved map {0}.", currentMapName);

            return playableMap;
        }

        /// <summary>
        /// Performs the actual map download operation.
        /// </summary>
        /// <param name="localPath">Local path in the operating system.</param>
        /// <param name="mapFileName"></param>
        /// <returns></returns>
        private static Map DownloadMap(string mapFileName, MasterCommunicator main)
        {
            Debugger.Write("Downloading map {0}...", mapFileName);
            VTankObject.Map map = main.GetProxy().DownloadMap(mapFileName);
            string title = map.title;
            int width = map.width;
            int height = map.height;
            VTankObject.Tile[] tiles = map.tileData;
            Tile[] realTiles = new Tile[tiles.Length];

            Map newMap = new Map(title, mapFileName, (uint)width, (uint)height);

            int size = width * height;
            for (int y = 0; y < height; ++y)
            {
                for (int x = 0; x < width; ++x)
                {
                    VTankObject.Tile relevantTile = tiles[y * width + x];
                    newMap.SetTile((uint)x, (uint)y, new Tile(
                        (uint)relevantTile.id, (ushort)relevantTile.objectId,
                        (ushort)relevantTile.eventId, relevantTile.passable,
                        (ushort)relevantTile.height, (ushort)relevantTile.type,
                        (ushort)relevantTile.effect));
                }
            }

            List<int> buf = new List<int>();
            for (int i = 0; i < map.supportedGameModes.Length; i++)
            {
                buf.Add(map.supportedGameModes[i]);
            }

            newMap.SetGameModes(buf);

            return newMap;
        }
    }
}
