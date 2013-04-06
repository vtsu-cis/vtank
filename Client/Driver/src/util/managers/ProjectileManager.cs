/*!
    \file   ProjectileManager.cs
    \brief  Manages projectiles in VTank
    \author (C)Copyright 2009 by Vermont Technical College
*/
using System;
using System.Collections.Generic;

using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Client.src.util.game;
using Client.src.service;
using Client.src.states.gamestate;
using Renderer;
using Renderer.SceneTools.Entities;
using Renderer.SceneTools.Entities.Particles;


namespace Client.src.util
{
    /// <summary>
    /// The ProjectileManager is in charge of adding, updating, deleting, drawing, and overall
    /// state management of in-game projectiles. The manager is thread-safe.
    /// </summary>
    public class ProjectileManager : IDictionary<int, Projectile>
    {
        #region Members
        private struct ProjectileToAddLater
        {
            public long timestamp;
            public GameSession.ProjectileDamageInfo projInfo;
            public float angle; //Need this in case they turn the turret during the interval.
            public PlayerTank owner;
        }

        public static float MISSILE_SCALE = 0.1f;
        private static Dictionary<int, string> soundEffectBank;
        private static Dictionary<int, Audio.SoundCue> activeSounds;

        private List<ProjectileToAddLater> projectilesToAddLater;
        
        private Dictionary<int, Projectile> projectiles;
        #endregion

        #region Constructor
        /// <summary>
        /// Create a ProjectileManager.
        /// </summary>
        /// <param name="_camera">The camera is used to draw the objects.</param>
        public ProjectileManager()
        {
            projectiles = new Dictionary<int, Projectile>();
            projectilesToAddLater = new List<ProjectileToAddLater>();
            activeSounds = new Dictionary<int, Audio.SoundCue>();

            if (soundEffectBank == null)
            {
                soundEffectBank = new Dictionary<int, string>();

                List<Weapon> weapons = WeaponLoader.GetWeaponsAsList();
                foreach (Weapon weapon in weapons)
                {
                    if (!soundEffectBank.ContainsKey(weapon.ProjectileID))
                    {
                        soundEffectBank[weapon.ProjectileID] = weapon.FiringSound;
                    }
                }
            }
        }
        #endregion

        #region Methods

        public static void PlayProjectileSound(ProjectileData data, PlayerTank owner, float angle)
        {
            Vector3 soundPos = new Vector3((float)owner.Position.X, (float)owner.Position.Y, 0);
            Vector3 velocity = new Vector3((float)((data.TerminalVelocity + 50) * Math.Cos(angle)),
                (float)(data.TerminalVelocity * Math.Sin(angle)), 0);

            if (soundEffectBank.ContainsKey(data.ID))
            {
                try
                {
                    if (owner.ActiveSound != null)
                    {
                        if (ServiceManager.AudioManager.SoundIsPlaying(owner.ActiveSound) && owner.Weapon.Name == "Flamethrower")
                            return;
                    }

                    GamePlayState state = (GamePlayState)ServiceManager.StateManager.CurrentState;
                   owner.ActiveSound = ServiceManager.AudioManager.Play3DSound(owner.Weapon.FiringSound,
                        state.Players.GetLocalPlayer().Position, soundPos, velocity, false);

                }
                catch (Exception e)
                {
                    ServiceManager.Game.Console.DebugPrint(
                        "Warning: Current state isn't a game: {0}", e);
                }
            }
            else
            {
                if (data != null)
                    ServiceManager.Game.Console.DebugPrint(
                        "Warning: Couldn't find sound effect for projectile #{0}.",
                        data.ID);
                else
                    ServiceManager.Game.Console.DebugPrint("Warning:  ProjectileManager received a null projectile");
            }
        }

        /// <summary>
        /// Create a missile object from the projectile data received from the server.
        /// </summary>
        /// <param name="data">Data to convert.</param>
        /// <returns>New missile object.</returns>
        public static Projectile CreateProjectileObject(
            ProjectileData data, VTankObject.Point point, float angle, PlayerTank owner)
        {
            Vector3 soundPos = new Vector3((float)owner.Position.X, (float)owner.Position.Y, 0);
            Vector3 velocity = new Vector3((float)((data.InitialVelocity + 50) * Math.Cos(angle)),
                (float)(data.InitialVelocity * Math.Sin(angle)), 0);

            if (soundEffectBank.ContainsKey(data.ID))
            {
                try
                {
                    GamePlayState state = (GamePlayState)ServiceManager.StateManager.CurrentState;
                    if ( ! String.IsNullOrEmpty(owner.Weapon.FiringSound))
                    {
                        ServiceManager.AudioManager.Play3DSound(owner.Weapon.FiringSound,
                            state.Players.GetLocalPlayer().Position, soundPos, velocity, false);
                    }
                }
                catch (Exception e)
                {
                    ServiceManager.Game.Console.DebugPrint(
                        "Warning: Current state isn't a game: {0}", e);
                }
            }
            else
            {
                if (data != null)
                    ServiceManager.Game.Console.DebugPrint(
                        "Warning: Couldn't find sound effect for projectile #{0}.", 
                        data.ID);
                else
                    ServiceManager.Game.Console.DebugPrint("Warning:  ProjectileManager received a null projectile");
            }

            const float inflation = -0.01f;
            float alive = (float)(data.Range) / (float)(data.TerminalVelocity);
            Projectile projectile = new Projectile(owner.Position, point,
                angle, data.InitialVelocity, alive + inflation, owner, data);

            if (!String.IsNullOrEmpty(owner.Weapon.MuzzleEffectName))
            {
                ParticleEmitter p = new ParticleEmitter(owner.Weapon.MuzzleEffectName, projectile.Position);
                p.Attach(owner.Turret, "Emitter0");
                p.MimicRotation(owner.Turret);
                ServiceManager.Scene.Add(p, 3);
            }

            /*
            if(projectile.Data.IsInstantaneous)
            {
                if (!String.IsNullOrEmpty(projectile.Data.ParticleEffect))
                {
                    ParticleEmitter p = new ParticleEmitter(projectile.Data.ParticleEffect, projectile.Position);
                    projectile.ParticleEmitter = p;
                    p.MimicPosition(projectile, Vector3.Zero);
                    ServiceManager.Scene.Add(p, 3);
                }
            }*/

            // TODO: Z-index should be transformed to be attached to the turret end.
            return projectile;
        }

        /// <summary>
        /// Add any projectiles that are in the 'add later' collection.
        /// </summary>
        public void AddDelayedProjectiles()
        {
            if (projectilesToAddLater.Count == 0)
                return;

            List<ProjectileToAddLater> toRemove = new List<ProjectileToAddLater>();

            foreach (ProjectileToAddLater proj in projectilesToAddLater)
            {
                if (proj.timestamp <= Network.Util.Clock.GetTimeMilliseconds())
                {
                    this.Add(proj.projInfo.projectileId, CreateProjectileObject(
                        WeaponLoader.GetProjectile(proj.projInfo.projectileTypeId),
                        proj.projInfo.target, proj.angle, proj.owner));
                    proj.owner.TurretAngle = -proj.angle;
                    toRemove.Add(proj);
                }
            }

            foreach (ProjectileToAddLater proj in toRemove)
            {
                projectilesToAddLater.Remove(proj);
            }
        }

        /// <summary>
        /// Add a projectile to the 'add later' collection, to be added to the game after a certain
        /// amount of time.
        /// </summary>
        /// <param name="projInfo">The projectile's 'add later' info.</param>
        public void AddLater(GameSession.ProjectileDamageInfo projInfo, PlayerTank owner)
        {

            ProjectileToAddLater delayedProj = new ProjectileToAddLater();
            delayedProj.owner = owner;
            delayedProj.timestamp = (Network.Util.Clock.GetTimeMilliseconds() + projInfo.spawnTimeMilliseconds);
            delayedProj.projInfo = projInfo;

            delayedProj.angle = (float)Math.Atan2(owner.Position.Y - projInfo.target.y,
                owner.Position.X - projInfo.target.x);

            projectilesToAddLater.Add(delayedProj);
        }

        /// <summary>
        /// Check to see if any projectiles have hit a wall.
        /// </summary>
        /// <param name="tiles">Current list of background tiles for the map.</param>
        public void CheckCollisions(Map map, DrawableTile[] tiles)
        {
            if (projectiles.Count == 0)
                return;
            
            int index = 0;
            int[] toRemove = new int[projectiles.Count];
            for (int i = 0; i < toRemove.Length; i++)
                toRemove[i] = -1;

            foreach (KeyValuePair<int, Projectile> projectile in projectiles)
            {
                if (projectile.Value.Elapsed >= projectile.Value.MaximumTimeAlive && 
                        !projectile.Value.Owner.Weapon.HasFiringArc)
                {
                    toRemove[index++] = projectile.Key;
                    continue;
                }

                if (MissileCollidedWithWall(projectile.Value, map, tiles))
                {
                    if (!String.IsNullOrEmpty(projectile.Value.Data.ImpactParticleName))
                    {
                        /*if (missile.Data.AreaOfEffectRadius > 0)
                        {
                            ParticleEmitter emitter = new ParticleEmitter(missile.Data.ImpactParticleName);
                            ParticleEmitterSettings emitterSettings = emitter.Settings;
                            emitterSettings.Radius = missile.Data.AreaOfEffectRadius;
                            ParticleEmitter _emitter = new ParticleEmitter(emitterSettings);
                            _emitter.Position = missile.Position;
                            ServiceManager.Scene.Add(_emitter, 3);
                        }
                        else*/
                            ServiceManager.Scene.AddParticleEmitterAtPosition(
                                projectile.Value.Data.ImpactParticleName,
                                projectile.Value.Position);
                    }
                    toRemove[index++] = projectile.Key;
                }
            }

            foreach (int key in toRemove)
            {
                if (key < 0)
                    continue;

                Remove(key);
            }
        }

/*
        /// <summary>
        /// Handles effects that trigger when colliding with certain objects
        /// and players.
        /// </summary>
        /// <param name="missile">The projectile being handled.</param>
        private void HandleEnvironmentCollisionEffects(Projectile missile)
        {
            if (!String.IsNullOrEmpty(missile.Data.ImpactParticleName))
            {
                ServiceManager.Scene.AddParticleEmitterAtPosition(missile.Data.ImpactParticleName,
                        missile.Position);
            }

            if (missile.Data.EnvProperty != null)
            {
                if (missile.Data.EnvProperty.TriggersUponImpactWithEnvironment)
                {
                    ServiceManager.Scene.AddParticleEmitterAtPosition(missile.Data.EnvProperty.ParticleEffectName, 
                        missile.Position);
                }
            }
        }

        /// <summary>
        /// Handles effects when a projectile collides with a player.
        /// </summary>
        /// <param name="projectileID">ID of the projectile colliding.</param>
        public void HandlePlayerCollisionEffects(int projectileID)
        {
            Projectile missile;

            if (this.projectiles.ContainsKey(projectileID))
            {
                missile = this.projectiles[projectileID];

                if (!String.IsNullOrEmpty(missile.Data.ImpactParticleName))
                {
                    ServiceManager.Scene.AddParticleEmitterAtPosition(missile.Data.ImpactParticleName,
                            missile.Position);
                }

                if (missile.Data.EnvProperty != null)
                {
                    if (missile.Data.EnvProperty.TriggersUponImpactWithPlayer)
                    {
                        ServiceManager.Scene.AddParticleEmitterAtPosition(missile.Data.EnvProperty.ParticleEffectName, 
                            missile.Position);
                    }
                }
            }
        }*/

        /// <summary>
        /// Test if a missile collided with a wall.
        /// </summary>
        /// <param name="missile">Missile to test.</param>
        /// <param name="tiles">List of tiles on the map.</param>
        /// <returns>True if the missile suffered a collision.</returns>
        private bool MissileCollidedWithWall(Projectile missile, Map map, DrawableTile[] tiles)
        {
            int baseX = (int)(missile.Position.X / Constants.TILE_SIZE);
            int baseY = (int)(-missile.Position.Y / Constants.TILE_SIZE);

            int minimumX = baseX - 1;
            int minimumY = baseY - 1;
            int maximumX = baseX + 1;
            int maximumY = baseY + 1;

            int height = (int)map.Height;
            int width = (int)map.Width;
            for (int y = minimumY; y < height && y <= maximumY; y++)
            {
                if (y < 0)
                    continue;

                for (int x = minimumX; x < width && x <= maximumX; x++)
                {
                    if (x < 0)
                        continue;

                    DrawableTile tile = tiles[y * width + x];
                    if (!tile.Passable && tile.BoundingBox.Intersects(missile.BoundingSphere))
                    {
                        return true;
                    }
                    else if (missile.Position.Z <= 0)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        #endregion

        #region IDictionary<int,Projectile> Members

        /// <summary>
        /// Add a projectile to the manager.
        /// </summary>
        /// <param name="key">ID of the projectile.</param>
        /// <param name="value">Projectile data to track.</param>
        public void Add(int key, Projectile value)
        {
            if (projectiles.ContainsKey(key))
            {
                Remove(key);
            }

            projectiles.Add(key, value);
        }        

        /// <summary>
        /// Check if a projectile item exists in the manager.
        /// </summary>
        /// <param name="key">ID to look for.</param>
        /// <returns>True if the ID is in the manager.</returns>
        public bool ContainsKey(int key)
        {
                return projectiles.ContainsKey(key);
        }

        /// <summary>
        /// Gets the key values (integer list) of the projectiles.
        /// </summary>
        public ICollection<int> Keys
        {
            get
            {
                    return projectiles.Keys;
             }
        }

        /// <summary>
        /// Remove a projectile forcefully from the list.
        /// </summary>
        /// <param name="key">ID of the projectile.</param>
        /// <returns>True if the projectile was found and successfully removed.</returns>
        public bool Remove(int key)
        {
            if (!projectiles.ContainsKey(key))
            {
                return false;
            }

            Projectile deadProjectile = projectiles[key];

            deadProjectile.Dispose();

            return projectiles.Remove(key);
        }

        /// <summary>
        /// Try to get a projectile value.
        /// </summary>
        /// <param name="key">ID of the projectile.</param>
        /// <param name="value">Out value, where the result is stored.</param>
        /// <returns>True if the attempt was successful.</returns>
        public bool TryGetValue(int key, out Projectile value)
        {
            bool success = projectiles.TryGetValue(key, out value);

            if (success == false)
                Console.WriteLine("Failed to get ID of projectile: " + success);

            return success;
        }

        /// <summary>
        /// Get a list of the objects inside of the manager.
        /// </summary>
        public ICollection<Projectile> Values
        {
            get
            {
                    return projectiles.Values;
            }
        }

        /// <summary>
        /// Indexer for missile objects. Gets or sets the value of 'key'.
        /// </summary>
        /// <param name="key">ID of a projectile.</param>
        public Projectile this[int key]
        {
            get
            {
                    return projectiles[key];
            }
            set
            {
                    this.Add(key, value);
            }
        }

        #endregion

        #region ICollection<KeyValuePair<int,Projectile>> Members

        /// <summary>
        /// Add a key-value pair to the projectile list.
        /// </summary>
        /// <param name="item">Item to add.</param>
        public void Add(KeyValuePair<int, Projectile> item)
        {
            this.Add(item.Key, item.Value);
        }

        /// <summary>
        /// Clear the list of projectiles, removing every entry.
        /// </summary>
        public void Clear()
        {
                projectiles.Clear();
        }

        /// <summary>
        /// Check if the projectile list contains both a given key and a given value.
        /// </summary>
        /// <param name="item">Key-value pair to check.</param>
        /// <returns>True if the pair is contained.</returns>
        public bool Contains(KeyValuePair<int, Projectile> item)
        {
                return projectiles.ContainsKey(item.Key) && projectiles.ContainsValue(item.Value);
         }

        /// <summary>
        /// This method is not supported.
        /// </summary>
        public void CopyTo(KeyValuePair<int, Projectile>[] array, int arrayIndex)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Count the number of active projectiles.
        /// </summary>
        public int Count
        {
            get
            {
                    return projectiles.Count;
            }
        }

        /// <summary>
        /// Check if the manager is read only. This is never true.
        /// </summary>
        public bool IsReadOnly
        {
            get { return false; }
        }

        /// <summary>
        /// Remove a projectile from the list. Only the key is considered.
        /// </summary>
        /// <param name="item">Key-value pair to remove.</param>
        /// <returns>True if the item was found and removed.</returns>
        public bool Remove(KeyValuePair<int, Projectile> item)
        {
            return this.Remove(item.Key);
        }

        #endregion

        #region IEnumerable<KeyValuePair<int,Projectile>> Members

        /// <summary>
        /// Get a generic enumerator.
        /// </summary>
        /// <returns>Generic enumerator.</returns>
        public IEnumerator<KeyValuePair<int, Projectile>> GetEnumerator()
        {
                return projectiles.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        /// <summary>
        /// Get a non-generic enumerator for the projectile list.
        /// </summary>
        /// <returns>Non-generic enumerator.</returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
                return projectiles.GetEnumerator();
        }

        #endregion
    }
}
