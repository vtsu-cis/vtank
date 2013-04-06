/*!
    \file   Minimap.cs
    \brief  A Minimap displayed on screen that shows the position of the player
 *          relative to its surroundings 
    \author (C) Copyright 2008 by Vermont Technical College
*/
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Renderer;
using Client.src.service;
using Renderer.SceneTools;
using Microsoft.Xna.Framework.Input;
using System.IO;
using Renderer.SceneTools.Entities;
using Client.src.states.gamestate;

namespace Client.src.util.game
{

    public class Minimap : IDisposable
    {
        #region Constants
        private static readonly int miniMapScaleFactor = 6; //max = 20
        private static readonly int miniMapBorderBuffer = 150;
        public static readonly int miniMapWidth = 190;
        public static readonly int miniMapHeight = 190;
        #endregion

        #region Members
        private PlayerManager players;
        private FlagManager flags;
        private BaseManager bases;
        private UtilityManager utilities;
        private PlayerTank localPlayer;
        private VTankObject.GameMode currentGameMode;
        private RenderTarget2D renderTarget;
        
        private Texture2D texture;
        private Texture2D mapObjectTexture;
        private Rectangle area;
        private Color minimapColor = new Color(255, 255, 255, 150);
        #endregion

        #region Properties
        public bool Enabled
        {
            get;
            set;
        }
        #endregion

        #region Constructor
        public Minimap(PlayerManager playerList, PlayerTank _localPlayer)
            :base()
        {
            IsDisposed = false;
            Enabled = true;
            players = playerList;
            localPlayer = _localPlayer;
            mapObjectTexture = ServiceManager.Resources.GetTexture2D("textures\\misc\\MiniMap\\PlayerIndicator");
            area = new Rectangle(ServiceManager.Game.Width - miniMapWidth - 15, 15, miniMapWidth, miniMapHeight);
        }
        #endregion

        #region Methods
        public void SetMap(Map map, GamePlayState _state)
        {
            currentGameMode = _state.CurrentGameMode;
            flags = _state.Flags;
            utilities = _state.Utilities;
            bases = _state.Bases;
            try
            {
                GraphicsDevice device = Renderer.GraphicOptions.graphics.GraphicsDevice;
                PresentationParameters pp = device.PresentationParameters;
                renderTarget = new RenderTarget2D(device, (int)map.Width * miniMapScaleFactor + 2 * miniMapBorderBuffer,
                             (int)map.Height * miniMapScaleFactor + 2 * miniMapBorderBuffer, 1, SurfaceFormat.Color,
                             pp.MultiSampleType,
                             pp.MultiSampleQuality, RenderTargetUsage.PreserveContents);
                
                DepthStencilBuffer previousDepth = device.DepthStencilBuffer;
                device.DepthStencilBuffer = null;
                device.SetRenderTarget(0, renderTarget);
                device.Clear(Color.Black);
                ServiceManager.Game.Batch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.None);
                Texture2D miniMapDrawer = ServiceManager.Resources.GetTexture2D("textures\\misc\\MiniMap\\wallandbackground");
                

                for (uint x = 0; x < map.Width; x++)
                {
                    for (uint y = 0; y < map.Height; y++)
                    {
                        Tile tmpTile = map.GetTile(x, y);

                        if (!tmpTile.IsPassable)
                            ServiceManager.Game.Batch.Draw(miniMapDrawer,
                                new Vector2(x * miniMapScaleFactor + miniMapBorderBuffer, y * miniMapScaleFactor + miniMapBorderBuffer),
                                new Rectangle(0, 0, miniMapScaleFactor, miniMapScaleFactor), Color.White);
                    }
                }

                ServiceManager.Game.Batch.End();
                device.DepthStencilBuffer = previousDepth;
                device.SetRenderTarget(0, null);
                texture = renderTarget.GetTexture();

                renderTarget = new RenderTarget2D(device, 235, 235,
                     1, SurfaceFormat.Color,
                     pp.MultiSampleType,
                     pp.MultiSampleQuality, RenderTargetUsage.PreserveContents);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
            }
        }

        private Vector2 MapFromWorld(Vector3 position)
        {
            Vector2 output = Vector2.Zero;

            Vector3 pos = position / Constants.TILE_SIZE;
            pos.Y = -pos.Y;
            output.X = miniMapBorderBuffer + (pos.X * miniMapScaleFactor);
            output.Y = miniMapBorderBuffer + (pos.Y * miniMapScaleFactor);

            return output;
        }
        #endregion

        #region Update Draw

        public void Draw(Vector3 playerPosition)
        {
            if (Enabled && !IsDisposed)
            {
                Vector2 mapPos = MapFromWorld(playerPosition);
                mapPos.X = mapPos.X - miniMapWidth / 2.0f;
                mapPos.Y = mapPos.Y - miniMapHeight / 2.0f;
                
                GraphicsDevice device = Renderer.GraphicOptions.graphics.GraphicsDevice;
                PresentationParameters pp = device.PresentationParameters;
                /*renderTarget = new RenderTarget2D(device, 235, 235,
                     1, SurfaceFormat.Color,
                     pp.MultiSampleType,
                     pp.MultiSampleQuality, RenderTargetUsage.PreserveContents);*/
                
                device.SetRenderTarget(0, renderTarget);
                device.Clear(Color.Black);
                
                ServiceManager.Game.Batch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.None);
                ServiceManager.Game.Batch.Draw(texture, Vector2.Zero,
                    new Rectangle((int)mapPos.X, (int)mapPos.Y, 235, 235), Color.White);

                Vector2 objPos;
                Rectangle tex;
                foreach (PlayerTank tank in players.Values)
                {
                    objPos = MapFromWorld(tank.Position);
                    objPos = objPos - mapPos;// -Vector2.One * 7.5f;
                    Vector2 origin;
                    if (tank == localPlayer)
                    {
                        tex = new Rectangle(20, 0, 15, 20);
                        origin = new Vector2(7.5f, 14.5f);
                    }
                    else
                    {
                        tex = new Rectangle(0, 0, 15, 15);
                        origin = new Vector2(7.5f, 7.5f);
                    }
                    Color c;
                    if (tank.Alive)
                        c = Toolkit.GetColor(Toolkit.GetColor(tank.Attributes.color), tank.Team);
                    else
                        c = Color.Gray;

                    ServiceManager.Game.Batch.Draw(mapObjectTexture, objPos, tex, c,
                        -tank.ZRotation + MathHelper.ToRadians(90f), origin, 1, SpriteEffects.None, 0 );
                }
                
                tex = new Rectangle(80, 0, 20, 20);
                foreach (UtilityIcon util in utilities)
                {

                    objPos = MapFromWorld(util.Position);
                    objPos = objPos - mapPos -Vector2.One * 10f;
                    ServiceManager.Game.Batch.Draw(mapObjectTexture, objPos, tex, Color.White);
                }

                tex = new Rectangle(40, 0, 20, 20);
                foreach (KeyValuePair<GameSession.Alliance, Flag> flag in flags)
                {
                    if (!flag.Value.Hidden)
                    {
                        objPos = MapFromWorld(flag.Value.Position);
                        objPos = objPos - mapPos - Vector2.One * 10f;
                        ServiceManager.Game.Batch.Draw(mapObjectTexture, objPos, tex, Toolkit.GetColor(Color.White, flag.Key));
                    }
                }


                tex = new Rectangle(60, 0, 20, 20);
                foreach (Base gBase in bases)
                {
                    objPos = MapFromWorld(gBase.Position);
                    objPos = objPos - mapPos - Vector2.One * 10f;
                    ServiceManager.Game.Batch.Draw(mapObjectTexture, objPos, tex, Toolkit.GetColor(Color.White, gBase.BaseColor));

                }

                ServiceManager.Game.Batch.End();

                device.SetRenderTarget(0, null);
                Texture2D tmptex = renderTarget.GetTexture();

                ServiceManager.Game.Batch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.None);
                ServiceManager.Game.Batch.Draw(tmptex, area, minimapColor);
                ServiceManager.Game.Batch.End();
            }
        }
        #endregion

        #region IDisposable Members

        /// <summary>
        /// Gets whether or not this minimap is disposed.
        /// </summary>
        public bool IsDisposed
        {
            get;
            private set;
        }

        /// <summary>
        /// Ensures all references are released.
        /// </summary>
        public void Dispose()
        {
            players = null;
            flags = null;
            bases = null;
            utilities = null;
            localPlayer = null;
            renderTarget = null;
            texture = null;
            mapObjectTexture = null;

            IsDisposed = true;
        }

        #endregion
    }
}