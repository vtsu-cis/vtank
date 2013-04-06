using Client.src.states.gamestate;
using Client.src.util;

namespace Client.src.events.types
{
    class PlayerRotateEvent : IEvent
    {
        int id;
        double angle;
        VTankObject.Direction direction;

        public PlayerRotateEvent(GamePlayState _game, int id, double angle, VTankObject.Direction direction)
            : base(_game)
        {
            this.id = id;
            this.angle = angle;
            this.direction = direction;
        }

        public override void DoAction()
        {
            PlayerTank tank = Game.Players[id];
            tank.Angle = (float)angle;
            tank.DirectionRotation = direction;
        }
    }
}
