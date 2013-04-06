using Client.src.states.gamestate;
using Microsoft.Xna.Framework;
using Client.src.util;

namespace Client.src.events.types
{
    /// <summary>
    /// Event which, upon invocation, adds a utility to the game.
    /// </summary>
    public class AddUtilityEvent : IEvent
    {
        #region Members
        private int utilityID;
        private VTankObject.Utility utility;
        private Vector3 position;
        #endregion

        #region Constructor
        public AddUtilityEvent(GamePlayState _game, 
            int _utilityID, VTankObject.Utility _utility, Vector3 _position)
            : base(_game)
        {
            utilityID = _utilityID;
            utility = _utility;
            position = _position;
        }
        #endregion

        #region Overriden Methods
        public override void DoAction()
        {
            //ServiceManager.Game.Console.DebugPrint("DoAction(): AddUtility");
            Game.Utilities.AddUtility(utilityID, utility.model, position);
        }
        #endregion
    }
}
