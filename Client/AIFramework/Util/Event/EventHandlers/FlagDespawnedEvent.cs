using AIFramework.Bot;

namespace AIFramework.Util.Event.EventHandlers
{
    public class FlagDespawnedEvent : IEvent
    {
        private GameSession.Alliance flagColor;

        public FlagDespawnedEvent(VTankBot _game, GameSession.Alliance flagColor)
            : base(_game)
        {
            this.flagColor = flagColor;
        }

        public override void DoAction()
        {
            
        }
    }
}
