using AIFramework.Bot;

namespace AIFramework.Util.Event.EventHandlers
{
    class ResetAngleEvent : IEvent
    {
        double angle;

        public ResetAngleEvent(VTankBot _game, double angle)
            : base(_game)
        {
            this.angle = angle;
        }

        public override void DoAction()
        {
            
        }
    }
}
