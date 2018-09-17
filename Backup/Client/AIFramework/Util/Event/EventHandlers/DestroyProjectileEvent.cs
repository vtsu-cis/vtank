using AIFramework.Bot;

namespace AIFramework.Util.Event.EventHandlers
{
    public class DestroyProjectileEvent : IEvent
    {
        private int projectileId;

        public DestroyProjectileEvent(VTankBot _game, int projectileId)
            : base(_game)
        {
            this.projectileId = projectileId;
        }

        public override void DoAction()
        {
            
        }
    }
}
