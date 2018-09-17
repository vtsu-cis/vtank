using System;
using System.Collections.Generic;
using System.Text;
using AIFramework.Bot;

namespace AIFramework.Util.Event.EventHandlers
{
    public class CreateProjectilesEvent : IEvent
    {
        #region Members
        private GameSession.ProjectileDamageInfo[] projectiles;
        #endregion

        #region Constuctors
        public CreateProjectilesEvent(VTankBot _game, GameSession.ProjectileDamageInfo[] projectiles) : base(_game)
        {
            this.projectiles = projectiles;
        }
        #endregion

        #region Overrides
        public override void DoAction()
        {
            
        }
        #endregion
    }
}
