using System;
using AIFramework.Bot;

namespace AIFramework.Util.Event.EventHandlers
{
    public class PlayerDamagedEvent : IEvent
    {
        private int victimId;
        private int projectileId;
        private int ownerId;
        private int damageTaken;
        private bool killingBlow;

        public PlayerDamagedEvent(VTankBot _game, int victimId, int projectileId, int ownerId, int damageTaken, bool killingBlow)
            : base(_game)
        {
            this.victimId = victimId;
            this.projectileId = projectileId;
            this.ownerId = ownerId;
            this.damageTaken = damageTaken;
            this.killingBlow = killingBlow;
        }

        public override void DoAction()
        {
            Bot.InvokeOnProjectileHit(victimId, projectileId, damageTaken, killingBlow);
        }
    }
}
