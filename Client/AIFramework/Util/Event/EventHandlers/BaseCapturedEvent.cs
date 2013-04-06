using System;
using AIFramework.Bot;
using AIFramework.Util.Modes.Impl;

namespace AIFramework.Util.Event.EventHandlers
{
    public class BaseCapturedEvent : IEvent
    {
        #region Members
        private int baseEventId;
        private GameSession.Alliance newBaseColor;
        private GameSession.Alliance oldBaseColor;
        private int capturerId;
        #endregion

        #region Constructors
        public BaseCapturedEvent(int eventId, GameSession.Alliance newBaseColor, int capturerId, 
            GameSession.Alliance oldBaseColor, VTankBot _game) : base(_game)
        {
            this.baseEventId = eventId;
            this.newBaseColor = newBaseColor;
            this.capturerId = capturerId;
            this.oldBaseColor = oldBaseColor;
        }
        #endregion

        public override void DoAction()
        {
            try
            {
                CaptureTheBaseMode handler = (CaptureTheBaseMode)Bot.Game.GameModeHandler;
                Base b = handler.GetBaseByID(baseEventId);
                b.Health = Base.DEFAULT_BASE_HEALTH;
                b.Team = newBaseColor;

                handler.DetectAttackableBases();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
            }
        }
    }
}
