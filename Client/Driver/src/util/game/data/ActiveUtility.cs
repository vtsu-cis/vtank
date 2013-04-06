using System;
using System.Collections.Generic;
using System.Text;
using VTankObject;
using Network.Util;
using Renderer.SceneTools.Entities;

namespace Client.src.util.game
{
    /// <summary>
    /// Represents a utility currently on a tank.
    /// </summary>
    public class ActiveUtility
    {
        #region Fields
        public Utility utility;
        private ParticleEmitter emitter;
        private int duration;
        private long expirationTime;
        private long creationTime;
        #endregion

        #region Constructors
        /// <summary>
        /// Constructor for an ActiveUtility object
        /// </summary>
        /// <param name="utility">A VTankObject Utility</param>
        public ActiveUtility(Utility utility)
        {
            this.utility = utility;
            this.duration = (int)utility.duration;
            this.creationTime = Clock.GetTimeMilliseconds();
            this.expirationTime = this.creationTime + this.duration*1000;
            
        }

        #endregion

        #region Properties
        /// <summary>
        /// Indicates whether a utility has run its full duration.
        /// </summary>
        public bool IsExpired
        {
            get
            {
                if (Clock.GetTimeMilliseconds() > this.expirationTime)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// The emitter placed on the tank for the utility.
        /// </summary>
        public ParticleEmitter Emitter
        {
            get
            { return emitter; }
            set
            { emitter = value; }
        }

        #endregion
    }
}
