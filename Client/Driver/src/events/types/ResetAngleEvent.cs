using Client.src.states.gamestate;

namespace Client.src.events.types
{
    class ResetAngleEvent : IEvent
    {
        double angle;

        public ResetAngleEvent(GamePlayState _game, double angle)
            : base(_game)
        {
            this.angle = angle;
        }

        public override void DoAction()
        {
            Game.Players.GetLocalPlayer().Angle = (float)angle;
        }
    }
}
