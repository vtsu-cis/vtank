using AIFramework.Bot;
using AIFramework.Bot.Game;
using AIFramework.Bot.EventArgs;

namespace AIFramework.Util.Event.EventHandlers
{
    public class PlayerJoinedEvent : IEvent
    {
        private GameSession.Tank tank;

        public PlayerJoinedEvent(VTankBot _game, GameSession.Tank tank)
            : base(_game)
        {
            this.tank = tank;
        }

        public override void DoAction()
        {
            Player newPlayer = new Player(tank);
            PlayerJoinedEventArgs args = new PlayerJoinedEventArgs(newPlayer);

            Bot.InvokePlayerJoined(args);

            args.Dispose();
        }

        public override void Dispose()
        {
            tank = null;

            base.Dispose();
        }
    }
}
