using System;
using Client.src.states.gamestate;
using Client.src.service;
using Microsoft.Xna.Framework.Graphics;

namespace Client.src.events.types
{
    public class FlagReturnedEvent : IEvent
    {
        int returnedById;
        GameSession.Alliance flagColor;

        public FlagReturnedEvent(GamePlayState _game, int returnedById, GameSession.Alliance flagColor)
            : base(_game)
        {
            this.returnedById = returnedById;
            this.flagColor = flagColor;
        }

        public override void DoAction()
        {
            Game.Flags.ResetFlag(flagColor);

            GamePlayState state = (GamePlayState)ServiceManager.StateManager.CurrentState;
            string message = state.Players[returnedById].Name + " returned the " + Enum.GetName(typeof(GameSession.Alliance), flagColor).ToLower() + " flag.";
            Game.Chat.AddMessage(message, Color.Chartreuse);
            //TODO do something with returnedById
        }
    }
}
