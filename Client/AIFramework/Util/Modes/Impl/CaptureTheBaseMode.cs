namespace AIFramework.Util.Modes.Impl
{
    using System;
    using VTankObject;
    using System.Collections.Generic;

    /// <summary>
    /// Handles Capture the Base mode.
    /// </summary>
    public class CaptureTheBaseMode : GameModeHandler
    {
        public static readonly int BASE_COUNT = 6;
        private Dictionary<int, Base> bases;
        private int redAttackableBaseID = -1;
        private int blueAttackableBaseID = -2;

        #region Constructor
        public CaptureTheBaseMode(AIFramework.Bot.Game.Map currentMap)
            : base(currentMap, GameMode.CAPTURETHEBASE)
        {
            bases = new Dictionary<int, Base>();
            GenerateBases();

            Console.WriteLine("[DEBUG] Capture the base mode initialized.");
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Generate bases and their positions.
        /// </summary>
        private void GenerateBases()
        {
            bases.Clear();

            int tileSize = AIFramework.Bot.Game.Tile.TILE_SIZE_IN_PIXELS;
            int mapWidth = (int)CurrentMap.Width;
            int mapHeight = (int)CurrentMap.Height;
            const int EVENT_ID_MINIMUM = 8;
            const int EVENT_ID_MAXIMUM = 13;

            int basesFound = 0;
            for (int y = 0; y < mapHeight; ++y)
            {
                for (int x = 0; x < mapWidth; ++x)
                {
                    AIFramework.Bot.Game.Tile tile = CurrentMap.GetTile(x, y);
                    if (tile.EventID >= EVENT_ID_MINIMUM && tile.EventID <= EVENT_ID_MAXIMUM)
                    {
                        // Found base tile.
                        ++basesFound;
                        Point position = new Point(x * tileSize + (tileSize / 2), -(y * tileSize + (tileSize / 2)));
                        GameSession.Alliance team = tile.EventID > 10 ? GameSession.Alliance.RED : GameSession.Alliance.BLUE;

                        Base nextBase = new Base(tile.EventID, position, team);
                        bases[nextBase.BaseID] = nextBase;
                    }

                    if (basesFound == BASE_COUNT)
                        goto done;
                }
            }
done:

            if (basesFound != BASE_COUNT)
            {
                Console.Error.WriteLine("[WARNING] Bases found = {0}", basesFound);
            }

            DetectAttackableBases();
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Get a base by it's (zero-based) ID number.
        /// If the given number is not zero-based, the offset is calculated automatically.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Base GetBaseByID(int id)
        {
            const int OFFSET = 8;
            if (id > BASE_COUNT)
            {
                id = id - OFFSET;
            }

            return bases[id];
        }

        /// <summary>
        /// Get a list of bases, sorted in order of base ID.
        /// </summary>
        /// <returns></returns>
        public List<Base> GetBases()
        {
            List<Base> baseList = new List<Base>(bases.Values);
            baseList.Sort((b1, b2) => {
                return b1.BaseID.CompareTo(b2.BaseID);
            });
            return baseList;
        }

        /// <summary>
        /// See if the given player can attack the given base.
        /// </summary>
        /// <param name="b"></param>
        /// <param name="player"></param>
        /// <returns></returns>
        public bool IsBaseAttackable(Base b, AIFramework.Bot.Game.Player player)
        {
            if (b.Team != player.Team)
            {
                bool inContention = false;
                int id = b.BaseID;
                if (player.Team == GameSession.Alliance.RED && id == blueAttackableBaseID)
                    inContention = true;
                else if (player.Team == GameSession.Alliance.BLUE && id == redAttackableBaseID)
                    inContention = true;

                return inContention;
            }

            return false;
        }

        /// <summary>
        /// Attempts to detect which bases can be attacked.
        /// </summary>
        public void DetectAttackableBases()
        {
            const int BLUE_START    = 0;
            const int RED_END       = 5;

            int switchesAt = -1;
            for (int i = BLUE_START; i < RED_END + 1; ++i)
            {
                if (bases[i].Team == GameSession.Alliance.RED)
                {
                    switchesAt = i;
                    break;
                }
            }

            blueAttackableBaseID = switchesAt - 1;
            redAttackableBaseID = switchesAt;
        }

        /// <summary>
        /// Get the next attackable base for the given player.
        /// </summary>
        /// <param name="forPlayer"></param>
        /// <returns></returns>
        public Base GetAttackableBase(AIFramework.Bot.Game.Player forPlayer)
        {
            try
            {
                if (forPlayer.Team == GameSession.Alliance.RED)
                    return bases[blueAttackableBaseID];

                return bases[redAttackableBaseID];
            }
            catch (KeyNotFoundException)
            {
                return null;
            }
        }

        /// <summary>
        /// Reset all bases to their original state.
        /// This method also calls DetectAttackableBases().
        /// </summary>
        public void ResetBases()
        {
            for (int i = 0; i < bases.Count; ++i)
            {
                Base b = bases[i];
                b.Reset();
            }

            DetectAttackableBases();
        }
        #endregion

        #region IGameModeHandler Members

        /// <summary>
        /// Performs small maintenance updates.
        /// </summary>
        /// <param name="game"></param>
        /// <param name="deltaTimeSeconds"></param>
        public override void Update(GameTracker game, double deltaTimeSeconds)
        {
            
        }

        /// <summary>
        /// Disposes of all references to this base mode.
        /// </summary>
        public override void Dispose()
        {
            bases.Clear();

            base.Dispose();
        }

        #endregion
    }

    /// <summary>
    /// Represents an in-game base for Capture the Base mode.
    /// </summary>
    public class Base : ITarget
    {
        #region Members
        public static readonly int DEFAULT_BASE_HEALTH = 600;
        private int health;
        private Point position;
        #endregion

        #region Properties
        /// <summary>
        /// Gets the base ID.
        /// </summary>
        public int BaseID
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the base's team.
        /// </summary>
        public GameSession.Alliance Team
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the base's health.
        /// </summary>
        public int Health
        {
            get { return health; }
            set { health = value; }
        }
        #endregion

        #region Constructor

        public Base(int baseId, Point _position, GameSession.Alliance _team)
        {
            const int OFFSET = 8; // Offset the base ID for zero-based indexing.
            BaseID = baseId - OFFSET;
            health = DEFAULT_BASE_HEALTH;
            position = _position;
            Team = _team;
        }

        #endregion

        #region ITarget Members

        /// <summary>
        /// Checks whether the base is alive.
        /// </summary>
        /// <returns></returns>
        public bool IsAlive()
        {
            return health > 0;
        }

        /// <summary>
        /// Checks the base's health.
        /// </summary>
        /// <returns></returns>
        public int GetHealth()
        {
            return health;
        }

        /// <summary>
        /// Inflicts damage to the base.
        /// </summary>
        /// <param name="health">Damage to inflict.</param>
        /// <param name="killingBlow">Whether the blow killed the base.</param>
        /// <returns>True if the base is dead; false otherwise.</returns>
        public bool InflictDamage(int health, bool killingBlow)
        {
            this.health -= health;
            if (this.health < 0)
            {
                this.health = 0;
            }

            if (killingBlow)
            {
                this.health = 0;
            }
            else if (!killingBlow && this.health == 0)
            {
                this.health = 1;
            }

            return IsAlive();
        }

        /// <summary>
        /// Do not call this method -- it will throw an exception. Bases may not be moved.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void SetPosition(double x, double y)
        {
            throw new Exception("Base positions cannot be modified.");
        }

        /// <summary>
        /// Gets the position of the base.
        /// </summary>
        /// <returns></returns>
        public Point GetPosition()
        {
            return position;
        }

        /// <summary>
        /// Get the team that this base belongs to.
        /// </summary>
        /// <returns></returns>
        public GameSession.Alliance GetTeam()
        {
            return Team;
        }

        #endregion

        #region Methods
        /// <summary>
        /// Resets the base to it's original state.
        /// </summary>
        public void Reset()
        {
            Team = BaseID < 3 ? GameSession.Alliance.BLUE : GameSession.Alliance.RED;
            health = DEFAULT_BASE_HEALTH;
        }
        #endregion
    }
}
