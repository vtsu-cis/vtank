using AIFramework.Bot;

namespace AIFramework.Util.Event.EventHandlers
{
    public class PlayerLeftEvent : IEvent
    {
        private int id;

        public PlayerLeftEvent(VTankBot _game, int id)
            : base(_game)
        {
            this.id = id;
        }

        public override void DoAction()
        {
            Bot.InvokePlayerLeft(id);
        }
    }
}
