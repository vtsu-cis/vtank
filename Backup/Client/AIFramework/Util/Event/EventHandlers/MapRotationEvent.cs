using System;
using System.Collections.Generic;
using System.Text;
using AIFramework.Bot;

namespace AIFramework.Util.Event.EventHandlers
{
    public class MapRotationEvent : IEvent
    {
        public MapRotationEvent(VTankBot bot)
            : base(bot)
        {
        }

        public override void DoAction()
        {
            Bot.InvokeOnMapRotation();
        }
    }
}
