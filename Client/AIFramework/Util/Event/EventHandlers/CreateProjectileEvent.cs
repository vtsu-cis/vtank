using AIFramework.Bot;
using System;
using AIFramework.Bot.Game;

namespace AIFramework.Util.Event.EventHandlers
{
    /// <summary>
    /// Event for adding projectiles to the game.
    /// </summary>
    public class CreateProjectileEvent : IEvent
    {
        #region Fields
        private ProjectileData type;
        private VTankObject.Point point;
        private int ownerId;
        private int projectileId;
        #endregion

        #region Constructors
        public CreateProjectileEvent(VTankBot _game, int projectileTypeId, 
                                     VTankObject.Point point, int ownerId,
                                     int projectileId)
            : base(_game)
        {
            this.type = WeaponLoader.GetProjectile(projectileTypeId);
            this.point = point;
            this.ownerId = ownerId;
            this.projectileId = projectileId;
        }
        #endregion

        #region Overrides
        public override void DoAction()
        {
            Bot.InvokeOnProjectileFired(projectileId, ownerId, type.ID, point);
        }

        public override void Dispose()
        {
            type = null;

            base.Dispose();
        }
        #endregion
    }
}
