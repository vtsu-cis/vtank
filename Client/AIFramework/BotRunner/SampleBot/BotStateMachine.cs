using System;
using System.Collections.Generic;
using System.Text;
using VTankObject;

namespace VTankBotRunner.SampleBot
{
    using AIFramework.Util;
    using AIFramework.Bot.Game;
    using AIFramework.Bot;
    using AIFramework.Util.Modes;
    using AIFramework.Util.Modes.Impl;

    /// <summary>
    /// Control how the bot (should) behave.
    /// </summary>
    internal class BotStateMachine
    {
        #region Members
        private List<TileNode> spawnPointList;
        private int spawnPointIndex;
        private GameMode mode;
        private AIFramework.Bot.Game.Map map;
        private bool refreshSpawnPointList;
        private BotState state;
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the current bot state. Note that the state is reverted to SEEK_AND_DESTROY
        /// if the given state is invalid for the current game mode. This also means that the game
        /// mode should be set before the state is set.
        /// </summary>
        public BotState State 
        {
            get
            {
                return state;
            }

            set
            {
                state = value;

                // Check if the assigned state makes sense. If it doesn't, set it to the default.
                if ((state == BotState.COMPLETE_OBJECTIVE ||
                    state == BotState.PROTECT_OBJECTIVE) &&
                    (GameMode == GameMode.DEATHMATCH || GameMode == GameMode.TEAMDEATHMATCH))
                {
                    state = BotState.SEEK_AND_DESTROY;
                }

                if (state == BotState.FOLLOW_ALLY && GameMode == GameMode.DEATHMATCH)
                {
                    state = BotState.SEEK_AND_DESTROY;
                }
            }
        }

        /// <summary>
        /// Gets or sets the current micro state.
        /// </summary>
        public MicroBotState MicroState { get; set; }

        /// <summary>
        /// Gets or sets the current game mode.
        /// </summary>
        public GameMode GameMode
        {
            get
            {
                return mode;
            }

            set
            {
                mode = value;
                refreshSpawnPointList = true;

                // Seems redundant, but it forces it to check the state to make sure
                // it's compatible with the current game mode.
                State = state;
            }
        }

        /// <summary>
        /// Gets or sets the current map.
        /// </summary>
        public AIFramework.Bot.Game.Map CurrentMap
        {
            get
            {
                return map;
            }

            set
            {
                map = value;
                refreshSpawnPointList = true;
            }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Sets the default behavior of the bot to seek and destroy.
        /// </summary>
        public BotStateMachine()
        {
            State = BotState.SEEK_AND_DESTROY;
            MicroState = MicroBotState.STILL;
            GameMode = GameMode.DEATHMATCH;
            spawnPointList = new List<TileNode>();
            refreshSpawnPointList = true;
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Get a new list of spawn points.
        /// </summary>
        private void RefreshSpawnPointList()
        {
            bool ctf = GameMode == VTankObject.GameMode.CAPTURETHEFLAG;
            bool ctb = GameMode == VTankObject.GameMode.CAPTURETHEBASE;
            int halfTile = Tile.TILE_SIZE_IN_PIXELS / 2;

            spawnPointList.Clear();
            AIFramework.Bot.Game.Map map = CurrentMap;
            for (int y = 0; y < map.Height; ++y)
            {
                for (int x = 0; x < map.Width; ++x)
                {
                    AIFramework.Bot.Game.Tile tile = map.GetTile(x, y);

                    if (GameMode == VTankObject.GameMode.DEATHMATCH)
                    {
                        if (tile.EventID == 1) // '1' represents a spawn point.
                        {
                            spawnPointList.Add(new TileNode(
                                x * Tile.TILE_SIZE_IN_PIXELS - halfTile,
                                y * Tile.TILE_SIZE_IN_PIXELS - halfTile));
                        }
                    }
                    else if (ctf)
                    {
                        if (tile.EventID == 4 || tile.EventID == 5)
                        {
                            spawnPointList.Add(new TileNode(
                                x * Tile.TILE_SIZE_IN_PIXELS,
                                y * Tile.TILE_SIZE_IN_PIXELS));
                        }
                    }
                    else if (ctb)
                    {
                        if (tile.EventID >= 8 && tile.EventID < 14)
                        {
                            spawnPointList.Add(new TileNode(
                               x * Tile.TILE_SIZE_IN_PIXELS,
                               y * Tile.TILE_SIZE_IN_PIXELS));
                        }
                    }
                    else /*if (GameMode == VTankObject.GameMode.TEAMDEATHMATCH)*/
                    {
                        if (tile.EventID == 3 || tile.EventID == 2)
                        {
                            spawnPointList.Add(new TileNode(
                                x * Tile.TILE_SIZE_IN_PIXELS - halfTile,
                                y * Tile.TILE_SIZE_IN_PIXELS - halfTile));
                        }
                    }
                }
            }

            foreach (TileNode node in spawnPointList)
            {
                int tileX = (int)Math.Round((double)node.X / Tile.TILE_SIZE_IN_PIXELS);
                int tileY = (int)Math.Round((double)node.Y / Tile.TILE_SIZE_IN_PIXELS);

                Tile tile = map.GetTile(tileX, tileY);
                if (!tile.IsPassable)
                {
                    Console.Error.WriteLine("[WARNING] ({0}, {1}) => ({2}, {3}) is unpassable.",
                        node.X, node.Y, tileX, tileY);
                }
            }

            refreshSpawnPointList = false;
            spawnPointIndex = 0;

            Shuffle<TileNode>(spawnPointList);
        }

        /// <summary>
        /// Randomly shuffle the contents of the given list.
        /// </summary>
        /// <typeparam name="T">Type of list to shuffle.</typeparam>
        /// <param name="list">List to shuffle.</param>
        public void Shuffle<T>(IList<T> list)
        {
            Random rng = new Random();
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        /// <summary>
        /// Get a path for seek and destroy.
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        private Path GetSeekAndDestroyPath(Player player)
        {
            TileNode node = spawnPointList[spawnPointIndex++];
            Path path = PathFinder.FindPath(map, player, node.X, node.Y);
            return path;
        }

        /// <summary>
        /// Calculate the distance between two targets.
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        private double GetDistance(VTankObject.Point p1, VTankObject.Point p2)
        {
            return Math.Sqrt(Math.Pow(p2.x - p1.x, 2) + Math.Pow(p2.y - p1.y, 2));
        }
        #endregion

        #region Methods
        /// <summary>
        /// Get a path ideal to the current state and current game mode.
        /// </summary>
        /// <returns></returns>
        public Path GetIdealPath(VTankBot bot, Player player)
        {
            if (refreshSpawnPointList)
                RefreshSpawnPointList();

            Path path = null;
            if (State == BotState.SEEK_AND_DESTROY)
            {
                path = GetSeekAndDestroyPath(player);
            }
            else if (State == BotState.COMPLETE_OBJECTIVE)
            {
                GameModeHandler handler = bot.Game.GameModeHandler;
                if (handler is CaptureTheBaseMode)
                {
                    const int RANGE_MINIMUM = 50;
                    CaptureTheBaseMode mode = (CaptureTheBaseMode)handler;
                    Base targetBase = mode.GetAttackableBase(player);
                    if (targetBase != null)
                    {
                        Point p1 = player.GetPosition();
                        Point p2 = targetBase.GetPosition();
                        double distance = GetDistance(p1, p2);

                        if (distance > RANGE_MINIMUM)
                        {
                            int tilePositionX = (int)Math.Round((p2.x));
                            int tilePositionY = (int)Math.Round(-(p2.y));
                            TileNode node = new TileNode(tilePositionX, tilePositionY);
                            path = PathFinder.FindPath(CurrentMap, player, node.X, node.Y);
                        }
                    }
                }
                // TODO: CTF.
                else
                {
                    path = GetSeekAndDestroyPath(player);
                }
            }

            if (spawnPointIndex >= spawnPointList.Count)
                spawnPointIndex = 0;

            return path;
        }
        #endregion
    }
}
