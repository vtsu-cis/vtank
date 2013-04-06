using System;
using AIFramework.Bot;

namespace AIFramework.Util.Event.EventHandlers
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
        public ApplyUtilityEvent(VTankBot _game, int _utilityID, 
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
            Bot.InvokeApplyUtility(utilityID, utility, playerID);
        }

        public override void Dispose()
        {
            utility = null;

            base.Dispose();
        }
        #endregion
    }
}
