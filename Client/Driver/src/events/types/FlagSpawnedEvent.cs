using Client.src.states.gamestate;
using Microsoft.Xna.Framework;

namespace Client.src.events.types
{
    public class FlagSpawnedEvent : IEvent
    {
        VTankObject.Point where;
        GameSession.Alliance flagColor;

        public FlagSpawnedEvent(GamePlayState _game, VTankObject.Point where, GameSession.Alliance flagColor)
            : base(_game)
        {
            this.where = where;
            this.flagColor = flagColor;
        }

        public override void DoAction()
        {
            Game.Flags.PositionFlag(flagColor, new Vector3((float)where.x, (float)where.y, 0));
        }
    }
}
