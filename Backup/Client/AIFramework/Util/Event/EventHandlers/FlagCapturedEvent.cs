using System;
using AIFramework.Bot;

namespace AIFramework.Util.Event.EventHandlers
{
    public class FlagCapturedEvent : IEvent
    {
        private int capturedById;
        private GameSession.Alliance flagColor;

        public FlagCapturedEvent(VTankBot _game, int capturedById, GameSession.Alliance flagColor)
            : base(_game)
        {
            this.capturedById = capturedById;
            this.flagColor = flagColor;
        }

        public override void DoAction()
        {
            
        }
    }
}
