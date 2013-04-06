using System;
using AIFramework.Bot;
using AIFramework.Util.Modes.Impl;

namespace AIFramework.Util.Event.EventHandlers
{
    public class DamageBaseByEnvironmentEvent : IEvent
    {
        #region Members
        private GameSession.Alliance baseColor;
        private int baseId;
        private int environmentEffectId;
        private int damage;
        private bool isDestroyed;
        #endregion

        #region Constructors
        public DamageBaseByEnvironmentEvent(VTankBot _game, GameSession.Alliance baseColor, int baseId, int envId,
            int damage, bool isDestroyed)
            : base(_game)
        {
            this.baseColor = baseColor;
            this.baseId = baseId;
            this.environmentEffectId = envId;
            this.damage = damage;
            this.isDestroyed = isDestroyed;
        }
        #endregion

        #region Overrides
        public override void DoAction()
        {
            try 
            {
                CaptureTheBaseMode handler = (CaptureTheBaseMode)Bot.Game.GameModeHandler;
                Base b = handler.GetBaseByID(baseId);
                b.InflictDamage(damage, isDestroyed);
            }
            catch (Exception ex) 
            {
                Console.Error.WriteLine(ex);
            }
        }
        #endregion
    }
}
