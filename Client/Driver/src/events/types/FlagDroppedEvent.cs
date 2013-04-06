using System;
using Client.src.states.gamestate;
using Client.src.service;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Client.src.events.types
{
    public class FlagDroppedEvent : IEvent
    {
        int droppedId;
        VTankObject.Point where;
        GameSession.Alliance flagColor;

        public FlagDroppedEvent(GamePlayState _game, int droppedId, VTankObject.Point where, GameSession.Alliance flagColor)
            : base(_game)
        {
            this.droppedId = droppedId;
            this.where = where;
            this.flagColor = flagColor;
        }

        public override void DoAction()
        {
            Game.Flags.PositionFlag(flagColor, new Vector3((float)where.x, (float)where.y, 0));

            GamePlayState state = (GamePlayState)ServiceManager.StateManager.CurrentState;
            string message = state.Players[droppedId].Name + " dropped the " + Enum.GetName(typeof(GameSession.Alliance), flagColor).ToLower() + " flag.";
            Game.Chat.AddMessage(message, Color.Chartreuse);
        }
    }
}
