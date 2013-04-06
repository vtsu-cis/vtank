/*!
    \file   DrawableTile.cs
    \brief  Defines a 3D tile with texture used in the map
    \author (C) Copyright 2009 by Vermont Technical College
*/
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Client.src.service;
using Renderer.SceneTools.Entities;
using Renderer;

namespace Client.src.util.game
{
    /// <summary>
    /// General Information about the tile to be loaded
    /// </summary>
    public class DrawableTile : VertexGroup
    {
        #region Members
        // Map the texture with these coordinates: (0, 0)->(1, 0)->(0, 1)->(1, 1)
        private static readonly Vector2 textureUpperLeft  = new Vector2(0.0f, 0.0f);
        private static readonly Vector2 textureUpperRight = new Vector2(1.0f, 0.0f);
        private static readonly Vector2 textureLowerLeft  = new Vector2(0.0f, 1.0f);
        private static readonly Vector2 textureLowerRight = new Vector2(1.0f, 1.0f);

        private static readonly int TILE_SIZE = /*TODO VTankGlobal.TILE_SIZE;*/64;

        private bool passable;
        private int height;
        private Point pos;
        #endregion

        /// <summary>
        /// Drawable tile Constructor
        /// </summary>
        /// <param name="_texture">the Texture to be used</param>
        /// <param name="x">its X position</param>
        /// <param name="y">its Y position</param>
        /// <param name="_passable">If the tile is passable</param>
        /// <param name="heightNorth">Height of the northern tile.</param>
        /// <param name="heightWest">Height of the western tile.</param>
        /// <param name="heightEast">Height of the eastern tile.</param>
        /// <param name="heightSouth">Height of the southern tile.</param>
        public DrawableTile(Texture2D _texture, Tile tile, int x, int y,
            int heightNorth, int heightWest, int heightEast, int heightSouth)
            : base(_texture)
        {

            //The Y value is set to negative because of the way the tiles are designed
            //This allows the map to be loaded in the 4th quadrant correctly
            // TODO: NOTE: Tile size is decreased by 64 in order to position it correctly.
            // Figure out why, later.
            pos = new Point(x * TILE_SIZE, (-y * TILE_SIZE) - Constants.TILE_SIZE);

            passable = tile.IsPassable;
            height = tile.Height;

            //Initializes the 3D tiles

            Initialize(heightNorth, heightWest, heightEast, heightSouth);       
            Ready();
            if (tile.ObjectID != 0)
            {
                boundingBox = new BoundingBox(new Vector3(pos.X, pos.Y, 0),
                    new Vector3(pos.X + TILE_SIZE, pos.Y + TILE_SIZE, TILE_SIZE * 1));
            }
            else
            {
                boundingBox = new BoundingBox(new Vector3(pos.X, pos.Y, 0),
                    new Vector3(pos.X + TILE_SIZE, pos.Y + TILE_SIZE, TILE_SIZE * height));
            }
        }
        #region Initialize
        /// <summary>
        /// Initializes the vertexes to be drawn
        /// </summary>
        private void Initialize(int heightNorth, int heightWest, int heightEast, int heightSouth)
        {
            //Draws the position and the texture coordinates to the specific vertex
            List<VertexPositionNormalTexture> verts = new List<VertexPositionNormalTexture>();
            VertexPositionNormalTexture[] tempVerts = new VertexPositionNormalTexture[6];
            if (height > 0)
            {
                // If the height is greater than zero, we need to apply a side wall tile.
                if (height > heightSouth)
                {
                    tempVerts = CreateFrontWall(this, heightSouth);
                    for (int i = 0; i < tempVerts.Length; i++)
                        verts.Add(tempVerts[i]);
                }

                if (height > heightNorth)
                {
                    tempVerts = CreateBackWall(this, heightNorth);
                    for (int i = 0; i < tempVerts.Length; i++)
                        verts.Add(tempVerts[i]);
                }

                if (height > heightWest)
                {
                    tempVerts = CreateLeftWall(this, heightWest);
                    for (int i = 0; i < tempVerts.Length; i++)
                        verts.Add(tempVerts[i]);
                }

                if (height > heightEast)
                {
                    tempVerts = CreateRightWall(this, heightEast);
                    for (int i = 0; i < tempVerts.Length; i++)
                        verts.Add(tempVerts[i]);
                }
            }

            Vector3 normal = new Vector3(0, 1, 0);
            tempVerts[0].Position = new Vector3(pos.X, pos.Y, height * TILE_SIZE);
            tempVerts[0].TextureCoordinate = textureLowerLeft;
            tempVerts[0].Normal = normal;
            tempVerts[1].Position = new Vector3(pos.X, pos.Y + TILE_SIZE, height * TILE_SIZE);
            tempVerts[1].TextureCoordinate = textureUpperLeft;
            tempVerts[1].Normal = normal;
            tempVerts[2].Position = new Vector3(pos.X + TILE_SIZE, pos.Y + TILE_SIZE, height * TILE_SIZE);
            tempVerts[2].TextureCoordinate = textureUpperRight;
            tempVerts[2].Normal = normal;
            tempVerts[3].Position = new Vector3(pos.X + TILE_SIZE, pos.Y, height * TILE_SIZE);
            tempVerts[3].TextureCoordinate = textureLowerRight;
            tempVerts[3].Normal = normal;
            tempVerts[4].Position = new Vector3(pos.X, pos.Y, height * TILE_SIZE);
            tempVerts[4].TextureCoordinate = textureLowerLeft;
            tempVerts[4].Normal = normal;
            tempVerts[5].Position = new Vector3(pos.X + TILE_SIZE, pos.Y + TILE_SIZE, height * TILE_SIZE);
            tempVerts[5].TextureCoordinate = textureUpperRight;
            tempVerts[5].Normal = normal;

            for (int i = 0; i < tempVerts.Length; i++)
            {
                //tempVerts[i].Normal.Normalize();
                verts.Add(tempVerts[i]);
            }

            base.vertices = verts;

            //Draws a bounding box around each tile
            boundingBox = new BoundingBox(new Vector3(pos.X, pos.Y, 0),
                new Vector3(pos.X + TILE_SIZE, pos.Y + TILE_SIZE, TILE_SIZE * height));

        }

        /// <summary>
        /// Helper method for adding a wall to a tile. Note that this does not actually
        /// 'add' the wall -- it simply returns the necessary vertices.
        /// </summary>
        /// <param name="tile">Tile to add a wall to.</param>
        /// <returns>Vertex list to add to the vertices.</returns>
        private static VertexPositionNormalTexture[] CreateFrontWall(DrawableTile tile, int heightSouth)
        {
            Point pos = tile.Pos;
            int height = tile.height;
            int numTiles = height - heightSouth;
            Vector3 normal = new Vector3(0, 0, -1);

            VertexPositionNormalTexture[] vertices = 
                new VertexPositionNormalTexture[6];

            vertices[0].Position = new Vector3(pos.X, pos.Y, heightSouth * TILE_SIZE);
            vertices[0].TextureCoordinate = textureLowerLeft * new Vector2(0, numTiles);
            vertices[0].Normal = normal;
            vertices[1].Position = new Vector3(pos.X, pos.Y, height*TILE_SIZE);
            vertices[1].TextureCoordinate = textureUpperLeft;
            vertices[1].Normal = normal;
            vertices[2].Position = new Vector3(pos.X + TILE_SIZE, pos.Y, height * TILE_SIZE);
            vertices[2].TextureCoordinate = textureUpperRight;
            vertices[2].Normal = normal;
            vertices[3].Position = new Vector3(pos.X + TILE_SIZE, pos.Y, heightSouth * TILE_SIZE);
            vertices[3].TextureCoordinate = textureLowerRight * new Vector2(1, numTiles);
            vertices[3].Normal = normal;
            vertices[4].Position = new Vector3(pos.X, pos.Y, heightSouth * TILE_SIZE);
            vertices[4].TextureCoordinate = textureLowerLeft * new Vector2(0, numTiles);
            vertices[4].Normal = normal;
            vertices[5].Position = new Vector3(pos.X + TILE_SIZE, pos.Y, height * TILE_SIZE);
            vertices[5].TextureCoordinate = textureUpperRight;
            vertices[5].Normal = normal;

            return vertices;
        }

        /// <summary>
        /// Helper method for adding a wall to a tile. Note that this does not actually
        /// 'add' the wall -- it simply returns the necessary vertices.
        /// </summary>
        /// <param name="tile">Tile to add a wall to.</param>
        /// <returns>Vertex list to add to the vertices.</returns>
        private static VertexPositionNormalTexture[] CreateBackWall(DrawableTile tile, int heightNorth)
        {
            Point pos = new Point(tile.Pos.X, tile.Pos.Y + TILE_SIZE);
            int height = tile.height;
            int numTiles = height - heightNorth;
            Vector3 normal = new Vector3(0, 0, 1);

            VertexPositionNormalTexture[] vertices = new VertexPositionNormalTexture[6];

            vertices[0].Position = new Vector3(pos.X + TILE_SIZE, pos.Y, height * TILE_SIZE);
            vertices[0].TextureCoordinate = textureUpperLeft;
            vertices[0].Normal = normal;
            vertices[1].Position = new Vector3(pos.X, pos.Y, heightNorth * TILE_SIZE);
            vertices[1].TextureCoordinate = textureLowerRight * new Vector2(1, numTiles); 
            vertices[1].Normal = normal;
            vertices[2].Position = new Vector3(pos.X + TILE_SIZE, pos.Y, heightNorth * TILE_SIZE);
            vertices[2].TextureCoordinate = textureLowerLeft * new Vector2(0, numTiles);
            vertices[2].Normal = normal;
            vertices[3].Position = new Vector3(pos.X + TILE_SIZE, pos.Y, height * TILE_SIZE);
            vertices[3].TextureCoordinate = textureUpperLeft; 
            vertices[3].Normal = normal;
            vertices[4].Position = new Vector3(pos.X, pos.Y, height * TILE_SIZE);
            vertices[4].TextureCoordinate = textureUpperRight;
            vertices[4].Normal = normal;
            vertices[5].Position = new Vector3(pos.X, pos.Y, heightNorth * TILE_SIZE);
            vertices[5].TextureCoordinate = textureLowerRight * new Vector2(1, numTiles);
            vertices[5].Normal = normal;

            return vertices;
        }

        /// <summary>
        /// Helper method for adding a wall to a tile. Note that this does not actually
        /// 'add' the wall -- it simply returns the necessary vertices.
        /// </summary>
        /// <param name="tile">Tile to add a wall to.</param>
        /// <returns>Vertex list to add to the vertices.</returns>
        private static VertexPositionNormalTexture[] CreateLeftWall(DrawableTile tile, int heightWest)
        {
            Point pos = tile.Pos;
            int height = tile.height;
            int numTiles = height - heightWest;
            Vector3 normal = new Vector3(-1, 0, 0);

            VertexPositionNormalTexture[] vertices = 
                new VertexPositionNormalTexture[6];

            vertices[0].Position = new Vector3(pos.X, pos.Y + TILE_SIZE, height * TILE_SIZE);
            vertices[0].TextureCoordinate = textureUpperLeft; //textureUpperRight;
            vertices[0].Normal = normal;
            vertices[1].Position = new Vector3(pos.X, pos.Y, heightWest * TILE_SIZE);
            vertices[1].TextureCoordinate = textureLowerRight * new Vector2(1, numTiles); //textureLowerLeft * new Vector2(0, numTiles);
            vertices[1].Normal = normal;
            vertices[2].Position = new Vector3(pos.X, pos.Y + TILE_SIZE, heightWest * TILE_SIZE);
            vertices[2].TextureCoordinate = textureLowerLeft * new Vector2(0, numTiles); //textureLowerRight * new Vector2(1, numTiles);
            vertices[2].Normal = normal;
            vertices[3].Position = new Vector3(pos.X, pos.Y + TILE_SIZE, height * TILE_SIZE);
            vertices[3].TextureCoordinate = textureUpperLeft; //textureUpperRight;
            vertices[3].Normal = normal;
            vertices[4].Position = new Vector3(pos.X, pos.Y, height * TILE_SIZE);
            vertices[4].TextureCoordinate = textureUpperRight; //textureUpperLeft;
            vertices[4].Normal = normal;
            vertices[5].Position = new Vector3(pos.X, pos.Y, heightWest * TILE_SIZE);
            vertices[5].TextureCoordinate = textureLowerRight * new Vector2(1, numTiles); //textureLowerLeft * new Vector2(0, numTiles);
            vertices[5].Normal = normal;

            return vertices;
        }

        /// <summary>
        /// Helper method for adding a wall to a tile. Note that this does not actually
        /// 'add' the wall -- it simply returns the necessary vertices.
        /// </summary>
        /// <param name="tile">Tile to add a wall to.</param>
        /// <returns>Vertex list to add to the vertices.</returns>
        private static VertexPositionNormalTexture[] CreateRightWall(DrawableTile tile, int heightEast)
        {
            Point pos = new Point(tile.Pos.X + TILE_SIZE, tile.Pos.Y);
            int height = tile.height;
            int numTiles = height - heightEast;
            Vector3 normal = new Vector3(1, 0, 0);

            VertexPositionNormalTexture[] vertices = 
                new VertexPositionNormalTexture[6];

            vertices[0].Position = new Vector3(pos.X, pos.Y, heightEast * TILE_SIZE);
            vertices[0].TextureCoordinate = textureLowerLeft * new Vector2(0, numTiles);
            vertices[0].Normal = normal;
            vertices[1].Position = new Vector3(pos.X, pos.Y, height * TILE_SIZE);
            vertices[1].TextureCoordinate = textureUpperLeft;
            vertices[1].Normal = normal;
            vertices[2].Position = new Vector3(pos.X, pos.Y + TILE_SIZE, height * TILE_SIZE);
            vertices[2].TextureCoordinate = textureUpperRight;
            vertices[2].Normal = normal;
            vertices[3].Position = new Vector3(pos.X, pos.Y + TILE_SIZE, heightEast * TILE_SIZE);
            vertices[3].TextureCoordinate = textureLowerRight * new Vector2(1, numTiles);
            vertices[3].Normal = normal;
            vertices[4].Position = new Vector3(pos.X, pos.Y, heightEast * TILE_SIZE);
            vertices[4].TextureCoordinate = textureLowerLeft * new Vector2(0, numTiles);
            vertices[4].Normal = normal;
            vertices[5].Position = new Vector3(pos.X, pos.Y + TILE_SIZE, height * TILE_SIZE);
            vertices[5].TextureCoordinate = textureUpperRight;
            vertices[5].Normal = normal;

            return vertices;
        }
        #endregion

        public override void Draw(EffectTechnique technique)
        {
            if (Height > 0)
            {
                RendererAssetPool.UniversalEffect.CurrentTechnique = 
                    RendererAssetPool.UniversalEffect.Techniques.TexturedWrap;
            }
            else
            {
                RendererAssetPool.UniversalEffect.CurrentTechnique =
                    RendererAssetPool.UniversalEffect.Techniques.TexturedClamp;
            }
            base.Draw(technique);
        }
        /// <summary>
        /// Gets the texture of the tile
        /// </summary>
        public Texture2D Texture
        {
            get { return texture; }
        }

        /// <summary>
        /// Gets the position of the tile
        /// </summary>
        public Point Pos
        {
            get { return pos; }
        }

        /// <summary>
        ///Gets the value of the tile if it is passable
        /// </summary>
        public bool Passable
        {
            get { return passable; }
        }

        /// <summary>
        /// Gets the height of this tile.
        /// </summary>
        public int Height
        {
            get { return height; }
        }

        public List<VertexPositionNormalTexture> Vertices
        {
            get { return base.vertices; }
        }
    }
}