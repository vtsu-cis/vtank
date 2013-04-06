/*!
    \file   HUD.cs
    \brief  Displays the health of the tanks
    \author (C) Copyright 2008 by Vermont Technical College
*/

using System;
using System.Collections.Generic;

using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Client.src.service;
using Client.src.service.services;
using Client.src.states.gamestate;
using Network.Util;

namespace Client.src.util.game
{
    //TODO 
    // - Implement Error Checking for input values
    // - Allow the user to change the orientation of the 
    //      text instead of moving according to orientation
    // - Comment a lot more
    // - Write Test Cases
    // - Provide some sort of API/Documentation
    // - Change the way it currently handles input 
    //      textures to be more general
    // - Animations - smooth decrease, opacity changes during fighting
    // - implement weapon cooldown
    public class HUD
    {
        #region Fields
        int numBars = 1;
        HUD_Bar[] bars;
        public HUD_Bar[] Bars { get { return bars; } }
        #endregion

        #region Properties
        /// <summary>
        /// Returns whether this HUD has an overheat bar.
        /// </summary>
        public bool HasOverHeatBar
        {
            get;
            private set;
        }

        /// <summary>
        /// Returns whether this HUD has a charge bar.
        /// </summary>
        public bool HasChargeBar
        {
            get;
            private set;
        }

        /// <summary>
        /// Returns the number of bars this HUD has.
        /// </summary>
        public int NumBars { get { return numBars; } }
        #endregion

        #region Constructors
        /// <summary>
        /// Default Constructor. Creates a HUD with one bar on the left side
        /// </summary>
        public HUD()
            : this(1)
        {
        }

        /// <summary>
        /// Constructor: Creates a HUD with a specified number of bars
        /// </summary>
        public HUD(int _numBars)
        {
            numBars = _numBars;
            bars = new HUD_Bar[NumBars];

            for (int i = 0; i < bars.Length; i++)
            {
                bars[i] = HUD_Bar.getDefaultHUD_Bar();
            }

            LoadContent();
        }
        #endregion

        #region Initializing
        /// <summary>
        /// Loads the default font and textures for the bars
        /// </summary>
        protected void LoadContent()
        {
            for (int i = 0; i < bars.Length; i++)
            {
                bars[i].Outline = ServiceManager.Resources.GetTexture2D(
                    "textures\\misc\\HUD\\EmptyT2");
                bars[i].Filler = ServiceManager.Resources.GetTexture2D(
                    "textures\\misc\\HUD\\WhiteT");
            }

        }

        /// <summary>
        /// Returns a default HUD for the VTank Client. Two scaled bars: one left, one right.
        /// Detects whether the player is using a weapon that requires a charge bar or a
        /// overheat bar instead of the default cooldown bar.
        /// </summary>
        public static HUD GetHudForPlayer(PlayerTank player)
        {
            HUD _hud = new HUD(2);
            _hud.Bars[0].BarColor = new Color(0.1f, 1.0f, 0.2f); //Green Health Bar
            _hud.Bars[0].Opacity = 0.1f;
            _hud.Bars[1].Opacity = 0.1f;

            _hud.Bars[0].Scale = .5f;
            _hud.Bars[1].Scale = .5f;

            _hud.Bars[0].Origin = new Vector2(0, 0);

            _hud.Bars[0].TextEnabled = false;
            _hud.Bars[1].TextEnabled = false;

            _hud.Bars[1].Orientation = HUD_Bar.HUDOrientation.RIGHT;
            _hud.HasChargeBar = false;
            _hud.HasOverHeatBar = false;

            if (player.Weapon.CanCharge)
            {
                _hud.Bars[1].BarColor = new Color(0.0f, 0.0f, 1.0f); //Blue cooldown bar
                _hud.HasChargeBar = true;
            }
            else if (player.Weapon.UsesOverHeat)
            {
                _hud.Bars[1].BarColor = new Color(1.0f, 0.0f, 0.0f); //Red cooldown bar   
                _hud.HasOverHeatBar = true;
            }
            else
            {
                _hud.Bars[1].BarColor = new Color(1.0f, 1.0f, 0.0f); //Yellow cooldown bar
                _hud.Bars[1].Opacity = 0.1f;
            }

            return _hud;

        }
        #endregion

        #region Methods
        #endregion       

        /// <summary>
        /// Updates the bars
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public void Update()
        {
            for (int i = 0; i < bars.Length; i++)
            {
                bars[i].Update();
            }

        }

        /// <summary>
        /// Handles the drawing of the HUD
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public void Draw()
        {
            for (int i = 0; i < bars.Length; i++)
            {
                bars[i].Draw();
            }
        }

        /// <summary>
        /// Update the health on the player's health bar
        /// </summary>
        /// <param name="player">The player</param>
        public void UpdateHealth(PlayerTank player)
        {
            this.Bars[0].Value = (int)MathHelper.Clamp((player.Health * 100 / Constants.MAX_PLAYER_HEALTH), 0f, 100f);
        }

        /// <summary>
        /// Update the secondary bar of the player.  Checks what type of bar we're
        /// dealing with and uses the appropriate method.
        /// </summary>
        /// <param name="player">The player</param>
        public void UpdateSecondaryBar(PlayerTank player)
        {
            if (this.HasChargeBar)
            {
                this.UpdateChargeBar(player);   
            }
            else if (this.HasOverHeatBar)
            {
                this.UpdateOverheatBar(player);                
            }
            else
            {
                this.UpdateCooldownBar(player);                
            }
        }

        /// <summary>
        /// Update a cooldown bar to its current status
        /// </summary>
        /// <param name="player">The player</param>
        private void UpdateCooldownBar(PlayerTank player)
        {
            if (player.GetRateOfFire() > 0)
                this.Bars[1].Value = (int)MathHelper.Clamp((1f - (player.Remaining /
                    (player.Weapon.Cooldown * (player.Weapon.Cooldown * player.GetRateOfFire())))) * 100f, 0f, 100f);
            else
                this.Bars[1].Value = (int)MathHelper.Clamp((1f - (player.Remaining /
                    player.Weapon.Cooldown)) * 100f, 0f, 100f);
        }

        /// <summary>
        /// Update a charge bar to the current charge status
        /// </summary>
        /// <param name="player"></param>
        private void UpdateChargeBar(PlayerTank player)
        {
            if (player.IsCharging)
            {
                float rateOfFire = player.GetRateOfFire();
                float chargeAmount;
                long delta = Clock.GetTimeMilliseconds() - player.TimeChargingBegan;
                long timeCharging;
                if (rateOfFire > 0f)
                {
                    timeCharging = delta + (long)(delta * rateOfFire);
                }
                else
                {
                    timeCharging = delta;
                }

                if (Clock.GetTimeMilliseconds() - player.TimeChargingBegan >= player.Weapon.MaxChargeTime * 1000)
                    chargeAmount = 100f;
                else
                {
                    if (timeCharging == 0)
                    {
                        this.Bars[1].StartPulsing(255, 255, 255);
                        this.Bars[1].PulseIncrement = 15;
                        timeCharging = 1;
                    }

                    chargeAmount = (timeCharging / (player.Weapon.MaxChargeTime * 1000)) * 100f;
                }

                this.Bars[1].Value = (int)MathHelper.Clamp(chargeAmount, 0f, 100f);
            }
            else
            {
                this.Bars[1].Value = (int)MathHelper.Clamp(0f, 0f, 100f);
                if (this.Bars[1].IsPulsing)
                {
                    this.Bars[1].StopPulsing();
                    this.Bars[1].PulseIncrement = 7;
                }
            }
        }

        /// <summary>
        /// Update an overheat bar to its current overheat value
        /// </summary>
        /// <param name="player"></param>
        private void UpdateOverheatBar(PlayerTank player)
        {
            if (player.Weapon.OverheatAmount > player.Weapon.OverheatTime)
                player.Weapon.IsOverheating = true;

            if (player.Weapon.IsOverheating)
            {
                if (player.Weapon.OverheatAmount <= 0)
                {
                    player.Weapon.IsOverheating = false;
                }
            }

            if (player.Weapon.OverheatAmount > 0 && player.TimeSinceFiring > player.Weapon.OverheatRecoveryStartTime)
            {
                player.Weapon.OverheatAmount -= (float)((player.Weapon.OverheatTime / player.Weapon.OverheatRecoverySpeed) *
                        ServiceManager.Game.DeltaTime);
            }

            this.Bars[1].Value = (int)MathHelper.Clamp((player.Weapon.OverheatAmount / player.Weapon.OverheatTime) * 100,
                0f,
                100f);
        }
    }
}
