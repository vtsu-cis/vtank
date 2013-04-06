using System;
using System.Collections.Generic;
using Client.src.states.gamestate;
using Client.src.util;
using Microsoft.Xna.Framework.Graphics;
using Client.src.service;
using Renderer.SceneTools.Entities;
using Microsoft.Xna.Framework;

namespace Client.src.events.types
{
    public class PlayerDamagedEvent : IEvent
    {
        int victimId;
        int projectileId;
        int ownerId;
        int damageTaken;
        bool killingBlow;

        public PlayerDamagedEvent(GamePlayState _game, int victimId, int projectileId, int ownerId, 
            int damageTaken, bool killingBlow)
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
            string killerName = "<Unknown>";
            Vector3 killerPos = Vector3.Zero;
            int killerProjectileID = 0;
            bool weaponIsInstant = false;
            PlayerTank killer;
            if (Game.Players.ContainsKey(ownerId))
            {
                killer = Game.Players[ownerId];
                killerName = killer.Name;
                killerPos = killer.Position;
                killerProjectileID = killer.Weapon.Projectile.ID;
                weaponIsInstant = killer.Weapon.Projectile.IsInstantaneous;
            }

            try
            {
                if (!Game.Players.ContainsKey(victimId))
                {
                    // The player doesn't exist in our local copy: the tank list may have been de-synchronized.
                    Game.Players.RefreshPlayerList();
                }
                else
                {
                    PlayerTank victim = Game.Players[victimId];
                    string shooterName = killerName;
                    bool isChargeableWeapon = victim.Weapon.CanCharge;
                    if (weaponIsInstant)
                    {
                        VTankObject.Point position = new VTankObject.Point(victim.Position.X, victim.Position.Y);
                        float angle = (float)Math.Atan2((killerPos.Y - victim.Position.Y), 
                            (killerPos.X - victim.Position.X));

                        new CreateProjectileEvent(Game, killerProjectileID, position,
                            ownerId, projectileId).DoAction();
                    }

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

                        victim.RemoveAssister(ownerId);
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
                        victim.AddAssist(ownerId);
                    }
                }

                if (!weaponIsInstant && Game.Projectiles.ContainsKey(projectileId))
                {
                    Game.Projectiles.Remove(projectileId);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}
