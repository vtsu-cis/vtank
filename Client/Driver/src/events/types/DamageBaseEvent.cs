using System;
using System.Collections.Generic;
using System.Text;
using Client.src.states.gamestate;
using Microsoft.Xna.Framework.Graphics;

namespace Client.src.events.types
{
    public class DamageBaseEvent : IEvent
    {
        #region Members
        int baseEventId;
        int damageAmount;
        int playerId;
        int projectileId;
        bool isDestroyed;
        #endregion

        #region Constructors
        public DamageBaseEvent(int eventId, int damageAmount, int playerId, int projectileId, bool isDestroyed, GamePlayState _game)
            : base(_game)
        {
            this.baseEventId = eventId;
            this.damageAmount = damageAmount;
            this.playerId = playerId;
            this.projectileId = projectileId;
            this.isDestroyed = isDestroyed;
        }
        #endregion

        public override void DoAction()
        {
            if (this.isDestroyed)
            {
                string message = Game.Players[playerId].Name + " destroyed the " + 
                Game.Bases.GetBase(baseEventId).BaseColor.ToString().ToLower() +" base!";
                Game.Bases.DestroyBase(baseEventId);
                Game.Chat.AddMessage(message, Color.Chartreuse);
            }
            else
            {
                Game.Bases.DamageBase(baseEventId, damageAmount);
            }

            if ( Game.Players.ContainsKey(playerId))
            {
                Game.Projectiles.Remove(projectileId);
            }
        }
    }
}
