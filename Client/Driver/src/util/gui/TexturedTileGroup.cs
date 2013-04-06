using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Renderer.SceneTools.Entities;
using Client.src.service;
using Renderer;

namespace Client.src.util.game
{
    public class TexturedTileGroupManager
    {
        #region Members
        private Dictionary<int, VertexGroup> WallTextureGroups;
        private Dictionary<int, VertexGroup> NonTransWallTextureGroups;
        private Dictionary<int, VertexGroup> FloorTextureGroups;
        private static readonly Vector2 TCUL = new Vector2(0.0f, 0.0f);
        private static readonly Vector2 TCUR = new Vector2(1.0f, 0.0f);
        private static readonly Vector2 TCLL = new Vector2(0.0f, 1.0f);
        private static readonly Vector2 TCLR = new Vector2(1.0f, 1.0f);

        #endregion

        #region Constructor
        public TexturedTileGroupManager()
        {
            WallTextureGroups = new Dictionary<int, VertexGroup>();
            NonTransWallTextureGroups = new Dictionary<int, VertexGroup>();
            FloorTextureGroups = new Dictionary<int, VertexGroup>();
        }
        #endregion

        public void AddWall(Vector3 LL, Vector3 normal, int tilesWide, int tilesHigh, int textureID)
        {
            VertexPositionNormalTexture lowerLeft, lowerRight, upperLeft, upperRight;
            Vector3 right = new Vector3( -normal.Y, normal.X, normal.Z);
            right.Normalize();
            lowerLeft = new VertexPositionNormalTexture(LL, normal, TCLL * tilesHigh);
            lowerRight = new VertexPositionNormalTexture(LL + right * (tilesWide * Constants.TILE_SIZE), normal,  TCLL * tilesHigh + TCUR * tilesWide);
            upperLeft = new VertexPositionNormalTexture(LL + Vector3.UnitZ * (tilesHigh * Constants.TILE_SIZE), normal, TCUL);
            upperRight = new VertexPositionNormalTexture(LL + Vector3.UnitZ * (tilesHigh * Constants.TILE_SIZE) + right * (tilesWide * Constants.TILE_SIZE),
                normal, TCUR * tilesWide);

            if (tilesHigh <= 1)
            {
                if (!NonTransWallTextureGroups.ContainsKey(textureID))
                {
                    VertexGroup gp = new VertexGroup(TileList.GetTile(textureID));
                    gp.Technique = RendererAssetPool.UniversalEffect.Techniques.TexturedWrap;
                    NonTransWallTextureGroups.Add(textureID, gp);
                }
                NonTransWallTextureGroups[textureID].Add(lowerLeft);
                NonTransWallTextureGroups[textureID].Add(upperLeft);
                NonTransWallTextureGroups[textureID].Add(lowerRight);
                NonTransWallTextureGroups[textureID].Add(lowerRight);
                NonTransWallTextureGroups[textureID].Add(upperLeft);
                NonTransWallTextureGroups[textureID].Add(upperRight);
            }


            else
            {
                if (!WallTextureGroups.ContainsKey(textureID))
                {
                    VertexGroup gp = new VertexGroup(TileList.GetTile(textureID));
                    gp.Technique = RendererAssetPool.UniversalEffect.Techniques.TexturedWrap;
                    gp.TransparencyEnabled = true;
                    WallTextureGroups.Add(textureID, gp);
                }
                WallTextureGroups[textureID].Add(lowerLeft);
                WallTextureGroups[textureID].Add(upperLeft);
                WallTextureGroups[textureID].Add(lowerRight);
                WallTextureGroups[textureID].Add(lowerRight);
                WallTextureGroups[textureID].Add(upperLeft);
                WallTextureGroups[textureID].Add(upperRight);
            }
            
        }

        public void AddFloor(Vector3 LL, int height, int textureID)
        {
            VertexPositionNormalTexture lowerLeft, lowerRight, upperLeft, upperRight;
            LL = LL + Vector3.UnitZ * height * Constants.TILE_SIZE;
            lowerLeft = new VertexPositionNormalTexture(LL , Vector3.UnitZ, TCLL);
            lowerRight = new VertexPositionNormalTexture(LL + Vector3.UnitX * Constants.TILE_SIZE, Vector3.UnitZ, TCLR);
            upperLeft = new VertexPositionNormalTexture(LL + Vector3.UnitY * Constants.TILE_SIZE, Vector3.UnitZ, TCUL);
            upperRight = new VertexPositionNormalTexture(LL + (Vector3.UnitY+ Vector3.UnitX) * Constants.TILE_SIZE
                                                         , Vector3.UnitZ, TCUR);
            if (height <= 1)
            {
                if (!FloorTextureGroups.ContainsKey(textureID))
                {
                    VertexGroup gp = new VertexGroup(TileList.GetTile(textureID));
                    gp.Technique = RendererAssetPool.UniversalEffect.Techniques.TexturedClamp;
                    FloorTextureGroups.Add(textureID, gp);
                }
                FloorTextureGroups[textureID].Add(lowerLeft);
                FloorTextureGroups[textureID].Add(upperLeft);
                FloorTextureGroups[textureID].Add(lowerRight);
                FloorTextureGroups[textureID].Add(lowerRight);
                FloorTextureGroups[textureID].Add(upperLeft);
                FloorTextureGroups[textureID].Add(upperRight);
            }
            else
            {
                if (!WallTextureGroups.ContainsKey(textureID))
                {
                    VertexGroup gp = new VertexGroup(TileList.GetTile(textureID));
                    gp.TransparencyEnabled = true;
                    gp.Technique = RendererAssetPool.UniversalEffect.Techniques.TexturedWrap;
                    WallTextureGroups.Add(textureID, gp);
                }
                WallTextureGroups[textureID].Add(lowerLeft);
                WallTextureGroups[textureID].Add(upperLeft);
                WallTextureGroups[textureID].Add(lowerRight);
                WallTextureGroups[textureID].Add(lowerRight);
                WallTextureGroups[textureID].Add(upperLeft);
                WallTextureGroups[textureID].Add(upperRight);
            }
        }

        public void AllReady()
        {
            foreach (VertexGroup vergrp in FloorTextureGroups.Values)
            {
                if (vergrp.GetType().Equals(typeof(TextObject)))
                    throw new Exception();

                ServiceManager.Scene.Add(vergrp, 0);
                vergrp.Ready();
            }
            foreach (VertexGroup vergrp in NonTransWallTextureGroups.Values)
            {
                if (vergrp.GetType().Equals(typeof(TextObject)))
                    throw new Exception();

                ServiceManager.Scene.Add(vergrp, 0);
                vergrp.Ready();
            }
            foreach (VertexGroup vergrp in WallTextureGroups.Values)
            {
                if (vergrp.Equals(typeof(TextObject)))
                    throw new Exception();

                ServiceManager.Scene.Add(vergrp, 3);
                vergrp.Ready();
            }
        }
    }
}
