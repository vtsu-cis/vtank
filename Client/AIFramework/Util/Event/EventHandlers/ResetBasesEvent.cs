using System;
using AIFramework.Bot;
using AIFramework.Util.Modes.Impl;
using System.Collections.Generic;

namespace AIFramework.Util.Event.EventHandlers
{
    public class ResetBasesEvent : IEvent
    {
        #region Members
        GameSession.Alliance winner;
        #endregion

        #region Constructors
        public ResetBasesEvent(VTankBot _game, GameSession.Alliance winner)
            : base(_game)
        {
            this.winner = winner;
        }
        #endregion

        #region Methods
        public override void DoAction()
        {
            try
            {
                CaptureTheBaseMode handler = (CaptureTheBaseMode)Bot.Game.GameModeHandler;
                handler.ResetBases();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
            }
        }
        #endregion
    }
}
