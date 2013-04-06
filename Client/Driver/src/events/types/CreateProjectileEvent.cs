using Client.src.states.gamestate;
using Client.src.util;
using Client.src.util.game;
using Client.src.service;
using Renderer.SceneTools.Entities;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using Renderer.SceneTools.Entities.Particles;

namespace Client.src.events.types
{
    /// <summary>
    /// Event for adding projectiles to the game.
    /// </summary>
    public class CreateProjectileEvent : IEvent
    {
        #region Fields
        private ProjectileData type;
        private VTankObject.Point point;
        private int ownerId;
        private int projectileId;
        private int projectileTypeId;
        #endregion

        #region Constructors
        public CreateProjectileEvent(GamePlayState _game, int projectileTypeId, 
                                     VTankObject.Point point, int ownerId,
                                     int projectileId)
            : base(_game)
        {
            this.projectileTypeId = projectileTypeId;
            this.point = point;
            this.ownerId = ownerId;
            this.projectileId = projectileId;
        }
        #endregion

        #region Overrides
        public override void DoAction()
        {
            PlayerTank owner;
            if (!Game.Players.TryGetValue(ownerId, out owner))
            {
                // Player not found.
                Game.Players.RefreshPlayerList();
                return;
            }
            
            float angle = (float)Math.Atan2(owner.Position.Y - point.y, owner.Position.X - point.x);
            if (Game.LocalPlayer != owner)
            {
                new TurretSpinningEvent(Game, ownerId, angle, VTankObject.Direction.NONE).DoAction();
            }

            this.type = WeaponLoader.GetProjectile(projectileTypeId);

            if (this.type.Name == "Laser Beam")
            {
                Vector3 startPosition = owner.Turret.MountPosition("Emitter0");
                Vector3 endPosition = new Vector3((float)point.x, (float)point.y, startPosition.Z);
                Color color = Toolkit.GetColor(owner.Attributes.color);
                Game.Lazers.AddLazer(startPosition, endPosition, color);
                ProjectileManager.PlayProjectileSound(type, owner, angle);

                /*   
                MobileEmitter _emitter = new MobileEmitter(15000, new ParticleEmitter("LaserBeam"), startPosition, endPosition);
                _emitter.RenderID = ServiceManager.Scene.Add(_emitter, 3);Game.Projectiles[projectileId] = ProjectileManager.CreateProjectileObject(
                type, point, angle, owner);
                Projectile proj = Game.Projectiles[projectileId];

                ParticleEmitter p = new ParticleEmitter(type.ParticleEffect, proj.Position);
                proj.ParticleEmitter = p; */
            }
            else
            {
                Game.Projectiles[projectileId] = ProjectileManager.CreateProjectileObject(
                    type, point, angle, owner);
            }
           
        }
        #endregion
    }
}
