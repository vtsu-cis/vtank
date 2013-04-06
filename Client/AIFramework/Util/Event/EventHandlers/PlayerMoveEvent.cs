using AIFramework.Bot;


namespace AIFramework.Util.Event.EventHandlers
{
    class PlayerMoveEvent : IEvent
    {
        private int id;
        private VTankObject.Point point;
        private VTankObject.Direction direction;

        public PlayerMoveEvent(VTankBot _game, int id, VTankObject.Point point,
            VTankObject.Direction direction) : base(_game)
        {
            this.id = id;
            this.direction = direction;
            this.point = point;
        }

        public override void DoAction()
        {
            Bot.InvokeOnPlayerMove(id, point, direction);
        }
    }
}
