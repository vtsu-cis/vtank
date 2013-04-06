using System;
using System.Collections.Generic;
using System.Text;
using Client.src.states.gamestate;
using Client.src.util;
using Client.src.service;
using Microsoft.Xna.Framework.Graphics;

namespace Client.src.events.types
{
    public class PlayerDamagedByEnvironmentEvent : IEvent
    {
        #region Members
        private int playerId;
        private int environId;
        private int damageTaken;
        private bool killingBlow;
        #endregion

        #region Constructors

        public PlayerDamagedByEnvironmentEvent(GamePlayState _game, int playerId, int environId, int damageTaken, bool killingBlow)
            : base(_game)
        {
            this.playerId = playerId;
            this.environId = environId;
            this.damageTaken = damageTaken;
            this.killingBlow = killingBlow;
        }

        #endregion

        #region Overrides
        public override void DoAction()
        {
            try
            {
                PlayerTank attacker = Game.EnvironmentEffects.GetEffect(environId).Owner;
                PlayerTank victim = Game.Players[playerId];
                string shooterName = attacker.Name;

                if (victim.Name == PlayerManager.LocalPlayerName)
                    Game.PlayerHit();

                victim.InflictDamage(damageTaken, killingBlow);
                if (killingBlow)
                {
                    // The player died as a result.
                    string chatMessage;
                    Color messageColor;
                    if (shooterName == PlayerManager.LocalPlayerName)
                    {
                        // The shooter was this user.
                        chatMessage = "You destroyed " + victim.Name + "!";
                        messageColor = Color.LightGreen;
                    }
                    else if (victim.Name == PlayerManager.LocalPlayerName)
                    {
                        // The victim was this user.
                        chatMessage = "You were destroyed by " + shooterName + "!";
                        messageColor = Color.DarkOrange;
                        Game.buffbar.RemoveAllBuffs();
                        Game.Players.GetLocalPlayer().RemoveAllUtilities();
                        Game.StartRespawnTimer();
                    }
                    else
                    {
                        // The shooter and victim are unrelated.
                        chatMessage = victim.Name + " was destroyed by " + shooterName + ".";
                        messageColor = Color.Yellow;
                    }

                    ServiceManager.Game.Console.DebugPrint(
                         String.Format("[CHAT] {0}", chatMessage));

                    Game.Chat.AddMessage(chatMessage, messageColor);

                    Game.Scores.AddKill(shooterName);
                    Game.Scores.AddDeath(victim.Name);

                    victim.RemoveAssister(attacker.ID);
                    List<int> assisters = victim.GetAssisters();
                    for (int i = 0; i < assisters.Count; ++i)
                    {
                        PlayerTank helper = Game.Players[assisters[i]];
                        if (helper != null)
                        {
                            Game.Scores.AddAssist(helper.Name);
                        }
                    }

                    victim.ClearAssists();
                }
                else
                {
                    victim.AddAssist(attacker.ID);
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
            }
        }
        #endregion
    }
}
