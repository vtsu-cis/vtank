using System;
using AIFramework.Bot;

namespace AIFramework.Util.Event.EventHandlers
{
    class ResetPositionEvent : IEvent
    {
        private VTankObject.Point position;

        public ResetPositionEvent(VTankBot _game, VTankObject.Point position)
            : base(_game)
        {
            this.position = position;
        }

        public override void DoAction()
        {
            Bot.InvokeResetPosition(position);
        }
    }
}
