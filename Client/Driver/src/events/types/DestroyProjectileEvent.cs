using Client.src.states.gamestate;

namespace Client.src.events.types
{
    public class DestroyProjectileEvent : IEvent
    {
        int projectileId;

        public DestroyProjectileEvent(GamePlayState _game, int projectileId)
            : base(_game)
        {
            this.projectileId = projectileId;
        }

        public override void DoAction()
        {
            Game.Projectiles.Remove(projectileId);
        }
    }
}
