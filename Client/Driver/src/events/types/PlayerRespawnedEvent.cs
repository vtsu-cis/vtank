using Client.src.states.gamestate;
using Client.src.util;

namespace Client.src.events.types
{
    public class PlayerRespawnedEvent : IEvent
    {
        private int id;
        private VTankObject.Point where;

        public PlayerRespawnedEvent(GamePlayState _game, int id, VTankObject.Point where)
            : base(_game)
        {
            this.id = id;
            this.where = where;
        }

        public override void DoAction()
        {
            PlayerTank tank;
            if (Game.Players.TryGetValue(id, out tank))
            {
                tank.Respawn(where);
                if (tank == Game.Players.GetLocalPlayer())
                {
                    Game.Scene.LockCameras();
                }
            }
        }
    }
}
