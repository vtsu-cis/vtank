using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Network.Util;
using Client.src.service;

namespace Client.src.util.game
{
    /// <summary>
    /// Class that describes a buff on a player's tank.  Buff is a GUI level class, it is responsible
    /// for tracking the buff's icon, duration/expiration and tooltip.
    /// </summary>
    public class Buff
    {
        #region Constants
        private readonly int fadeStart = 5; //Buff will begin fading with 5 seconds left.
        private readonly int millisecondsPerSecond = 1000;
        private readonly string pathToIcons = "textures\\misc\\buffs\\";
        #endregion

        #region Fields
        private Texture2D buffIcon;
        private int duration;
        private long creationTime;
        private long expirationTime;
        private string toolTip;

        private double opacity;
        private int fadeCounter;
        private FadeMode fadeMode;

        /// <summary>
        /// Indicates whether the buff is fading in or out.
        /// </summary>
        private enum FadeMode
        {
            In,
            Out
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Constructor for a buff
        /// </summary>
        /// <param name="buffIcon">Texture2D object for the icon.</param>
        /// <param name="duration">How long the buff lasts</param>
        /// <param name="toolTip">Tooltip info for the user.</param>
        public Buff(Texture2D buffIcon, int duration, string toolTip)
        {
            this.duration = duration;
            this.buffIcon = buffIcon;
            this.toolTip = toolTip;
            Initialize();
        }

        /// <summary>
        /// Constructor, takes a filename instead of the icon.
        /// </summary>
        /// <param name="fileName">The name of the image file.</param>
        /// <param name="duration">How long the buff lasts</param>
        /// <param name="toolTip">Tooltip info for the user.</param>
        public Buff(string fileName, int duration, string toolTip)
        {
            this.duration = duration;
            this.buffIcon = ServiceManager.Resources.GetTexture2D(pathToIcons + fileName);
            this.toolTip = toolTip;
            Initialize();
        }

        /// <summary>
        /// Load initial values.
        /// </summary>
        private void Initialize()
        {            
            this.creationTime = Clock.GetTimeMilliseconds();
            this.expirationTime = GetExpirationTime(duration, this.creationTime);
            this.fadeMode = FadeMode.Out;
            this.fadeCounter = 0;
            this.opacity = 1.0;
        }

        #endregion

        #region Properties
        /// <summary>
        /// The duratoin of this buff
        /// </summary>
        public int Duration 
        {
            get
            {
                return this.duration;
            }
            set
            {
                this.duration = value;
            }
        }

        /// <summary>
        /// Returns the remaining time on the buff.
        /// </summary>
        public int CountDown
        {
            get
            {
                long countD = ((expirationTime - Clock.GetTimeMilliseconds()) / millisecondsPerSecond);
                return (int)countD;
            }
        }

        /// <summary>
        /// The icon that this buff uses on the buffbar
        /// </summary>
        public Texture2D BuffIcon
        {
            get
            {
                return this.buffIcon;
            }
            set
            {
                this.buffIcon = value;
            }

        }

        /// <summary>
        /// Indicates whether a buff has run its full duration.
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
        /// Indicates whether the buff should be fading.
        /// </summary>
        public bool IsFading
        {
            get
            {
                if (Clock.GetTimeMilliseconds() > this.expirationTime - (this.fadeStart * millisecondsPerSecond))
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
        /// A helper property that is used as a timer to find out whether the opacity
        /// should be adjusted.
        /// </summary>
        private int FadeTimer
        {
            get
            {
                return (int)(Clock.GetTimeMilliseconds() / 100)% 10; // Tenths of seconds
            }
        }

        /// <summary>
        /// The Opacity of the buff bar -- how transparent it is.
        /// </summary>
        public byte Opacity
        {
            get
            {
                if (!this.IsFading)
                    return 255; //Max byte size, equivalent to 1.0 or full opacity
                else
                {
                    return this.GetOpacity();
                }
            }
        }
        #endregion

        #region Helpers
        /// <summary>
        /// Get the expiration time that this buff should be removed at
        /// </summary>
        /// <param name="duration">The duration of the buff</param>
        /// <param name="creationTime">The creation time of the buff</param>
        /// <returns>The expiration time</returns>
        private long GetExpirationTime(int duration, long creationTime)
        {
            return (creationTime + duration * millisecondsPerSecond);
        }

        /// <summary>
        /// Calculates the opacity value of a buff.  Uses a counter that ticks by tenths of seconds for its decrements/increments of opacity.
        /// </summary>
        /// <returns>The buff's opacity as a byte.</returns>
        private byte GetOpacity()
        {
            if (this.fadeMode == FadeMode.Out)
            {
                if (this.fadeCounter != this.FadeTimer)                
                    this.opacity -= .1;

                if (this.opacity <= 0)
                {
                    fadeMode = FadeMode.In;
                    this.opacity = 0.0;
                }

                this.fadeCounter = this.FadeTimer;
                return (byte)(this.opacity * 255);
                
            }
            else
            {
                if (this.fadeCounter != this.FadeTimer)                
                    this.opacity += .1;

                if (this.opacity >= 1.0)
                {
                    fadeMode = FadeMode.Out;
                    this.opacity = 1.0;
                }

                this.fadeCounter = this.FadeTimer;
                return (byte)(this.opacity * 255);
            }
        }

        #endregion
    }
}
