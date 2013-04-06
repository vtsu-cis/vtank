using AIFramework.Bot;

namespace AIFramework.Util.Event.EventHandlers
{
    /// <summary>
    /// Event which, upon invocation, adds a utility to the game.
    /// </summary>
    public class AddUtilityEvent : IEvent
    {
        #region Members
        private int utilityID;
        private VTankObject.Utility utility;
        private VTankObject.Point position;
        #endregion

        #region Constructor
        public AddUtilityEvent(VTankBot _game, 
            int _utilityID, VTankObject.Utility _utility, VTankObject.Point _position)
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
            
        }

        public override void Dispose()
        {
            utility = null;

            base.Dispose();
        }
        #endregion
    }
}
