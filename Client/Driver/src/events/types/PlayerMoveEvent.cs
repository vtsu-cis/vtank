using Client.src.states.gamestate;
using Client.src.util;

namespace Client.src.events.types
{
    class PlayerMoveEvent : IEvent
    {
        int id;
        VTankObject.Point point;
        VTankObject.Direction direction;

        public PlayerMoveEvent(GamePlayState _game, int id, VTankObject.Point point,
            VTankObject.Direction direction) : base(_game)
        {
            this.id = id;
            this.direction = direction;
            this.point = point;
        }

        public override void DoAction()
        {
            PlayerTank tank = Game.Players[id];
            tank.SetPosition(point);
            tank.DirectionMovement = direction;
            tank.PreviouslyCollidied = false;
        }
    }
}
