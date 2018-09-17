using System;
using System.Collections.Generic;
using System.Text;
using AIFramework.Bot;

namespace AIFramework.Util.Event.EventHandlers
{
    class SpawnUtilityEvent : IEvent
    {
        private int ID;
        private VTankObject.Utility utility;
        private VTankObject.Point position;

        public SpawnUtilityEvent(VTankBot bot, int utilityId, VTankObject.Utility utility, VTankObject.Point pos)
            : base(bot)
        {
            ID = utilityId;
            this.utility = utility;
            position = pos;
        }

        public override void DoAction()
        {
            Bot.InvokeSpawnUtility(ID, utility, position);
        }

        public override void Dispose()
        {
            utility = null;

            base.Dispose();
        }
    }
}
