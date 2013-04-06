using System;
using System.Collections.Generic;
using System.Text;
using Client.src.util;
using Client.src.states.gamestate;
using Client.src.util.game;
using Microsoft.Xna.Framework;

namespace Client.src.events.types
{
    public class SpawnEnvironmentEffectEvent : IEvent
    {

        #region Members
        private int id;
        private int typeId;
        private VTankObject.Point location;
        private int ownerId;
        private EnvironmentProperty envEffect;
        private Vector3 creationPosition;
        #endregion

        #region Constructors
        public SpawnEnvironmentEffectEvent(GamePlayState _game, int id, int typeId, VTankObject.Point location, int ownerId)
            : base(_game)
        {
            this.id = id;
            this.typeId = typeId;
            this.location = location;
            this.ownerId = ownerId;            
        }

        /// <summary>
        /// Get the position where the effect should be created.
        /// </summary>
        /// <returns>A vector3 describing the position.</returns>
        private Vector3 GetPosition()
        {
            return new Vector3((float)location.x, (float)location.y, 0f);
        }
        #endregion

        #region Overrides
        public override void DoAction()
        {
            envEffect = WeaponLoader.GetEnvironmentProperty(typeId);

            creationPosition = this.GetPosition();

            if ( Game.Players.ContainsKey(ownerId))
                envEffect.Owner = Game.Players[ownerId];

            Game.EnvironmentEffects.AddEffect(envEffect, id, creationPosition);
        }
        #endregion
    }
}
