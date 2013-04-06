/*!
    \file   PlayerManager.cs
    \brief  Manages players who are connected to the game server
    \author (C)Copyright 2009 by Vermont Technical College
*/
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Client.src.states.gamestate;
using Client.src.service;
using Renderer.SceneTools.Entities;

namespace Client.src.util
{
    /// <summary>
    /// The PlayerManager is responsible for holding the most current list of players.
    /// Additionally, the manager must provide lists and methods of deleting, adding, or
    /// modifying existing players in a thread-safe way.
    /// </summary>
    public class PlayerManager : IDictionary<int, PlayerTank>
    {
        public static string LocalPlayerName = "";
        #region Members

        public object playerLock;
        private int localClientId;
        
        private Dictionary<int, PlayerTank> players;

        #endregion

        #region Constructors

        /// <summary>
        /// Construct a new PlayerManager object.
        /// </summary>
        /// <param name="_localClientName">Name of the person playing with this client.</param>
        /// <param name="_camera">Camera, which is used in creating new TankObject types.</param>
        public PlayerManager()
        {
            playerLock = new object();
            players = new Dictionary<int,PlayerTank>();

            RefreshPlayerList();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Refresh the list of players. This will remove every player from the list
        /// (including the local player) and re-add them.
        /// </summary>
        public void RefreshPlayerList()
        {
            GameSession.Tank[] tankList = ServiceManager.Theater.GetPlayerList();

            Clear();

            foreach (GameSession.Tank tank in tankList)
            {
                if (tank.attributes.name == LocalPlayerName)
                {
                    localClientId = tank.id;
                }

                PlayerTank newTank = CreateTankObject(tank);
                //newTank.MeshColor = Utility.GetColor(tank.attributes.color);
                Add(tank.id, newTank);
            }

            ServiceManager.Scene.Access3D(
                GetLocalPlayer().TurretRenderID).TrackToCursor();
            
            try
            {
                ServiceManager.Game.Renderer.ActiveScene.CurrentCamera.
                    Follow(ServiceManager.Scene.Access3D(GetLocalPlayer().RenderID));
            }
            catch (Exception e)
            {
                ServiceManager.Game.Console.DebugPrint(
                    "Warning: Couldn't follow tank with camera: {0}", e);
            }


            ServiceManager.Game.Console.DebugPrint(
                "Player list refreshed, {0} players found.", players.Count);            
        }

        /// <summary>
        /// Gets the player who is running this instance of the player manager.
        /// </summary>
        /// <returns>The tank object representing the local player.</returns>
        public PlayerTank GetLocalPlayer()
        {
            if (players.Count == 0 || !players.ContainsKey(localClientId))
                return null;

            return this[localClientId];
        }

        /// <summary>
        /// Method helper for creating a known TankObject type from a VTankObject.Tank type.
        /// </summary>
        /// <param name="tank">Tank to convert.</param>
        /// <returns>Converted tank object.</returns>
        public static PlayerTank CreateTankObject(GameSession.Tank tank)
        {
            PlayerTank newTank = new PlayerTank(
                ServiceManager.Resources.GetModel("tanks\\" + tank.attributes.model),
                tank.attributes, 
                new Vector3((float)tank.position.x, (float)tank.position.y, -1f),
                tank.attributes.health > 0, (float)tank.angle, tank.id, tank.team);

            return newTank;
        }

        /// <summary>
        /// Returns a player based on their name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns>The player if found, null otherwise</returns>
        public PlayerTank GetPlayerByName(String name)
        {
            foreach (PlayerTank tank in players.Values)
            {
                if (tank.Name == name)
                {
                    return tank;
                }
            }

            return null;
        }
        #endregion

        #region IDictionary<string,TankObject> Members

        /// <summary>
        /// Add a player to the player list, overwriting any existing member
        /// whose key is also "key".
        /// </summary>
        /// <param name="key">Key of the tank. This is the tank's name. Should be unique.</param>
        /// <param name="value">The tank object stored.</param>
        public void Add(int key, PlayerTank value)
        {
            if (players.ContainsKey(key))
            {
                Remove(key);
            }

            if (value.RenderID != -1)
            {
                throw new Exception("Integrity error: Player already in renderer.");
            }

            int id = ServiceManager.Scene.Add(value, 2);
            value.RenderID = id;

            Microsoft.Xna.Framework.Graphics.Model turretModel =
                ServiceManager.Resources.GetModel(
                    "weapons\\" + value.Weapon.Model);
            Object3 turret = new Object3(turretModel, 
                Vector3.Zero); // The turret's position shouldn't matter since it's being
                               // attached to the tank.

            int turretId = ServiceManager.Scene.Add(turret, 0);
            
            if (value.HealthBar != null)
            {
                int healthBarId = ServiceManager.Scene.Add(value.HealthBar, 4);
                value.HealthBarRenderID = healthBarId;
            }
            
            if (value.NameObject != null)
            {
                int nameBarId = ServiceManager.Scene.Add(value.NameObject, 4);
                value.NameObjectRenderID = nameBarId;

            }

            turret.Attach(value, Constants.TURRET_MOUNT);
            value.SetTurret(turretId, turret);
            players.Add(key, value);
        }

        /// <summary>
        /// Test if a key resides in the player dictionary.
        /// </summary>
        /// <param name="key">Key (username) to look for.</param>
        /// <returns>True if the key exists.</returns>
        public bool ContainsKey(int key)
        {
            return players.ContainsKey(key);
        }

        /// <summary>
        /// Gets the keys (username list) of the players.
        /// </summary>
        public ICollection<int> Keys
        {
            get
            {
                return players.Keys;
            }
        }

        /// <summary>
        /// Remove a key from the player list.
        /// </summary>
        /// <param name="key">Player ID to remove.</param>
        /// <returns>True if the target was found and removed successfully.</returns>
        public bool Remove(int key)
        {
            if (players.ContainsKey(key))
            {
                PlayerTank tank = players[key];
                try
                {
                    ServiceManager.Scene.Delete(tank.RenderID);
                }
                catch (Exception e) 
                {
                    ServiceManager.Game.Console.DebugPrint(
                        "Warning: Scene.Delete error:\n  {0}", e);
                }

                try
                {
                    ServiceManager.Scene.Delete(tank.TurretRenderID);
                }
                catch (Exception e)
                {
                    ServiceManager.Game.Console.DebugPrint(
                        "Warning: Scene.Delete error:\n  {0}", e);
                }

                try
                {
                    if (tank.HealthBar != null)
                    {
                        ServiceManager.Scene.Delete(tank.HealthBarRenderID);
                    }
                    if (tank.NameObject != null)
                    {
                        ServiceManager.Scene.Delete(tank.NameObjectRenderID);
                    }
                }
                catch (Exception e)
                {
                    ServiceManager.Game.Console.DebugPrint(
                        "Warning: Scene.Delete error:\n  {0}", e);
                }

                return players.Remove(key);
            }

            return false;            
        }

        /// <summary>
        /// Try to get a value.
        /// </summary>
        /// <param name="key">Username to try to get.</param>
        /// <param name="value">Out object; the object where the value is stored.</param>
        /// <returns>True if the value exists.</returns>
        /// <see>Dictionary#TryGetValue(T key, out T value)</see>
        public bool TryGetValue(int key, out PlayerTank value)
        {
            return players.TryGetValue(key, out value);
        }

        /// <summary>
        /// Get a collection of values from the player dictionary.
        /// The values are TankObject types.
        /// </summary>
        public ICollection<PlayerTank> Values
        {
            get
            {
                 return players.Values;
            }
        }

        /// <summary>
        /// Indexer which allows convenient access to players. Allows both
        /// gets and sets.
        /// </summary>
        /// <param name="key">Username to grab.</param>
        /// <returns>TankObject value if the key exists.</returns>
        public PlayerTank this[int key]
        {
            get
            {
                if (!players.ContainsKey(key))
                {
                    return null;
                }

                return players[key];
            }
            set
            {
                 this.Add(key, value);
            }
        }

        #endregion

        #region ICollection<KeyValuePair<int,TankObject>> Members

        /// <summary>
        /// Add a player to the manager.
        /// </summary>
        /// <param name="item">Key-value pair to add.</param>
        public void Add(KeyValuePair<int, PlayerTank> item)
        {
            this.Add(item.Key, item.Value);
        }

        /// <summary>
        /// Clear the list, removing all players including the local player.
        /// </summary>
        public void Clear()
        {
            int[] keys = new int[players.Keys.Count];
            players.Keys.CopyTo(keys, 0);
            foreach (int id in keys)
            {
                try
                {
                    Remove(id);
                }
                catch (Exception) { }
            }

            // For good measure.
            players.Clear();            
        }

        /// <summary>
        /// Tests to see if the dictionary holds both a key and an object.
        /// </summary>
        /// <param name="item">Key-value pair to check.</param>
        /// <returns>True if the key/value pair exists.</returns>
        public bool Contains(KeyValuePair<int, PlayerTank> item)
        {
            return players.ContainsKey(item.Key) && players.ContainsValue(item.Value);
        }

        /// <summary>
        /// This method is not supported.
        /// </summary>
        public void CopyTo(KeyValuePair<int, PlayerTank>[] array, int arrayIndex)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Gets the number of elements in the player list.
        /// </summary>
        public int Count
        {
            get
            {
                return players.Count;
            }
        }

        /// <summary>
        /// Checks if the list is read-only. The list is not read-only, so this method
        /// will never return true.
        /// </summary>
        public bool IsReadOnly
        {
            get { return false; }
        }

        /// <summary>
        /// Remove a key-value pair. It only checks the key.
        /// </summary>
        /// <param name="item">Key-value pair to remove. Only the key is considered.</param>
        /// <returns>True if the client was removed successfully.</returns>
        public bool Remove(KeyValuePair<int, PlayerTank> item)
        {
            return Remove(item.Key);
        }

        #endregion

        #region IEnumerable<KeyValuePair<int,TankObject>> Members

        /// <summary>
        /// Get the enumerator for the dictionary.
        /// </summary>
        /// <returns>Enumerator for the player list.</returns>
        public IEnumerator<KeyValuePair<int, PlayerTank>> GetEnumerator()
        {
            return players.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        /// <summary>
        /// Get the enumerator for the player list.
        /// </summary>
        /// <returns>Non-generic enumerator.</returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return players.GetEnumerator();
        }

        #endregion
    }
}
