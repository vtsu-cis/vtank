using System;
using System.Collections.Generic;
using System.Text;
using AIFramework.Bot.Game;
using AIFramework.Bot;
using AIFramework.Bot.EventArgs;
using AIFramework.Util.Modes;

namespace AIFramework.Util
{
    /// <summary>
    /// Tracks statistics about each game in one central location. 
    /// </summary>
    public class GameTracker
    {
        #region Members
        public static readonly double VELOCITY = 275.0;
        public static readonly double ANGULAR_VELOCITY = 2.666666667f;

        private Dictionary<int, MetaPlayer> players;
        private Dictionary<int, MetaProjectile> projectiles;
        private VTankBot bot;
        private long lastTimestamp = 0L;
        private Map currentMap;
        private bool needsSync = false;
        private double syncTimer;
        private static readonly double SYNC_TIMER_MAX = 2.0;
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the algorithm used for pathfinding.
        /// </summary>
        public PathAlgorithms.IPathfindingAlgorithm PathfindingAlgorithm
        {
            get
            {

                return PathFinder.Algorithm;
            }

            set
            {
                PathFinder.Algorithm = value;
            }
        }

        /// <summary>
        /// Gets the local player (you) in this game.
        /// </summary>
        public Player LocalPlayer
        {
            get
            {
                foreach (MetaPlayer player in players.Values)
                {
                    if (player.IsLocalPlayer)
                    {
                        return player.Player;
                    }
                }

                return null;
            }
        }

        public bool NeedsSync
        {
            get
            {
                lock (this)
                {
                    return needsSync;
                }
            }

            set
            {
                lock (this)
                {
                    needsSync = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the current map being played in a thread-safe way.
        /// </summary>
        public Map CurrentMap
        {
            get
            {
                return currentMap;
            }

            set
            {
                currentMap = value;
                StopMoving();
            }
        }

        /// <summary>
        /// Gets the game mode handler for this game.
        /// </summary>
        public GameModeHandler GameModeHandler
        {
            get;
            internal set;
        }
        #endregion

        #region Constructor
        /// <summary>
        /// 
        /// </summary>
        /// <param name="parentBot"></param>
        public GameTracker(VTankBot parentBot)
        {
            players = new Dictionary<int, MetaPlayer>();
            projectiles = new Dictionary<int, MetaProjectile>();
            bot = parentBot;
            PathfindingAlgorithm = new PathAlgorithms.AStarAlgorithm();
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Handle a regular update occurrence.
        /// </summary>
        public void Update()
        {
            long currentTimestamp = Network.Util.Clock.GetTimeMilliseconds();
            double deltaTimeSeconds = (currentTimestamp - lastTimestamp) / 1000.0;
            lastTimestamp = currentTimestamp;
            syncTimer += deltaTimeSeconds;

            // Process local player first.
            if (LocalPlayer == null)
            {
                // Game not initialized yet. Try again later.
                return;
            }

            if (syncTimer > SYNC_TIMER_MAX)
            {
                syncTimer = 0;
                bot.GameServer.Move(LocalPlayer.Position.x, LocalPlayer.Position.y,
                    LocalPlayer.MovementDirection);
                //bot.GameServer.Rotate(LocalPlayer.Angle, LocalPlayer.RotationDirection);
            }

            foreach (MetaPlayer player in players.Values)
            {
                MovePlayer(player, deltaTimeSeconds);
                RotatePlayer(player, deltaTimeSeconds);
            }

            DoRangeCheck();

            if (GameModeHandler != null)
                GameModeHandler.Update(this, deltaTimeSeconds);
        }

        /// <summary>
        /// Gets a list of players.
        /// </summary>
        /// <returns></returns>
        public List<Player> GetPlayerList()
        {
            List<Player> playerList = new List<Player>();
            foreach (MetaPlayer p in players.Values)
            {
                playerList.Add(p.Player);
            }

            return playerList;
        }

        /// <summary>
        /// Gets a list of players who want to kill the local player.
        /// </summary>
        /// <returns></returns>
        public List<Player> GetEnemyList()
        {
            if (LocalPlayer.Team == GameSession.Alliance.NONE)
            {
                // There are no teams in this match: all players are enemies.
                List<Player> result = GetPlayerList();
                result.Remove(LocalPlayer);
                return result;
            }

            List<Player> enemyList = new List<Player>();
            foreach (MetaPlayer p in players.Values)
            {
                if (p.Player == LocalPlayer)
                    continue;

                if (p.Player.Team != LocalPlayer.Team)
                {
                    enemyList.Add(p.Player);
                }
            }

            return enemyList;
        }

        /// <summary>
        /// Gets a list of allies.
        /// </summary>
        /// <returns></returns>
        public List<Player> GetAllyList()
        {
            if (LocalPlayer.Team == GameSession.Alliance.NONE)
            {
                // There are no teams in this match: return empty list.
                return new List<Player>();
            }

            List<Player> allyList = new List<Player>();
            foreach (MetaPlayer p in players.Values)
            {
                if (p.Player == LocalPlayer)
                    continue;

                if (p.Player.Team == LocalPlayer.Team)
                {
                    allyList.Add(p.Player);
                }
            }

            return allyList;
        }

        /// <summary>
        /// Check and update everyone's range variables.
        /// </summary>
        /// <returns></returns>
        private void DoRangeCheck()
        {
            foreach (MetaPlayer player in players.Values)
            {
                bool inRange = player.Player.IsInRangeOf(LocalPlayer);
                if (inRange && !player.InRange && player.Player.Alive)
                {
                    player.InRange = true;
                }
                else if (!inRange && player.InRange)
                {
                    player.InRange = false;
                }

                if (!player.Player.Alive && player.InRange)
                {
                    player.InRange = false;
                }
            }
        }

        /// <summary>
        /// Check if a player is in range of another player.
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public bool IsInRange(Player player)
        {
            MetaPlayer testPlayer = players[player.ID];
            return testPlayer.Player.IsInRangeOf(player);
        }

        /// <summary>
        /// Set a player's movement data (who isn't the bot itself). This is done in a
        /// thread-safe way.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="position"></param>
        /// <param name="direction"></param>
        public void SetPlayerMovement(Player player, VTankObject.Point position,
            VTankObject.Direction direction)
        {
            player.SetPosition(position);
            player.MovementDirection = direction;
        }

        /// <summary>
        /// Set a player's rotation data (who isn't the bot itself). This is done in a
        /// thread-safe way.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="angle"></param>
        /// <param name="direction"></param>
        public void SetPlayerRotation(Player player, double angle,
            VTankObject.Direction direction)
        {
            player.Angle = angle;
            player.RotationDirection = direction;
        }

        /// <summary>
        /// Interrupt a movement, if one is in progress.
        /// </summary>
        public void StopMoving()
        {
            if (LocalPlayer == null)
            {
                return;
            }

            MetaPlayer player = players[LocalPlayer.ID];
            player.UnsetGoalTile();

            player.SecondsUntilAngle = 0;
            player.CurrentNode = null;
            player.Paused = false;
            player.Player.MovementDirection = VTankObject.Direction.NONE;
            player.Player.RotationDirection = VTankObject.Direction.NONE;

            bot.GameServer.Move(player.Player.Position.x, player.Player.Position.y, VTankObject.Direction.NONE);
            bot.GameServer.Rotate(player.Player.Angle, VTankObject.Direction.NONE);
        }

        /// <summary>
        /// Ask the game tracker to move a player to a certain position.
        /// </summary>
        /// <param name="player">Player to move.</param>
        /// <param name="x">X-position to move to.</param>
        /// <param name="y">Y-position to move to.</param>
        public void StartMoving(VTankObject.Direction direction)
        {
            MetaPlayer movingPlayer = players[LocalPlayer.ID];

            SetPlayerMovement(LocalPlayer, LocalPlayer.Position, direction);
            bot.GameServer.Move(movingPlayer.Player.Position.x, movingPlayer.Player.Position.y,
                direction);
        }

        /// <summary>
        /// Rotate [player] until he is facing [angleInRadians].
        /// </summary>
        /// <param name="player">Player to rotate.</param>
        /// <param name="angleInRadians">Angle to rotate to. This is not a theta value.</param>
        public void RotateTo(Player player, double angleInRadians)
        {
            MetaPlayer currentPlayer = players[player.ID];
            currentPlayer.GoalAngle = (angleInRadians) % (Math.PI * 2.0f);

            double currentAngle = player.Angle;

            double distance = 0;
            VTankObject.Direction result = ShortestDistance(currentAngle, angleInRadians, out distance);
            if (result == VTankObject.Direction.RIGHT)
            {
                player.RotationDirection = VTankObject.Direction.RIGHT;
            }
            else
            {
                player.RotationDirection = VTankObject.Direction.LEFT;
            }

            // Time = distance/velocity
            double timeInSeconds = Math.Abs(
                distance / (ANGULAR_VELOCITY * player.SpeedFactor));
            currentPlayer.SecondsUntilAngle = timeInSeconds;

            bot.GameServer.Rotate(currentAngle, player.RotationDirection);
        }

        /// <summary>
        /// Add a player to the game. Does nothing if the player was already added.
        /// </summary>
        /// <param name="player">Player to add.</param>
        /// <param name="isLocalPlayer">True if the player is the bot.</param>
        public void AddPlayer(Player player, bool isLocalPlayer)
        {
            if (players.ContainsKey(player.ID))
            {
                // Do not add duplicates.
                return;
            }

            players[player.ID] = new MetaPlayer(player)
            {
                GoalAngle = player.Angle,
                IsLocalPlayer = isLocalPlayer
            };
        }

        /// <summary>
        /// Adds a projectile to the game. The projectile is automatically removed from
        /// the game later.
        /// </summary>
        /// <param name="projectile"></param>
        public void AddProjectile(Projectile projectile)
        {
            if (projectiles.ContainsKey(projectile.ID))
            {
                // Do not add duplicates.
                return;
            }

            //projectiles[projectile.ID] = new MetaProjectile(projectile);
            // We currently do not track projectiles.
        }

        /// <summary>
        /// Pop the player from the list of players.
        /// </summary>
        /// <param name="playerID">ID of the player to remove.</param>
        /// <returns>The removed player, or null if he didn't exist.</returns>
        public Player RemovePlayer(int playerID)
        {
            Player poppedPlayer = null;
            if (players.ContainsKey(playerID))
            {
                poppedPlayer = players[playerID].Player;
                players.Remove(playerID);
            }

            return poppedPlayer;
        }
        
        /// <summary>
        /// Access a player by his player ID.
        /// </summary>
        /// <param name="playerID">ID of the player.</param>
        /// <returns>The relevant player, or null if he doesn't exist.</returns>
        public Player GetPlayerByID(int playerID)
        {
            if (players.ContainsKey(playerID))
            {
                return players[playerID].Player;
            }

            return null;
        }

        /// <summary>
        /// Reset the game, clearing all slates.
        /// </summary>
        public void Reset()
        {
            players.Clear();
            projectiles.Clear();
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Adjust a player's position according to his movement/rotation.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="deltaTimeSeconds"></param>
        private void MovePlayer(MetaPlayer player, double deltaTimeSeconds)
        {
            Player info = player.Player;
            if (info.MovementDirection != VTankObject.Direction.NONE)
            {
                double speed = (VELOCITY * info.SpeedFactor) * deltaTimeSeconds;
                if (info.MovementDirection == VTankObject.Direction.REVERSE)
                {
                    speed = -speed;
                }

                VTankObject.Point newPosition = new VTankObject.Point(
                    info.Position.x + Math.Cos(info.Angle) * speed,
                    info.Position.y + Math.Sin(info.Angle) * speed
                );

                if (!CheckCollision(newPosition))
                {
                    info.SetPosition(newPosition);
                }
                else
                {
                    info.MovementDirection = VTankObject.Direction.NONE;
                    if (info == LocalPlayer)
                    {
                        bot.GameServer.Move(info.Position.x, info.Position.y, VTankObject.Direction.NONE);
                        bot.InvokeOnHitWall();
                    }
                }
            }
        }
        
        /// <summary>
        /// Rotate a player.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="deltaTimeSeconds"></param>
        private void RotatePlayer(MetaPlayer player, double deltaTimeSeconds)
        {
            Player info = player.Player;
            if (info.RotationDirection != VTankObject.Direction.NONE)
            {
                double speed = (ANGULAR_VELOCITY * info.SpeedFactor) * deltaTimeSeconds;
                if (info.RotationDirection == VTankObject.Direction.RIGHT)
                {
                    speed = -speed;
                }

                double angle = (info.Angle * speed) % (Math.PI * 2);
                info.Angle = angle;

                if (player.Player == LocalPlayer)
                {
                    player.SecondsUntilAngle -= deltaTimeSeconds;
                    if (player.SecondsUntilAngle <= 0)
                    {
                        info.Angle = player.GoalAngle;
                        player.GoalAngle = 0;
                        player.HasGoal = false;
                        info.RotationDirection = VTankObject.Direction.NONE;
                        bot.GameServer.Rotate(info.Angle, VTankObject.Direction.NONE);
                        bot.InvokeOnCompletedRotation();
                    }
                }
            }
        }

        /// <summary>
        /// Calculates whether it's shortest to rotate towards the left, or towards
        /// the right, to the target angle.
        /// </summary>
        /// <param name="current">Current angle.</param>
        /// <param name="target">Target angle.</param>
        /// <returns>Returns one of the following values:
        /// - VTankObject.Direction.LEFT
        /// - VTankObject.Direction.RIGHT
        /// </returns>
        private VTankObject.Direction ShortestDistance(double current, double target, out double distance)
        {
            const double MAX_ANGLE = Math.PI * 2.0;
            double rightDistance = target - current;
            if (target > current)
            {
                rightDistance = (MAX_ANGLE - target) + current;
            }

            double leftDistance = target - current;
            if (target < current)
            {
                leftDistance = (MAX_ANGLE - current) + target;
            }

            if (leftDistance > rightDistance)
            {
                distance = rightDistance;
                return VTankObject.Direction.RIGHT; // right
            }

            distance = leftDistance;
            return VTankObject.Direction.LEFT; // left
        }

        /// <summary>
        /// Perform a movement on the player if that player wants to move.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="deltaTimeSeconds"></param>
        private bool CheckCollision(VTankObject.Point position)
        {
            return false;
            /*const float TANK_RADIUS = 25f;
            
            int tileX = (int)Math.Round(position.x / Tile.TILE_SIZE_IN_PIXELS);
            int tileY = (int)Math.Round((-position.y) / Tile.TILE_SIZE_IN_PIXELS);

            Circle circle = new Circle(position.x, position.y, TANK_RADIUS);

            for (int y = tileY - 2; y < tileY + 2 && y < CurrentMap.Height; ++y)
            {
                if (y < 0) continue;
                for (int x = tileX - 2; x < tileX + 2 && x < CurrentMap.Width; ++x)
                {
                    if (x < 0) continue;

                    Tile currentTile = CurrentMap.GetTile(x, y);
                    Rectangle tileRect = new Rectangle(x * Tile.TILE_SIZE_IN_PIXELS,
                        -(y * Tile.TILE_SIZE_IN_PIXELS + 64),
                        Tile.TILE_SIZE_IN_PIXELS, Tile.TILE_SIZE_IN_PIXELS);
                    if (!currentTile.IsPassable && circle.CollidesWith(tileRect))
                    {
                        return true;
                    }
                }
            }

            return false;*/
        }

        #region Helpers
        /// <summary>
        /// Start moving forward.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="pos"></param>
        private void StartMovingForward(MetaPlayer player, VTankObject.Point pos)
        {
            player.Player.MovementDirection = VTankObject.Direction.FORWARD;
            player.Player.Position = pos;

            bot.GameServer.Move(pos.x, pos.y, VTankObject.Direction.FORWARD);
        }

        /// <summary>
        /// Stop moving forward.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="pos"></param>
        private void StopMovingForward(MetaPlayer player, VTankObject.Point pos)
        {
            player.Player.Position = pos;

            //if (player.Player.MovementDirection != VTankObject.Direction.NONE)
            //{
                player.Player.MovementDirection = VTankObject.Direction.NONE;

                bot.GameServer.Move(pos.x, pos.y, VTankObject.Direction.NONE);
            //}
        }

        /// <summary>
        /// Get the next position.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="currentNode"></param>
        /// <returns></returns>
        private VTankObject.Point SetPosition(MetaPlayer player, Node currentNode)
        {
            double halfTile = Tile.TILE_SIZE_IN_PIXELS / 2;
            VTankObject.Point newPosition = new VTankObject.Point(
                (currentNode.X * Tile.TILE_SIZE_IN_PIXELS) + halfTile,
                -((currentNode.Y * Tile.TILE_SIZE_IN_PIXELS + halfTile)));
            player.Player.Position = newPosition;

            return newPosition;
        }
        #endregion

        /// <summary>
        /// Calculates how long it will take for the player to reach the target node.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="node"></param>
        /// <returns></returns>
        private static double SecondsUntilNextNode(Player player, Node current, Node next)
        {
            int halfTile = Tile.TILE_SIZE_IN_PIXELS / 2;

            int x1 = (current.X * Tile.TILE_SIZE_IN_PIXELS) + halfTile;
            int x2 = (next.X    * Tile.TILE_SIZE_IN_PIXELS) + halfTile;
            int y1 = (current.Y * Tile.TILE_SIZE_IN_PIXELS) - halfTile;
            int y2 = (next.Y    * Tile.TILE_SIZE_IN_PIXELS) - halfTile;

            // Find distance to next point.
            double distance = Math.Sqrt(Math.Pow(y1 - y2, 2) + Math.Pow(x1 - x2, 2));

            // T = D / V
            double seconds = distance / (VELOCITY * player.SpeedFactor);
            return seconds;
        }

        /// <summary>
        /// Calculates the angle the player needs to face in order to get to the next node.
        /// </summary>
        /// <param name="playerX">X position of the player in terms of tiles.</param>
        /// <param name="playerY">Y position of the player in terms of tiles.</param>
        /// <param name="next">Next node to travel to.</param>
        /// <returns>The desired angle.</returns>
        private static double GetNextTargetAngle(int playerX, int playerY, Node next)
        {
            return Math.Atan2(next.Y - playerY, next.X - playerX) % (Math.PI * 2);
        }

        /// <summary>
        /// Test if two angles are approximately equal.
        /// </summary>
        /// <param name="a1">Angle 1.</param>
        /// <param name="a2">Angle 2.</param>
        /// <returns>True if the two angles are approximately equal.</returns>
        private static bool DoneRotating(MetaPlayer player, double deltaTimeInSeconds)
        {
            player.SecondsUntilAngle -= deltaTimeInSeconds;
            if (player.SecondsUntilAngle <= 0)
            {
                player.SecondsUntilAngle = 0;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Check if the bot is finished moving.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="deltaTimeInSeconds"></param>
        /// <returns></returns>
        private static bool DoneMoving(MetaPlayer player, double deltaTimeInSeconds)
        {
            player.SecondsUntilGoal -= deltaTimeInSeconds;
            if (player.SecondsUntilGoal <= 0)
            {
                player.SecondsUntilGoal = 0;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Test if two angles are approximately equal.
        /// </summary>
        /// <param name="a1"></param>
        /// <param name="a2"></param>
        /// <returns></returns>
        private static bool ApproximatelyEqual(double a1, double a2)
        {
            double MAX_ANGLE = (360.0 * Math.PI) / 180.0;
            double MAX_DIFFERENCE = 0.12;

            if ((a1 < MAX_DIFFERENCE / 2 && a2 > MAX_ANGLE - (MAX_DIFFERENCE / 2)) ||
                (a2 < MAX_DIFFERENCE / 2 && a1 > MAX_ANGLE - (MAX_DIFFERENCE / 2))) 
            {
                return true;
            }

            double result = Math.Abs(a1 - a2);
            if (result < MAX_DIFFERENCE)
            {
                return true;
            }

            return false;
        }
        #endregion
    }

    #region struct MetaPlayer
    class MetaPlayer
    {
        public Player Player;
        public bool HasGoal;
        public bool Paused;
        public double GoalAngle;
        public double SecondsUntilAngle;
        public Node CurrentNode;
        public int GoalTileX;
        public int GoalTileY;
        public double SecondsUntilGoal;
        public bool IsLocalPlayer;
        public bool InRange;

        public MetaPlayer(Player player)
        {
            Player = player;
            GoalTileX = -1;
            GoalTileY = -1;
            SecondsUntilAngle = 0;
            SecondsUntilGoal = 0;
            InRange = false;
        }

        public void SetGoalTile(int x, int y)
        {
            GoalTileX = x;
            GoalTileY = y;

            HasGoal = true;
        }

        public void UnsetGoalTile()
        {
            GoalTileX = -1;
            GoalTileY = -1;
            HasGoal = false;
            SecondsUntilGoal = 0;
        }
    }
    #endregion

    #region struct MetaProjectile
    struct MetaProjectile
    {
        public Projectile Projectile
        {
            get;
            set;
        }

        public long MillisecondsUntilExpiration
        {
            get;
            private set;
        }

        public long MillisecondsSoFar
        {
            get;
            private set;
        }

        public VTankObject.Point Position
        {
            get;
            private set;
        }

        public MetaProjectile(Projectile projectile)
            : this()
        {
            Projectile = projectile;

            MillisecondsUntilExpiration = (projectile.Data.Range / Projectile.Data.TerminalVelocity)*1000;
            MillisecondsSoFar = 0;
            Position = projectile.StartingPosition;
        }

        public void Update(double deltaTimeSeconds)
        {
            MillisecondsUntilExpiration -= (long)(deltaTimeSeconds * 1000);
        }
    }
    #endregion
}
