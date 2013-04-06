using System;
using AIFramework.Bot;

namespace AIFramework.Util.Event.EventHandlers
{
    public class TurretSpinningEvent : IEvent
    {
        private int id;
        private double angle;
        private VTankObject.Direction direction;

        public TurretSpinningEvent(VTankBot _game, int id, double angle, VTankObject.Direction direction)
            : base(_game)
        {
            this.id = id;
            this.angle = angle;
            this.direction = direction;
        }

        public override void DoAction()
        {
            
        }
    }
}
