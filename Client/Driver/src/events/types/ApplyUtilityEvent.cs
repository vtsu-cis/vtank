using System;
using Client.src.states.gamestate;
using Client.src.util.game;
using Client.src.util;

namespace Client.src.events.types
{
    /// <summary>
    /// An event which, upon invocation, applies a utility to a player.
    /// </summary>
    public class ApplyUtilityEvent : IEvent
    {
        #region Members
        private int utilityID;
        private VTankObject.Utility utility;
        private int playerID;
        #endregion

        #region Constructor
        public ApplyUtilityEvent(GamePlayState _game, int _utilityID, 
            VTankObject.Utility _utility, int _playerID)
            : base(_game)
        {
            utilityID = _utilityID;
            utility = _utility;
            playerID = _playerID;
        }
        #endregion

        #region Overriden Methods
        public override void DoAction()
        {
            if ( !Game.Utilities.HasUtility(utilityID) || !Game.Players.ContainsKey(playerID))
            {
                return;
            }

            if (utility.duration > 0)
            {
                if (playerID == Game.Players.GetLocalPlayer().ID)
                {
                    Game.buffbar.AddBuff(new Buff(utility.model, (int)utility.duration, utility.description));
                }

                Game.Players[playerID].ApplyUtility(utility);
            }
            else
            {
                Game.Players[playerID].ApplyInstantHealth(utility);
            }

            Game.Utilities.Remove(utilityID);
        }
        #endregion
    }
}
