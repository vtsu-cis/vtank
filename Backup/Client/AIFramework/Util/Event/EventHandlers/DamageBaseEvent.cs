using System;
using AIFramework.Bot;
using AIFramework.Util.Modes.Impl;

namespace AIFramework.Util.Event.EventHandlers
{
    public class DamageBaseEvent : IEvent
    {
        #region Members
        private int baseEventId;
        private int damageAmount;
        private int playerId;
        private int projectileId;
        private bool isDestroyed;
        #endregion

        #region Constructors
        public DamageBaseEvent(int eventId, int damageAmount, int playerId, 
            int projectileId, bool isDestroyed, VTankBot _game)
            : base(_game)
        {
            this.baseEventId = eventId;
            this.damageAmount = damageAmount;
            this.playerId = playerId;
            this.projectileId = projectileId;
            this.isDestroyed = isDestroyed;
        }
        #endregion

        public override void DoAction()
        {
            try
            {
                CaptureTheBaseMode handler = (CaptureTheBaseMode)Bot.Game.GameModeHandler;
                Base b = handler.GetBaseByID(baseEventId);
                b.InflictDamage(damageAmount, isDestroyed);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
            }
        }
    }
}
