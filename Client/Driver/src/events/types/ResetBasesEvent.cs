using System;
using System.Collections.Generic;
using System.Text;
using Client.src.states.gamestate;
using Microsoft.Xna.Framework.Graphics;
using Client.src.util;

namespace Client.src.events.types
{
    public class ResetBasesEvent : IEvent
    {
        #region Members
        GameSession.Alliance winner;
        #endregion

        #region Constructors
        public ResetBasesEvent(GamePlayState _game, GameSession.Alliance winner)
            : base(_game)
        {
            this.winner = winner;
        }
        #endregion
        
        #region Methods
        public override void DoAction()
        {
            if (Game.CurrentGameMode == VTankObject.GameMode.CAPTURETHEBASE)
            {
                Game.Bases.ResetBases();
                Game.Scores.AddRoundWin(winner);
                string message = "The " + winner.ToString().ToLower() + " team has won the round!";

                Game.Chat.AddMessage(message, Color.Chartreuse);

                // Force players to look toward the next base.
                const int BLUE = 10;
                const int RED = 11;
                Base blueBase = Game.Bases.GetBase(BLUE);
                Base redBase  = Game.Bases.GetBase(RED);
                float angleRedToBlue = (float)Math.Atan2(
                    blueBase.Position.Y - redBase.Position.Y,
                    blueBase.Position.X - redBase.Position.X);
                float angleBlueToRed = (float)Math.Atan2(
                    redBase.Position.Y - blueBase.Position.Y,
                    redBase.Position.X - blueBase.Position.X);

                foreach (PlayerTank player in Game.Players.Values)
                {
                    bool isRed = player.Team == GameSession.Alliance.RED;
                    if (isRed)
                        player.Angle = angleRedToBlue;
                    else
                        player.Angle = angleBlueToRed;

                    if (player != Game.LocalPlayer)
                        player.TurretAngle = player.Angle;
                }
                Game.Scene.LockCameras();
            }
        }
        #endregion
    }
}
