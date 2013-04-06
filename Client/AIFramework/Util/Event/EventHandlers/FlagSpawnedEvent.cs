using AIFramework.Bot;

namespace AIFramework.Util.Event.EventHandlers
{
    public class FlagSpawnedEvent : IEvent
    {
        private VTankObject.Point where;
        private GameSession.Alliance flagColor;

        public FlagSpawnedEvent(VTankBot _game, VTankObject.Point where, GameSession.Alliance flagColor)
            : base(_game)
        {
            this.where = where;
            this.flagColor = flagColor;
        }

        public override void DoAction()
        {
            
        }
    }
}
