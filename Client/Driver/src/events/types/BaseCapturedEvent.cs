using System;
using System.Collections.Generic;
using System.Text;
using Client.src.states.gamestate;
using Microsoft.Xna.Framework.Graphics;

namespace Client.src.events.types
{
    public class BaseCapturedEvent : IEvent
    {
        #region Members
        int baseEventId;
        GameSession.Alliance newBaseColor;
        GameSession.Alliance oldBaseColor;
        int capturerId;
        #endregion

        #region Constructors
        public BaseCapturedEvent(int eventId, GameSession.Alliance newBaseColor, int capturerId, 
            GameSession.Alliance oldBaseColor, GamePlayState _game) : base(_game)
        {
            this.baseEventId = eventId;
            this.newBaseColor = newBaseColor;
            this.capturerId = capturerId;
            this.oldBaseColor = oldBaseColor;
        }
        #endregion

        public override void DoAction()
        {
            if (Game.Scores.DrawingRoundWinner)
                return;

            base.Game.Bases.CaptureBase(this.baseEventId, this.newBaseColor);
            string message = Game.Players[capturerId].Name + " captured the " + Enum.GetName(typeof(GameSession.Alliance), oldBaseColor).ToLower() + " base!";
            Game.Chat.AddMessage(message, Color.Chartreuse);
            Game.Scores.AddObjCapture(Game.Players[capturerId].Name, 1);
        }
    }
}
