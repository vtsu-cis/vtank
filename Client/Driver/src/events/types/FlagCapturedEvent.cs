using System;
using Client.src.states.gamestate;
using Client.src.service;
using Microsoft.Xna.Framework.Graphics;

namespace Client.src.events.types
{
    public class FlagCapturedEvent : IEvent
    {
        int capturedById;
        GameSession.Alliance flagColor;

        public FlagCapturedEvent(GamePlayState _game, int capturedById, GameSession.Alliance flagColor)
            : base(_game)
        {
            this.capturedById = capturedById;
            this.flagColor = flagColor;
        }

        public override void DoAction()
        {
            Game.Flags.DespawnAll();

            GamePlayState state = (GamePlayState)ServiceManager.StateManager.CurrentState;
            string message = state.Players[capturedById].Name + " captured the " + Enum.GetName(typeof(GameSession.Alliance), flagColor).ToLower() + " flag.";
            state.Scores.AddObjCapture(state.Players[capturedById].Name, 1);
            Game.Chat.AddMessage(message, Color.Chartreuse);
        }
    }
}
