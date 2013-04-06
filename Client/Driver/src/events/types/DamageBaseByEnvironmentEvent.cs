using System;
using System.Collections.Generic;
using System.Text;
using Client.src.states.gamestate;
using Microsoft.Xna.Framework.Graphics;
using Client.src.util;
using Client.src.util.game;

namespace Client.src.events.types
{
    public class DamageBaseByEnvironmentEvent : IEvent
    {
        #region Members
        GameSession.Alliance baseColor;
        int baseId;
        int environmentEffectId;
        int damage;
        bool isDestroyed;
        #endregion

        #region Constructors
        public DamageBaseByEnvironmentEvent(GamePlayState _game, GameSession.Alliance baseColor, int baseId, int envId,
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
            PlayerTank owner = null;
            EnvironmentProperty envprop = Game.EnvironmentEffects.GetEffect(environmentEffectId);

            if (envprop != null)
            {
                owner = envprop.Owner;
            }

            if (this.isDestroyed)
            {
                if (owner != null)
                {
                    string message = owner.Name + " destroyed the " +
                    Game.Bases.GetBase(baseId).BaseColor.ToString().ToLower() + " base!";
                    Game.Chat.AddMessage(message, Color.Chartreuse);
                }

                Game.Bases.DestroyBase(baseId);
                
            }
            else
            {
                Game.Bases.DamageBase(baseId, damage);
            }
        }
        #endregion
    }
}
