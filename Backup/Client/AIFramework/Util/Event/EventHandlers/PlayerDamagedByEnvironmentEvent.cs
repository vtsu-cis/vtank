using System;
using AIFramework.Bot;

namespace AIFramework.Util.Event.EventHandlers
{
    public class PlayerDamagedByEnvironmentEvent : IEvent
    {
        #region Members
        private int playerId;
        private int environId;
        private int damageTaken;
        private bool killingBlow;
        #endregion

        #region Constructors

        public PlayerDamagedByEnvironmentEvent(VTankBot _game, int playerId, int environId, int damageTaken, bool killingBlow)
            : base(_game)
        {
            this.playerId = playerId;
            this.environId = environId;
            this.damageTaken = damageTaken;
            this.killingBlow = killingBlow;
        }

        #endregion

        #region Overrides
        public override void DoAction()
        {
            
        }
        #endregion
    }
}
