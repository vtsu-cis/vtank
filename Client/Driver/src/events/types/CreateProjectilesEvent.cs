using System;
using System.Collections.Generic;
using System.Text;
using Client.src.states.gamestate;

namespace Client.src.events.types
{
    public class CreateProjectilesEvent : IEvent
    {
        #region Members
        GameSession.ProjectileDamageInfo[] projectiles;
        #endregion

        #region Constuctors
        public CreateProjectilesEvent(GamePlayState _game, GameSession.ProjectileDamageInfo[] projectiles) : base(_game)
        {
            this.projectiles = projectiles;
        }
        #endregion

        #region Overrides
        public override void DoAction()
        {
            foreach (GameSession.ProjectileDamageInfo damageInfo in projectiles)
            {
                if (Game.Players.ContainsKey(damageInfo.ownerId))
                    Game.Projectiles.AddLater(damageInfo, Game.Players[damageInfo.ownerId]);
            }
        }
        #endregion
    }
}
