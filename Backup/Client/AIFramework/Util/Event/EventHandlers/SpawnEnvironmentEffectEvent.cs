using System;
using AIFramework.Bot;
using AIFramework.Bot.Game;

namespace AIFramework.Util.Event.EventHandlers
{
    public class SpawnEnvironmentEffectEvent : IEvent
    {
        #region Members
        private int id;
        private int typeId;
        private VTankObject.Point location;
        private int ownerId;
        private EnvironmentProperty envEffect;
        #endregion

        #region Constructors
        public SpawnEnvironmentEffectEvent(VTankBot _game, int id, int typeId, VTankObject.Point location, int ownerId)
            : base(_game)
        {
            this.id = id;
            this.typeId = typeId;
            this.location = location;
            this.ownerId = ownerId;
            envEffect = WeaponLoader.GetEnvironmentProperty(typeId);
        }
        #endregion

        #region Overrides
        public override void DoAction()
        {
            
        }

        public override void Dispose()
        {
            envEffect = null;

            base.Dispose();
        }
        #endregion
    }
}
