/*!
    \file   MiniMapTile.cs
    \brief  Defines a tile for rendering in the MiniMap
    \author (C) Copyright 2009 by Vermont Technical College
*/
using System;
using System.Collections.Generic;
using System.Text;
using Renderer.SceneTools.Entities;
using Microsoft.Xna.Framework.Graphics;
using Client.src.service;
using Microsoft.Xna.Framework;

namespace Client.src.util.game
{
    /// <summary>
    /// General Information about the minimap tile
    /// </summary>
    public class MiniMapTile : VertexColorGroup
    {

        #region Properties

        public Color TileColor { get; set; }
        public bool Passable { get; protected set; }
        public Texture2D Texture { get; protected set; }

        #endregion

        /// <summary>
        /// MiniMap Tile constructor
        /// </summary>
        /// <param name="passable">If the tile is passable</param>
        /// <param name="x">x position</param>
        /// <param name="y">y position</param>
        public MiniMapTile(bool passable, ushort height, int x, int y) :base()
        {
            Passable = passable;

            Position = new Vector3(x * Constants.TILE_SIZE, -y * Constants.TILE_SIZE, 0);

            GetColor(passable, height);
            InitalizeTexture();
            Initialize();
        }

        /// <summary>
        /// Initializes a texture for the tiles
        /// </summary>
        private void InitalizeTexture()
        {
            Texture2D Texture = new Texture2D(ServiceManager.Game.GraphicsDevice, 1, 1, 1,
                    TextureUsage.None, SurfaceFormat.Color);
            Color[] color = new Color[] { TileColor };
            Texture.SetData(color);
        }

        /// <summary>
        /// Initalizes the position of the vertexes to be drawn
        /// </summary>
        private void Initialize()
        {
            List<VertexPositionColor> verts = new List<VertexPositionColor>();
            VertexPositionColor[] tempVerts = new VertexPositionColor[4];

            tempVerts[0].Position = new Vector3(Position.X, Position.Y, 1);
            tempVerts[0].Color = TileColor;
            tempVerts[1].Position = new Vector3(Position.X, Position.Y + Constants.TILE_SIZE, 1);
            tempVerts[1].Color = TileColor;
            tempVerts[2].Position = new Vector3(Position.X + Constants.TILE_SIZE, Position.Y, 1);
            tempVerts[2].Color = TileColor;
            tempVerts[3].Position = new Vector3(Position.X + Constants.TILE_SIZE,
                                                Position.Y + Constants.TILE_SIZE, 1);
            tempVerts[3].Color = TileColor;

            for (int i = 0; i < tempVerts.Length; i++)
                verts.Add(tempVerts[i]);

            base.vertices = verts;
            base.Ready();
        }


        /// <summary>
        /// Helper method to color the tile to the proper color.
        /// Light green if the tile is passable
        /// Green if its not.
        /// </summary>
        /// <param name="passable">Tile is passable</param>
        /// <param name="height">The height of the tile</param>
        private void GetColor(bool passable, ushort height)
        {

            Color background = new Color(0, 30, 0);
            Color blocked = Color.Green;

            if (!passable || height > 0)
                TileColor = blocked;
            else
                TileColor = background;
        }
    }
}
