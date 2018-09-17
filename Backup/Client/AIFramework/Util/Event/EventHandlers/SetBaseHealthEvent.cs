using System;
using System.Collections.Generic;
using System.Text;
using AIFramework.Bot;
using AIFramework.Util.Modes.Impl;

namespace AIFramework.Util.Event.EventHandlers
{
    public class SetBaseHealthEvent : IEvent
    {
        #region Members
        GameSession.Alliance baseColor;
        int baseEventId;
        int newBaseHealth;
        #endregion

        #region Constructors
        public SetBaseHealthEvent(GameSession.Alliance color, int eventId, int health, VTankBot _game)
            : base(_game)
        {
            this.baseColor = color;
            this.baseEventId = eventId;
            this.newBaseHealth = health;
        }
        #endregion

        public override void DoAction()
        {
            try
            {
                CaptureTheBaseMode handler = (CaptureTheBaseMode)Bot.Game.GameModeHandler;
                Base b = handler.GetBaseByID(baseEventId);
                b.Team = baseColor;
                b.Health = newBaseHealth;

                handler.DetectAttackableBases();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
            }
        }
    }
}
