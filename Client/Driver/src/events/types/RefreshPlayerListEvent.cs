using Client.src.states.gamestate;

namespace Client.src.events.types
{
    public class RefreshPlayerListEvent : IEvent
    {
        public RefreshPlayerListEvent(GamePlayState _game) : base(_game) { }

        public override void DoAction()
        {
            Game.Players.RefreshPlayerList();
        }
    }
}
