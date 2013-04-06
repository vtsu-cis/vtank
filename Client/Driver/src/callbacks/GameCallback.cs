/*!
    \file   GameCallback.cs
    \brief  Helps informing of player movement and events.
    \author (C) Copyright 2008 by Vermont Technical College
*/
using System;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Client.src.states.gamestate;
using Client.src.util;
using Client.src.service;
using Client.src.util.game;
using System.Collections.Generic;
using Renderer.SceneTools.Entities;
using Client.src.events.types;
using Client.src.events;

namespace Client.src.callbacks
{
    /// <summary>
    /// Callback which accepts messages regarding other players.
    /// Documentation for this class's methods are available in the GameSession.ice file.
    /// </summary>
    public class GameCallback : GameSession.ClientEventCallbackDisp_
    {
        #region Members
        private GamePlayState game;
        private EventBuffer buffer;
        private bool ready;
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the parent game that this callback is meant for.
        /// </summary>
        public GamePlayState Game
        {
            get
            {
                return game;
            }

            set
            {
                game = value;
            }
        }

        /// <summary>
        /// Gets or sets whether or not this callback is receiving messages.
        /// </summary>
        public bool Ready
        {
            get
            {
                 return ready;
            }

            set
            {
                ready = value;
            }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Start the game callback. This initially refuses messages until the Ready attribute
        /// is set to 'true'.
        /// </summary>
        /// <param name="_game"></param>
        public GameCallback(GamePlayState _game, EventBuffer _buffer)
        {
            game = _game;
            buffer = _buffer;
            ready = false;
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Check if this callback is ready to receive messages.
        /// </summary>
        /// <returns>True if the callback can receive messages; false otherwise.</returns>
        private bool ReceivingMessages()
        {
            return Ready;
        }

        /// <summary>
        /// Helper method for adding a chat message into the game. Uses a default color of
        /// white.
        /// </summary>
        /// <param name="message">Message to insert.</param>
        private void InsertChatMessage(string message)
        {
            InsertChatMessage(message, Color.White);
        }

        /// <summary>
        /// Helper method for adding a chat message into the game.
        /// </summary>
        /// <param name="message">Message to insert.</param>
        /// <param name="color">Color of the message text.</param>
        private void InsertChatMessage(string message, Color color)
        {
            ServiceManager.Game.Console.DebugPrint(
                String.Format("[CHAT] {0}", message));

            buffer.Enqueue(new ChatMessageEvent(game, message, color));
        }
        #endregion

        /*
         * The following methods are implemented from the Slice interface. For documentation, see
         * the GameSession.ice file.
         */
        #region Ice Methods
        #region Player

        public override void PlayerJoined(GameSession.Tank tank, Ice.Current current__)
        {
            buffer.Enqueue(new PlayerJoinedEvent(Game, tank));
        }

        public override void PlayerLeft(int id, Ice.Current current__)
        {
            buffer.Enqueue(new PlayerLeftEvent(Game, id, Game.Players[id].Name));
        }

        public override void ResetPosition(VTankObject.Point position, Ice.Current current__)
        {
            buffer.Enqueue(new ResetPositionEvent(Game, position));            
        }

        public override void ResetAngle(double angle, Ice.Current current__)
        {
            buffer.Enqueue(new ResetAngleEvent(Game, angle));
        }

        public override void PlayerMove(int id, VTankObject.Point point, 
            VTankObject.Direction direction, Ice.Current current__)
        {
            if (!ReceivingMessages())
                return;

            buffer.Enqueue(new PlayerMoveEvent(Game, id, point, direction));
        }

        public override void PlayerRotate(int id, double angle, VTankObject.Direction direction, Ice.Current current__)
        {
            if (!ReceivingMessages())
                return;

            buffer.Enqueue(new PlayerRotateEvent(Game, id, angle, direction));
        }

        public override void TurretSpinning(int id, double angle, VTankObject.Direction direction, Ice.Current current__)
        {
            if (!ReceivingMessages())
                return;

            buffer.Enqueue(new TurretSpinningEvent(Game, id, angle, direction));
        }


        public override void PlayerDamaged(int id, int projectileId, int ownerId, int damageTaken, bool killingBlow,
            Ice.Current current__)
        {
            if (!ReceivingMessages())
                return;
            
            buffer.Enqueue(new PlayerDamagedEvent(Game, id, projectileId, ownerId, damageTaken, killingBlow));
        }

        public override void PlayerRespawned(int id, VTankObject.Point where, Ice.Current current__)
        {
            if (!ReceivingMessages())
                return;

            buffer.Enqueue(new PlayerRespawnedEvent(Game, id, where));
        }
        #endregion        

        #region Projectile
        public override void CreateProjectile(int ownerId, int projectileId,
            int projectileTypeId, VTankObject.Point point, Ice.Current current__)
        {
            if (!ReceivingMessages())
                return;
            
            buffer.Enqueue(new CreateProjectileEvent(Game, projectileTypeId, point, ownerId, projectileId));
        }

        public override void CreateProjectiles(GameSession.ProjectileDamageInfo[] list, Ice.Current current__)
        {
            if (!ReceivingMessages())
                return;

            buffer.Enqueue(new CreateProjectilesEvent(Game, list));
        }

        public override void DestroyProjectile(int projectileId, Ice.Current current__)
        {
            if (!ReceivingMessages())
                return;

            buffer.Enqueue(new DestroyProjectileEvent(Game, projectileId));
        }

        #endregion

        public override void ChatMessage(string message, VTankObject.VTankColor color, Ice.Current current__)
        {
            if (!ReceivingMessages())
                return;

            try
            {
                InsertChatMessage(message, new Color(color.red, color.green, color.blue));
            }
            catch (Exception e) 
            { 
                Console.Error.WriteLine(e); 
            }
        }

        public override void RotateMap(Ice.Current current__)
        {
            try
            {
                Ready = false;
                buffer.Enqueue(new RotateMapEvent(Game));
            }
            catch(Exception e)
            {
                System.Console.Error.WriteLine("Error Rotating Map: \n{0}", e);
            }
        }

        #region Environment
        public override void SpawnEnvironmentEffect(int id, int typeId, int ownerId,
            VTankObject.Point position, Ice.Current current__)
        {
            buffer.Enqueue(new SpawnEnvironmentEffectEvent(Game, id, typeId, position, ownerId));
        }

        public override void PlayerDamagedByEnvironment(int playerId, int environId, int damageTaken,
            bool killingBlow, Ice.Current current__)
        {
            buffer.Enqueue(new PlayerDamagedByEnvironmentEvent(Game, playerId, environId, damageTaken, killingBlow));
        }

        public override void DamageBaseByEnvironment(GameSession.Alliance baseColor, int baseId, int envId,
            int damage, bool isDestroyed, Ice.Current current__)
        {
            buffer.Enqueue(new DamageBaseByEnvironmentEvent(Game, baseColor, baseId, envId, damage, isDestroyed));
        }
        #endregion

        #region Utility
        public override void SpawnUtility(int utilityId, VTankObject.Utility utility, VTankObject.Point pos, Ice.Current current__)
        {
            Vector3 position3d = new Vector3((float)pos.x, (float)pos.y, 0.0f);
            buffer.Enqueue(new AddUtilityEvent(Game, utilityId, utility, position3d));
        }

        public override void ApplyUtility(int utilityId, VTankObject.Utility utility, int playerId, Ice.Current current__)
        {
            buffer.Enqueue(new ApplyUtilityEvent(Game, utilityId, utility, playerId));
        }
        #endregion

        #region Capture The Flag
        public override void FlagDropped(int droppedId, VTankObject.Point where, GameSession.Alliance flagColor, Ice.Current current__)
        {
            buffer.Enqueue(new FlagDroppedEvent(game, droppedId, where, flagColor));
        }

        public override void FlagReturned(int returnedById, GameSession.Alliance flagColor, Ice.Current current__)
        {
            buffer.Enqueue(new FlagReturnedEvent(Game, returnedById, flagColor));
        }

        public override void FlagPickedUp(int pickedUpById, GameSession.Alliance flagColor, Ice.Current current__)
        {
            buffer.Enqueue(new FlagPickedUpEvent(Game, pickedUpById, flagColor));
        }

        public override void FlagCaptured(int capturedById, GameSession.Alliance flagColor, Ice.Current current__)
        {
            buffer.Enqueue(new FlagCapturedEvent(Game, capturedById, flagColor));
        }

        public override void FlagSpawned(VTankObject.Point where, GameSession.Alliance flagColor, Ice.Current current__)
        {
            buffer.Enqueue(new FlagSpawnedEvent(Game, where, flagColor));
        }

        public override void FlagDespawned(GameSession.Alliance flagColor, Ice.Current current__)
        {
            buffer.Enqueue(new FlagDespawnedEvent(Game, flagColor));
        }
        #endregion

        #region Capture The Base
        public override void BaseCaptured(GameSession.Alliance oldBaseColor, GameSession.Alliance newBaseColor,
           int baseId, int capturerId, Ice.Current current__)
        {
            buffer.Enqueue(new BaseCapturedEvent(baseId, newBaseColor, capturerId, oldBaseColor, Game));
        }

        public override void SetBaseHealth(GameSession.Alliance baseColor, int baseId, int newHealth, Ice.Current current__)
        {
            buffer.Enqueue(new SetBaseHealthEvent(baseColor, baseId, newHealth, Game));
        }

        public override void DamageBase(GameSession.Alliance baseColor, int baseId, int damage, int projectileId,
            int playerId, bool isDestroyed, Ice.Current current__)
        {
            buffer.Enqueue(new DamageBaseEvent(baseId, damage, playerId, projectileId, isDestroyed, Game));
        }

        public override void EndRound(GameSession.Alliance winner, Ice.Current current__)
        {
            buffer.Enqueue(new ResetBasesEvent(Game, winner));
        }
        #endregion

        #endregion
    }
}
