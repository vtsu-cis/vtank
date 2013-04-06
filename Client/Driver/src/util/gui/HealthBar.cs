/*!
    \file   HealthBar.cs
    \brief  Displays the health of the tanks
    \author (C) Copyright 2008 by Vermont Technical College
*/
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Client.src.service;
using Renderer.SceneTools.Entities;
using Renderer;

namespace Client.src.util.game
{
    /// <summary>
    /// HealthBar shows the current health of the tank
    /// It is situated above the tank and decreases
    /// as the tank gets hit or increase if the tank
    /// picks up health packs
    /// </summary>
    public class HealthBar : VertexColorGroup
    {
        #region Members
        private int health;
        private Color color;
        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="_position">Position of the Health Bar</param>
        /// <param name="_camera">Current Game Camera</param>
        public HealthBar(Vector3 _position, int _health)
        {
            position = _position;
            Health = _health;

            AdjustVertices();
        }

        /// <summary>
        /// Initalizes the health bar setting
        /// the bar at a certain position and 
        /// the color of the bar to green
        /// </summary>
        private void AdjustVertices()
        {
            float offset = 125 * Constants.UI_SCALE;

            vertices.Clear();
            Matrix camWorld = Matrix.Invert(GraphicOptions.CurrentCamera.View);
            Vector3 right = camWorld.Right;
            Vector3 down = camWorld.Down;
            right.Normalize(); down.Normalize();
            right = right * (health * 100 / Constants.MAX_PLAYER_HEALTH);
            Vector3 ps = new Vector3(position.X, position.Y, position.Z + 75);
            Vector3 tl = ps - right / 2;
            Vector3 tr = ps + right / 2;
            Vector3 bl = tl + down * 5;
            Vector3 br = tr + down * 5;

            vertices.Add(new VertexPositionColor(tl, color));
            vertices.Add(new VertexPositionColor(tr, color));
            vertices.Add(new VertexPositionColor(bl, color));
            vertices.Add(new VertexPositionColor(br, color));

            this.Ready();
        }

        #region Properties
        /// <summary>
        /// Gets and sets the health of the tank
        /// </summary>
        public int Health
        {
            get { return health; }
            set 
            { 
                health = value;
                if (health <= PlayerTank.MaxHealth / 4)
                {
                    color = Color.Red;
                }
                else if (health <= PlayerTank.MaxHealth / 2)
                {
                    color = Color.Yellow;
                }
                else
                {
                    color = Color.Green;
                }

            }
        }
        #endregion

        /// <summary>
        /// Updates the health bar and determines
        /// if it is low on health. If it is the bar
        /// turns red.
        /// </summary>
        public override void Update(bool updateToggle)
        {
            if (updateToggle != HasUpdated)
            {
                base.Update(updateToggle);

                AdjustVertices();
            }
        }
    }
}
