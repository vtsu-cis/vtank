using System;
using System.Collections.Generic;
using System.Text;
using Client.src.states.gamestate;

namespace Client.src.events.types
{
    public class SetBaseHealthEvent : IEvent
    {
        #region Members
        GameSession.Alliance baseColor;
        int baseEventId;
        int newBaseHealth;
        #endregion

        #region Constructors
        public SetBaseHealthEvent(GameSession.Alliance color, int eventId, int health, GamePlayState _game)
            : base(_game)
        {
            this.baseColor = color;
            this.baseEventId = eventId;
            this.newBaseHealth = health;
        }
        #endregion

        public override void DoAction()
        {
            Game.Bases.SetBaseHealth(baseEventId, newBaseHealth, baseColor);
        }
    }
}
