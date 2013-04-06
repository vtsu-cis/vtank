using System;
using AIFramework.Bot;
using AIFramework.Runner;
using AIFramework.Util;
using System.Collections.Generic;
using AIFramework.Bot.Game;
using AIFramework.Bot.EventArgs;
using AIFramework.Util.Modes;
using AIFramework.Util.Modes.Impl;
using System.Threading;

namespace VTankBotRunner.SampleBot
{
    public class SampleBot : VTankBot
    {
        #region Members
        private static readonly double VELOCITY = 275.0;
        private List<TileNode> spawnPointList = new List<TileNode>();
        private ITarget currentTarget = null;
        private bool alive = true;
        private double cooldownPeriod = 0;
        private double cooldownElapsed = 0;
        private long lastStamp;
        private BotStateMachine stateMachine;
        private Path currentPath;
        private Node targetNode;
        private Node lastNode;
        private int pathIndex;
        private double timeToNextPoint;
        private double elapsed;
        private bool refreshStateMachine;

        private int winIndex = 0;
        private static readonly string[] winMessages = new string[]
        {
            "Why are you so bad at video games?",
            "That was my Grandma playing.",
            "Are you OK?",
            "Get up, you wuss.",
            "Go back to playing Solitaire.",
            "Good fight! Just kidding, you're terrible.",
            "My AI programming is awful, and you're still losing to me. How sad.",
            "You should try Bejeweled instead.",
            "hahahahaha",
        };

        private int loseIndex = 0;
        private static readonly string[] loseMessages = new string[]
        {
            ":(",
            "Why are you picking on me?",
            "Sure, go for the easy target.",
            "You got lucky.",
            "THIS GAME IS SO INBALANCED.",
            "this sucks",
            "Will you stop killing me?",
            "You're lucky my creator sucks at AI programming.",
            "RIDICULOUS.",
            "Absurd."
        };
        #endregion

        #region Constructor
        public SampleBot(BotRunner runner, TargetServer server, AuthInfo auth)
            : base(runner, server, auth)
        {
            lastStamp = Network.Util.Clock.GetTimeMilliseconds();
            stateMachine = new BotStateMachine();
            refreshStateMachine = true;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Print something to the console.
        /// </summary>
        /// <param name="format"></param>
        /// <param name="parameters"></param>
        public void DebugPrint(string format, params object[] parameters)
        {
#if DEBUG
            if (Game.LocalPlayer.Name.EndsWith("1"))
            {
                string line = String.Format("[{0}] {1}", DateTime.Now.ToShortTimeString(),
                    String.Format(format, parameters));
                Console.WriteLine(line);
            }
#endif
        }

        /// <summary>
        /// Refresh the state machine with the latest updates.
        /// </summary>
        private void RefreshStateMachine()
        {
            stateMachine.CurrentMap = Game.CurrentMap;
            stateMachine.GameMode = GameMode;
            stateMachine.State = BotState.COMPLETE_OBJECTIVE;
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

        /// <summary>
        /// Select a target according to an internal priority.
        /// </summary>
        /// <returns></returns>
        private ITarget UpdateTarget()
        {
            List<Player> enemies = Game.GetEnemyList();
            if (enemies.Count == 0)
                return null;

            // Sort by distance from my tank.
            enemies.Sort((x, y) => 
            {
                if (x.Alive && !y.Alive)
                    return -1;
                else if (!x.Alive && y.Alive)
                    return 1;
                else if (!x.Alive && !y.Alive)
                    return 0;

                return x.DistanceFromLocalPlayer.CompareTo(y.DistanceFromLocalPlayer);
            });

            List<ITarget> gameModeTargets = GetGameModeTargets();
            gameModeTargets.Sort((x, y) =>
            {
                if (x.IsAlive() && !y.IsAlive())
                    return -1;
                else if (!x.IsAlive() && y.IsAlive())
                    return 1;
                else if (!x.IsAlive() && !y.IsAlive())
                    return 0;

                VTankObject.Point playerPos = Game.LocalPlayer.GetPosition();
                return GetDistance(x.GetPosition(), playerPos).CompareTo(
                    GetDistance(y.GetPosition(), Game.LocalPlayer.GetPosition()));
            });

            Player highestPriority = enemies[0];
            if (!highestPriority.IsInRangeOf(Game.LocalPlayer) || !highestPriority.Alive)
            {
                if (gameModeTargets.Count == 0)
                    return null;

                int playerRange = Game.LocalPlayer.Weapon.Projectile.Range;
                ITarget highestPriorityTarget = gameModeTargets[0];
                double distance = GetDistance(highestPriorityTarget.GetPosition(),
                    Game.LocalPlayer.GetPosition());
                if (!highestPriorityTarget.IsAlive() || distance > playerRange)
                    return null;

                return highestPriorityTarget;
            }

            return highestPriority;
        }

        /// <summary>
        /// Get a list of targets available from the current game mode, if there are any.
        /// </summary>
        /// <returns></returns>
        private List<ITarget> GetGameModeTargets()
        {
            List<ITarget> results = new List<ITarget>();

            GameModeHandler handler = Game.GameModeHandler;
            if (handler != null && handler is CaptureTheBaseMode)
            {
                CaptureTheBaseMode mode = (CaptureTheBaseMode)handler;
                Base b = mode.GetAttackableBase(Player);
                if (b != null)
                    results.Add(b);
            }

            return results;
        }
        #endregion

        #region Overrides
        public override void Update()
        {
            if (refreshStateMachine)
            {
                refreshStateMachine = false;
                RefreshStateMachine();
            }

            // Find delta time.
            long currentStamp = Network.Util.Clock.GetTimeMilliseconds();
            double deltaTimeSeconds = (double)(currentStamp - lastStamp) / 1000.0;
            lastStamp = currentStamp;

            if (cooldownElapsed > 0)
            {
                cooldownElapsed -= deltaTimeSeconds;
                if (cooldownElapsed < 0) 
                    cooldownElapsed = 0;
            }

            currentTarget = UpdateTarget();

            if (alive && stateMachine.MicroState == MicroBotState.STILL)
            {
                try
                {
                    if (currentPath == null)
                    {
                        pathIndex = 0;
                        currentPath = stateMachine.GetIdealPath(this, Game.LocalPlayer);
                        targetNode = null;
                    }

                    if (currentPath != null && targetNode == null)
                    {
                        if (lastNode == null)
                            lastNode = GetPositionAsNode();
                        targetNode = currentPath.Pop();
                        pathIndex++;
                        if (targetNode == null)
                        {
                            // Target reached.
                            pathIndex = 0;
                            currentPath = stateMachine.GetIdealPath(this, Game.LocalPlayer);
                            targetNode = currentPath.Pop();

                            Game.StopMoving();

                            if (targetNode != null)
                            {
                                double targetAngle = GetAngleToNextNode(lastNode, targetNode);
                                Game.RotateTo(Game.LocalPlayer, targetAngle);
                                DebugPrint("Rotating {0} degrees to meet next node.", targetAngle);
                            }
                            else
                            {
                                DebugPrint("Error: target node is null because path is empty.");
                            }
                        }
                        else
                        {
                            double targetAngle = GetAngleToNextNode(lastNode, targetNode);
                            Game.RotateTo(Game.LocalPlayer, targetAngle);
                            DebugPrint("Rotating {0} degrees to meet target node at ({1}, {2}) => ({3}, {4})",
                                targetAngle * 180.0 / Math.PI, targetNode.ToPixelsX(), targetNode.ToPixelsY(), 
                                targetNode.X, targetNode.Y);
                        }

                        stateMachine.MicroState = MicroBotState.ROTATE;
                    }
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine(e);
                    RefreshPlayerList();
                    Thread.Sleep(50);
                    refreshStateMachine = true;
                }
            }
            else if (alive && stateMachine.MicroState == MicroBotState.MOVE_FORWARD 
                || stateMachine.MicroState == MicroBotState.MOVE_REVERSE)
            {
                elapsed += deltaTimeSeconds;
                if (elapsed >= timeToNextPoint)
                {
                    timeToNextPoint = 0;
                    elapsed = 0;

                    Game.StopMoving();

                    if (stateMachine.MicroState == MicroBotState.MOVE_FORWARD)
                    {
                        // Target reached.
                        Game.LocalPlayer.SetPosition(targetNode.ToPixelsX(), targetNode.ToPixelsY());
                        lastNode = targetNode;
                        targetNode = null;
                        stateMachine.MicroState = MicroBotState.STILL;
                    }
                    else
                    {
                        Node lastNode = GetPositionAsNode();
                        double angle = GetAngleToNextNode(lastNode, targetNode);
                        Game.RotateTo(Game.LocalPlayer, angle);
                        stateMachine.MicroState = MicroBotState.ROTATE;
                    }
                }
            }

            // Shoot players.
            if (alive)
            {
                Random randomModifier = new Random();
                cooldownPeriod = Player.Weapon.Cooldown;
                if (currentTarget != null && cooldownElapsed == 0)
                {
                    bool accurateShot = randomModifier.Next(0, 3) <= 1;
                    int variation = 25;
                    if (!accurateShot)
                        variation = 200;
                    double variationX = (double)randomModifier.Next(-variation, variation);
                    double variationY = (double)randomModifier.Next(-variation, variation);
                    GameServer.Fire(currentTarget.GetPosition().x + variationX, currentTarget.GetPosition().y + variationY);
                    cooldownElapsed = cooldownPeriod;
                }
            }
        }

        public override void PlayerHasRespawned(PlayerRespawnEventArgs e)
        {
            if (e.Player.ID == base.Player.ID)
            {
                alive = true;
                stateMachine.MicroState = MicroBotState.STILL;
                currentTarget = null;
                targetNode = null;
                pathIndex = 0;
                elapsed = 0;
            }
        }

        public override void OnProjectileHit(ProjectileHitEventArgs e)
        {
            if (e.Victim == Player && !Player.Alive)
            {
                // I was killed.
                if (new Random().Next(0, 5) == 0) // Send chat message rarely.
                {
                    GameServer.SendChatMessage(loseMessages[loseIndex]);

                    loseIndex++;
                    if (loseIndex >= loseMessages.Length)
                    {
                        loseIndex = 0;
                    }
                }

                alive = false;
                stateMachine.MicroState = MicroBotState.STILL;
                currentPath = null;
                targetNode = null;
            }
            else if (!e.Victim.Alive)
            {
                if (e.Victim == currentTarget)
                {
                    if (new Random().Next(0, 5) == 0)
                    {
                        GameServer.SendChatMessage(winMessages[winIndex]);
                        winIndex++;
                        if (winIndex >= winMessages.Length)
                        {
                            winIndex = 0;
                        }
                    }

                    currentTarget = null;
                }
            }
        }

        public override void OnPositionReset()
        {
            stateMachine.MicroState = MicroBotState.STILL;
            Game.StopMoving();
            targetNode = null;
            currentPath = null;
            currentTarget = null;
        }

        public override void OnCompletedRotation()
        {
            DebugPrint("Rotation complete.");

            if (stateMachine.MicroState == MicroBotState.ROTATE)
            {
                stateMachine.MicroState = MicroBotState.MOVE_FORWARD;
                elapsed = 0;
                Game.StartMoving(VTankObject.Direction.FORWARD);

                // Time to next point: t = d / v
                if (targetNode != null)
                {
                    timeToNextPoint = Math.Sqrt(
                        Math.Pow(Game.LocalPlayer.Position.y - targetNode.ToPixelsY(), 2) +
                        Math.Pow(Game.LocalPlayer.Position.x - targetNode.ToPixelsX(), 2)) /
                        (VELOCITY * Game.LocalPlayer.SpeedFactor);
                    DebugPrint("It will take {0:0.00} seconds to reach ({1:0.00}, {2:0.00}).", timeToNextPoint,
                        targetNode.ToPixelsX(), targetNode.ToPixelsY());
                }
            }
        }

        public override void OnHitWall()
        {
            Game.StopMoving();
            stateMachine.MicroState = MicroBotState.STILL;

            if (targetNode != null)
            {
                Game.StartMoving(VTankObject.Direction.REVERSE);
                stateMachine.MicroState = MicroBotState.MOVE_REVERSE;
                VTankObject.Point targetPosition = new VTankObject.Point(
                    Game.LocalPlayer.Position.x - Math.Cos(Game.LocalPlayer.Angle) * 100.0f,
                    Game.LocalPlayer.Position.y - Math.Sin(Game.LocalPlayer.Angle) * 100.0f);
                
                timeToNextPoint = Math.Sqrt(
                    Math.Pow(Game.LocalPlayer.Position.y - targetPosition.y, 2) +
                    Math.Pow(Game.LocalPlayer.Position.x - targetPosition.x, 2)) /
                    (VELOCITY * Game.LocalPlayer.SpeedFactor);

                DebugPrint("OnHitWall(), will take {0:0.00} seconds to reverse.", timeToNextPoint);
            }
            else
            {
                stateMachine.MicroState = MicroBotState.ROTATE;
                Random random = new Random();
                float rotation = random.Next(0, 1) == 0 ? 1.5f : -1.5f;
                Game.RotateTo(Player, (Player.Angle + rotation) % (Math.PI * 2));
            }
        }

        public override void OnMapRotation(MapRotationEventArgs e)
        {
            stateMachine.GameMode = GameMode;
            stateMachine.CurrentMap = Game.CurrentMap;
            stateMachine.State = BotState.COMPLETE_OBJECTIVE;

            currentPath = null;
            targetNode = null;

            string[] messages = new string[] {
                "Saved by the bell...",
                "Ding ding! Round 2!",
                "See you on " + e.MapName.Replace(".vtmap", "").Replace('-', ' ').Replace('_', ' ') + "."
            };
            int index = new Random().Next(0, 3);

            GameServer.SendChatMessage(messages[index]);

            RefreshStateMachine();

            alive = true;
            stateMachine.MicroState = MicroBotState.STILL;
            currentTarget = null;
            lastNode = null;
            elapsed = 0;
            timeToNextPoint = 0;
        }
        #endregion

        #region Private Methods
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
        /// Get the local player's position as a node.
        /// </summary>
        /// <returns></returns>
        private Node GetPositionAsNode()
        {
            VTankObject.Point pos = Game.LocalPlayer.Position;
            int tileX = (int)Math.Round(pos.x / Tile.TILE_SIZE_IN_PIXELS);
            int tileY = (int)Math.Round(-pos.y / Tile.TILE_SIZE_IN_PIXELS);
            return new Node(tileX, tileY);
        }

        /// <summary>
        /// Get the angle required to face the next node.
        /// </summary>
        /// <returns></returns>
        private double GetAngleToNextNode(Node lastNode, Node nextNode)
        {
            int deltaX = nextNode.X - lastNode.X;
            int deltaY = -(nextNode.Y - lastNode.Y);

            return Math.Atan2(deltaY, deltaX);
        }

        #endregion
    }

    #region struct TileNode
    /// <summary>
    /// Represents a specific 2D position of a tile.
    /// </summary>
    struct TileNode
    {
        public int X;
        public int Y;

        public TileNode(int x, int y)
            : this()
        {
            X = x;
            Y = y;
        }
    }
    #endregion
}
