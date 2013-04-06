using System;
using AIFramework.Bot;
using System.Threading;
using AIFramework.Bot.EventArgs;
using AIFramework.Bot.Game;
using AIFramework.Util.Event;
using AIFramework.Util.Event.EventHandlers;

namespace AIFramework.Runner
{
    /// <summary>
    /// Internal client class which handles callbacks.
    /// </summary>
    internal class ClientI : GameSession.ClientEventCallbackDisp_
    {
        #region Members
        private BotRunner runner;
        private VTankBot bot;
        private EventBuffer buffer;
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the bot runner running this client.
        /// </summary>
        internal BotRunner BotRunner
        {
            get
            {
                lock (this)
                {
                    return runner;
                }
            }

            set
            {
                lock (this)
                {
                    runner = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the bot which is used for callbacks.
        /// </summary>
        internal VTankBot Bot
        {
            get
            {
                lock (this)
                {
                    return bot;
                }
            }

            set
            {
                lock (this)
                {
                    bot = value;
                }
            }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Constructs the client interface.
        /// </summary>
        /// <param name="runner">Runner running this bot.</param>
        /// <param name="bot">Bot which will receive events.</param>
        public ClientI(BotRunner runner, VTankBot bot, EventBuffer buffer)
        {
            BotRunner = runner;
            Bot = bot;
            this.buffer = buffer;
        }
        #endregion

        #region Overridden Methods
        public override void PlayerJoined(GameSession.Tank tank, Ice.Current current__)
        {
            buffer.Push(new PlayerJoinedEvent(bot, tank));
        }

        public override void PlayerLeft(int id, Ice.Current current__)
        {
            buffer.Push(new PlayerLeftEvent(bot, id));
        }

        public override void PlayerMove(int id, VTankObject.Point point, 
            VTankObject.Direction direction, Ice.Current current__)
        {
            buffer.Push(new PlayerMoveEvent(bot, id, point, direction));
        }

        public override void PlayerRotate(int id, double angle,
            VTankObject.Direction direction, Ice.Current current__)
        {
            buffer.Push(new PlayerRotateEvent(bot, id, angle, direction));
        }

        public override void TurretSpinning(int id, double angle, 
            VTankObject.Direction direction, Ice.Current current__)
        {
            buffer.Push(new TurretSpinningEvent(bot, id, angle, direction));
        }

        public override void PlayerDamaged(int id, int projectileId, int owner,
            int damageTaken, bool killingBlow, Ice.Current current__)
        {
            buffer.Push(new PlayerDamagedEvent(bot, id, projectileId, owner, damageTaken, killingBlow));
        }

        public override void PlayerRespawned(int id, VTankObject.Point where,
            Ice.Current current__)
        {
            buffer.Push(new PlayerRespawnedEvent(bot, id, where));
        }

        public override void CreateProjectile(int ownerId, int projectileId,
            int projectileTypeId, VTankObject.Point point, Ice.Current current__)
        {
            buffer.Push(new CreateProjectileEvent(bot, projectileTypeId, point, ownerId, projectileId));
        }

        public override void DestroyProjectile(int projectileId, Ice.Current current__)
        {
            buffer.Push(new DestroyProjectileEvent(Bot, projectileId));
        }

        public override void ChatMessage(string message, VTankObject.VTankColor color,
            Ice.Current current__)
        {
            Bot.ChatMessage(message);
        }

        public override void ResetPosition(VTankObject.Point point,
            Ice.Current current__)
        {
            buffer.Push(new ResetPositionEvent(bot, point));
        }

        public override void ResetAngle(double angle, Ice.Current current__)
        {
            buffer.Push(new ResetAngleEvent(bot, angle));
        }

        public override void RotateMap(Ice.Current current__)
        {
            buffer.PopAll();
            buffer.Push(new MapRotationEvent(bot));
        }

        public override void SpawnUtility(int utilityId, VTankObject.Utility utility, VTankObject.Point pos, Ice.Current current__)
        {
            buffer.Push(new SpawnUtilityEvent(bot, utilityId, utility, pos));
        }

        public override void ApplyUtility(int utilityId, VTankObject.Utility utility, int playerId, Ice.Current current__)
        {
            buffer.Push(new ApplyUtilityEvent(bot, utilityId, utility, playerId));
        }

        public override void FlagDropped(int droppedId, VTankObject.Point where, GameSession.Alliance flagColor, Ice.Current current__)
        {
            buffer.Push(new FlagDroppedEvent(bot, droppedId, where, flagColor));
        }

        public override void FlagReturned(int returnedById, GameSession.Alliance flagColor, Ice.Current current__)
        {
            buffer.Push(new FlagReturnedEvent(bot, returnedById, flagColor));
        }

        public override void FlagPickedUp(int pickedUpById, GameSession.Alliance flagColor, Ice.Current current__)
        {
            buffer.Push(new FlagPickedUpEvent(bot, pickedUpById, flagColor));
        }

        public override void FlagCaptured(int capturedById, GameSession.Alliance flagColor, Ice.Current current__)
        {
            buffer.Push(new FlagCapturedEvent(bot, capturedById, flagColor));
        }

        public override void FlagSpawned(VTankObject.Point where, GameSession.Alliance flagColor, Ice.Current current__)
        {
            buffer.Push(new FlagSpawnedEvent(bot, where, flagColor));
        }

        public override void FlagDespawned(GameSession.Alliance flagColor, Ice.Current current__)
        {
            buffer.Push(new FlagDespawnedEvent(bot, flagColor));
        }

        public override void BaseCaptured(GameSession.Alliance oldBaseColor,
            GameSession.Alliance newBaseColor, int baseId, int capturerId, Ice.Current current__)
        {
            buffer.Push(new BaseCapturedEvent(baseId, newBaseColor, capturerId, oldBaseColor, bot));
        }

        public override void SetBaseHealth(GameSession.Alliance baseColor, int baseId,
            int newHealth, Ice.Current current__)
        {
            buffer.Push(new SetBaseHealthEvent(baseColor, baseId, newHealth, bot));
        }

        public override void DamageBase(GameSession.Alliance baseColor, int baseId, int damage,
            int projectileId, int playerId, bool isDestroyed, Ice.Current current__)
        {
            buffer.Push(new DamageBaseEvent(baseId, damage, playerId, projectileId, isDestroyed, bot));
        }

        public override void EndRound(GameSession.Alliance winner, Ice.Current current__)
        {
            buffer.Push(new ResetBasesEvent(bot, winner));
        }

        public override void SpawnEnvironmentEffect(int id, int typeId, int ownerId, 
            VTankObject.Point position, Ice.Current current__)
        {
            buffer.Push(new SpawnEnvironmentEffectEvent(bot, id, typeId, position, ownerId));
        }

        public override void PlayerDamagedByEnvironment(int playerId, int environId, int damageTaken, 
            bool killingBlow, Ice.Current current__)
        {
            buffer.Push(new PlayerDamagedByEnvironmentEvent(bot, playerId, environId, damageTaken, killingBlow));
        }

        public override void CreateProjectiles(GameSession.ProjectileDamageInfo[] list, Ice.Current current__)
        {
            buffer.Push(new CreateProjectilesEvent(bot, list));
        }

        public override void DamageBaseByEnvironment(GameSession.Alliance baseColor, int baseId, int envId, 
            int damage, bool isDestroyed, Ice.Current current__)
        {
            buffer.Push(new DamageBaseByEnvironmentEvent(bot, baseColor, baseId, envId, damage, isDestroyed));
        }
        #endregion
    }
}
