using System;
using Client.src.states.gamestate;
using Client.src.util;

namespace Client.src.events.types
{
    public class TurretSpinningEvent : IEvent
    {
        int id;
        double angle;
        VTankObject.Direction direction;

        public TurretSpinningEvent(GamePlayState _game, int id, double angle, VTankObject.Direction direction)
            : base(_game)
        {
            this.id = id;
            this.angle = angle;
            this.direction = direction;
        }

        public override void DoAction()
        {
            PlayerTank tank = Game.Players[id];
            tank.TurretAngle = (float)(angle + Math.PI);
        }
    }
}
