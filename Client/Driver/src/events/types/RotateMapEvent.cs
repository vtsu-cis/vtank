using System;
using System.Collections.Generic;
using System.Text;
using Client.src.states.gamestate;
using Client.src.service;

namespace Client.src.events.types
{
    public class RotateMapEvent : IEvent
    {
        #region Members
        #endregion

        #region Constructors
        public RotateMapEvent(GamePlayState _game)
            : base(_game)
        {
        }
        #endregion

        #region Overrides
        public override void DoAction()
        {
            Game.RotateMap();
        }
        #endregion
    }
}
