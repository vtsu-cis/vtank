using System;
using AIFramework.Bot;

namespace AIFramework.Util.Event.EventHandlers
{
    public class FlagDroppedEvent : IEvent
    {
        private int droppedId;
        private VTankObject.Point where;
        private GameSession.Alliance flagColor;

        public FlagDroppedEvent(VTankBot _game, int droppedId, VTankObject.Point where, GameSession.Alliance flagColor)
            : base(_game)
        {
            this.droppedId = droppedId;
            this.where = where;
            this.flagColor = flagColor;
        }

        public override void DoAction()
        {
            
        }
    }
}
