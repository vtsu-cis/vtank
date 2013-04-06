using Client.src.states.gamestate;
using Microsoft.Xna.Framework.Graphics;

namespace Client.src.events.types
{
    public class PlayerLeftEvent : IEvent
    {
        int id;
        string name;

        public PlayerLeftEvent(GamePlayState _game, int id, string name)
            : base(_game)
        {
            this.id = id;
            this.name = name;
        }

        public override void DoAction()
        {
            Game.Players.Remove(id);
            Game.Scores.RemovePlayer(name);
            Game.Chat.AddMessage(name + " has left the game.", Color.Yellow);
        }
    }
}
