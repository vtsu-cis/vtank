using System;
using Client.src.states.gamestate;
using Client.src.service;
using Renderer.SceneTools.Entities;
using Microsoft.Xna.Framework.Graphics;

namespace Client.src.events.types
{
    public class FlagPickedUpEvent : IEvent
    {
        int pickedUpById;
        GameSession.Alliance flagColor;

        public FlagPickedUpEvent(GamePlayState _game, int pickedUpById, GameSession.Alliance flagColor)
            : base(_game)
        {
            this.pickedUpById = pickedUpById;
            this.flagColor = flagColor;
        }

        public override void DoAction()
        {
            GamePlayState state = (GamePlayState)ServiceManager.StateManager.CurrentState;
            Game.Flags.FlagPickedUp(flagColor, ServiceManager.Scene.Access3D(state.Players[pickedUpById].RenderID));

            string message = state.Players[pickedUpById].Name + " has the " + Enum.GetName(typeof(GameSession.Alliance), flagColor).ToLower() + " flag.";
            Game.Chat.AddMessage(message, Color.Chartreuse);
        }
    }
}
