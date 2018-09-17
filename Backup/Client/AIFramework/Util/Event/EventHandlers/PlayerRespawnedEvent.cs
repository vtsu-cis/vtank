using AIFramework.Bot;


namespace AIFramework.Util.Event.EventHandlers
{
    public class PlayerRespawnedEvent : IEvent
    {
        private int id;
        private VTankObject.Point where;

        public PlayerRespawnedEvent(VTankBot _game, int id, VTankObject.Point where)
            : base(_game)
        {
            this.id = id;
            this.where = where;
        }

        public override void DoAction()
        {
            Bot.InvokePlayerHasRespawned(id, where);
        }
    }
}
