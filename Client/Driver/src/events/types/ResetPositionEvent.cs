using System;
using Client.src.util;
using Client.src.states.gamestate;

namespace Client.src.events.types
{
    class ResetPositionEvent : IEvent
    {
        VTankObject.Point position;

        public ResetPositionEvent(GamePlayState _game, VTankObject.Point position)
            : base(_game)
        {
            this.position = position;
        }

        public override void DoAction()
        {
            PlayerTank player = Game.Players.GetLocalPlayer();

            Console.Error.WriteLine();
            Console.Error.WriteLine("Tank's position has been reset.");
            Console.Error.WriteLine("Old position: ({0}, {1})", player.Position.X, player.Position.Y);
            Console.Error.WriteLine("New position: ({0}, {1})", position.x, position.y);
            Console.Error.WriteLine();

            player.SetPosition(position);
        }
    }
}
