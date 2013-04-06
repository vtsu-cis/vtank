using System;
using AIFramework.Bot;

namespace AIFramework.Util.Event.EventHandlers
{
    public class FlagReturnedEvent : IEvent
    {
        private int returnedById;
        private GameSession.Alliance flagColor;

        public FlagReturnedEvent(VTankBot _game, int returnedById, GameSession.Alliance flagColor)
            : base(_game)
        {
            this.returnedById = returnedById;
            this.flagColor = flagColor;
        }

        public override void DoAction()
        {
            
        }
    }
}
