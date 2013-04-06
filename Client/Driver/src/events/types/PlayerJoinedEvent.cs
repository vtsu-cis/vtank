using Client.src.states.gamestate;
using Client.src.util;
using Microsoft.Xna.Framework.Graphics;

namespace Client.src.events.types
{
    public class PlayerJoinedEvent : IEvent
    {
        GameSession.Tank tank;

        public PlayerJoinedEvent(GamePlayState _game, GameSession.Tank tank)
            : base(_game)
        {
            this.tank = tank;
        }

        public override void DoAction()
        {
            PlayerTank newTank = PlayerManager.CreateTankObject(tank);
            newTank.SetPosition(tank.position);
            Game.Players[newTank.ID] = newTank;
            Game.NeedsSync = true;
            Game.Scores.AddPlayer(tank.attributes.name, tank.team);
            Game.Chat.AddMessage(tank.attributes.name + " has joined the game.", Color.Yellow);
        }
    }
}
