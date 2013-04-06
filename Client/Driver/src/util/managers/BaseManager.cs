using System;
using System.Collections.Generic;
using System.Text;
using Client.src.service;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Client.src.util
{
    /// <summary>
    /// Manager class that deals with bases in a Capture the Base game.
    /// </summary>
    public class BaseManager
    {
        #region Members
        private Dictionary<int, Base> bases;
        #endregion

        #region Constructors
        /// <summary>
        /// Generic constructor for Base Manager, bases are added later.
        /// </summary>
        public BaseManager()
        {
            bases = new Dictionary<int, Base>();
        }
        #endregion

        #region Properties
        
        /// <summary>
        /// Get the number of red bases.
        /// </summary>
        public int NumRedBases 
        {
            get
            {
                int count = 0;
                foreach (KeyValuePair<int, Base> _base in bases)
                {
                    if (_base.Value.BaseColor == GameSession.Alliance.RED)
                        count++;
                }

                return count;
            }
        }

        /// <summary>
        /// Get the number of blue bases.
        /// </summary>
        public int NumBlueBases 
        { 
            get
            {
                int count = 0;
                foreach (KeyValuePair<int, Base> _base in bases)
                {
                    if (_base.Value.BaseColor == GameSession.Alliance.BLUE)
                        count++;
                }

                return count;
            }
        }
        #endregion

        #region Methods

        /// <summary>
        /// Adds base to the game.
        /// </summary>
        /// <param name="newBase">The new base</param>
        public void AddBase (Base newBase)
        {
            bases.Add(newBase.EventId, newBase);
            bases[newBase.EventId].RenderID = ServiceManager.Scene.Add(bases[newBase.EventId], 0);

            if (bases.Count >= 6)
            {
                // Special case handler: if CTB mode, make player face next tower.
                Client.src.states.gamestate.State currentState = ServiceManager.StateManager.CurrentState;
                if (currentState is Client.src.states.gamestate.GamePlayState)
                {
                    PlayerTank localPlayer = ((Client.src.states.gamestate.GamePlayState)currentState).LocalPlayer;
                    const int BLUE_FRONT = 10;
                    const int RED_FRONT = 11;
                    Base blueBase = GetBase(BLUE_FRONT);
                    Base redBase = GetBase(RED_FRONT);

                    if (localPlayer.Team == GameSession.Alliance.RED)
                    {
                        localPlayer.Angle = (float)Math.Atan2(
                            blueBase.Position.Y - redBase.Position.Y,
                            blueBase.Position.X - redBase.Position.X);
                    }
                    else
                    {
                        localPlayer.Angle = (float)Math.Atan2(
                            redBase.Position.Y - blueBase.Position.Y,
                            redBase.Position.X - blueBase.Position.X);
                    }
                    Client.src.states.gamestate.GamePlayState game =
                        (Client.src.states.gamestate.GamePlayState)currentState;
                    game.Scene.LockCameras();
                }
            }
        }

        /// <summary>
        /// Adds a base to the game.
        /// </summary>
        /// <param name="color">Team color of the base.</param>
        /// <param name="eventId">Event ID of the base.</param>
        /// <param name="position">Position of the base.</param>
        public void AddBase (GameSession.Alliance color, int eventId, Vector3 position)
        {
            Model baseModel = ServiceManager.Resources.GetModel("events\\base");
            Base newBase = new Base(color, position, eventId, baseModel);

            AddBase(newBase);
        }

        /// <summary>
        /// Inflict damage to a base.  No checking is done to see if this is fatal damage, since 
        /// we should be informed of this by the server.
        /// </summary>
        /// <param name="eventId">The event id of the base.</param>
        /// <param name="damage">The amount of damage to inflict.</param>
        public void DamageBase(int eventId, int damage)
        {
            if (!bases.ContainsKey(eventId))
            {
                return;
            }

            bases[eventId].Health -= damage;
        }

        /// <summary>
        /// Set the health of a base to a new value.
        /// </summary>
        /// <param name="eventId">The event id of the base.</param>
        /// <param name="health">The new health of the base.</param>
        public void SetBaseHealth(int eventId, int health, GameSession.Alliance baseColor)
        {
            if (!bases.ContainsKey(eventId))
            {
                return;
            }

            if (bases[eventId].Destroyed == true && health > 0)
            {
                bases[eventId].Destroyed = false;
            }

            if (health == 0)
            {
                bases[eventId].Destroyed = true;
            }

            if (bases[eventId].BaseColor != baseColor)
            {
                bases[eventId].BaseColor = baseColor;
            }

            bases[eventId].Health = health;
            this.UpdateInConflictStatuses();
        }

        /// <summary>
        /// Destroy the base of a given event id.
        /// </summary>
        /// <param name="eventId">Event Id of the base.</param>
        public void DestroyBase(int eventId)
        {
            if (!bases.ContainsKey(eventId))
            {
                return;
            }

            if (bases[eventId].Health > 0)
            {
                bases[eventId].Health = 0;
            }

            bases[eventId].Destroyed = true;

        }
        
        /// <summary>
        /// One team has captured the base, so it will swap possession.
        /// </summary>
        /// <param name="eventId">Event id of the base.</param>
        /// <param name="newColor">The color of the new owner.</param>
        public void CaptureBase(int eventId, GameSession.Alliance newColor)
        {
            if (!bases.ContainsKey(eventId))
            {
                return;
            }

            bases[eventId].CaptureBase(newColor);

            foreach (Base _base in bases.Values)
            {
                _base.ResetToFullHealth();
            }

            this.UpdateInConflictStatuses();
        }

        /// <summary>
        /// Remove a base from bases and the renderer.
        /// </summary>
        /// <param name="eventId">Event id of the base.</param>
        public void RemoveBase(int eventId)
        {
            if (!bases.ContainsKey(eventId))
            {
                return;
            }

            ServiceManager.Scene.Delete(bases[eventId].RenderID);
            bases.Remove(eventId);
        }

        /// <summary>
        /// Do the per-cycle maintenance.
        /// </summary>
        public void Update()
        {
            foreach (Base _base in bases.Values)
            {
                _base.SetEmitters();
                _base.RotateObjects();
            }
        }

        /// <summary>
        /// Get a base from the collection
        /// </summary>
        /// <param name="baseId">The ID of the base</param>
        /// <returns>The base</returns>
        public Base GetBase(int baseId)
        {
            if (bases.ContainsKey(baseId))
                return bases[baseId];
            else
                return null;
        }

        /// <summary>
        /// Reset bases to their original (as per the map) owner.
        /// </summary>
        public void ResetBases()
        {
            foreach (Base _base in bases.Values)
            {
                _base.ResetBaseToOriginalOwner();
            }

            this.UpdateInConflictStatuses();
        }

        /// <summary>
        /// Update the in-conflict status of the bases.  Two adjacent bases of opposing teams
        /// are deemed 'in conflict'.
        /// </summary>
        public void UpdateInConflictStatuses()
        {
            Base currentBase = null;
            Base previousBase = null;
            int lowest = 9000; // Arbitrary large number 
            Dictionary<int, Base> tempBases = new Dictionary<int, Base>();

            Queue<Base> basesQueue = new Queue<Base>();

            foreach (Base _base in bases.Values)
            {
                tempBases.Add(_base.EventId, _base);
            }

            //Sort bases by lowest -> highest and put them in the queue
            foreach (Base _base in bases.Values)
            {
                foreach (Base __base in tempBases.Values)
                {
                    if (__base.EventId < lowest)
                        lowest = __base.EventId;
                }

                bases[lowest].InConflict = false;
                basesQueue.Enqueue(bases[lowest]);
                tempBases.Remove(lowest);
                lowest = 9000;
            }

            //Iterate through queue, set adjacent bases as in conflict and break.
            for (int count = 0; count < bases.Count; count++)
            {
                if (count == 0)
                {
                    previousBase = basesQueue.Dequeue();
                    currentBase = basesQueue.Dequeue();
                }

                if (previousBase.BaseColor != currentBase.BaseColor)
                {
                    bases[previousBase.EventId].InConflict = true;
                    bases[currentBase.EventId].InConflict = true;
                    break;
                }

                if (basesQueue.Count == 0)
                    break;

                previousBase = currentBase;
                currentBase = basesQueue.Dequeue();
            }

        }

        /// <summary>
        /// Get the enumerator for the bases collection.
        /// </summary>
        /// <returns>The enumerator.</returns>
        public IEnumerator<Base> GetEnumerator()
        {
            return bases.Values.GetEnumerator();
        }
        #endregion
    }
}
