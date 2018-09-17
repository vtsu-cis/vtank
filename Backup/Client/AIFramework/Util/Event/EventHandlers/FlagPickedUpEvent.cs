using System;
using AIFramework.Bot;

namespace AIFramework.Util.Event.EventHandlers
{
    public class FlagPickedUpEvent : IEvent
    {
        private int pickedUpById;
        private GameSession.Alliance flagColor;

        public FlagPickedUpEvent(VTankBot _game, int pickedUpById, GameSession.Alliance flagColor)
            : base(_game)
        {
            this.pickedUpById = pickedUpById;
            this.flagColor = flagColor;
        }

        public override void DoAction()
        {
            
        }
    }
}
