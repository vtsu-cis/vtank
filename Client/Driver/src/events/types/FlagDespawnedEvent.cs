using Client.src.states.gamestate;

namespace Client.src.events.types
{
    public class FlagDespawnedEvent : IEvent
    {
        GameSession.Alliance flagColor;

        public FlagDespawnedEvent(GamePlayState _game, GameSession.Alliance flagColor)
            : base(_game)
        {
            this.flagColor = flagColor;
        }

        public override void DoAction()
        {
            Game.Flags.DespawnFlag(flagColor);
        }
    }
}
