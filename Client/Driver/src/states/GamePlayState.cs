/*!
    \file   GamePlayState.cs
    \brief  VTank's game state - the last state where the user is playing VTank.
    \author (C) Copyright 2009 by Vermont Technical College

*/
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Timers;
using System.Threading;
using Renderer;
using Client.src.service;
using Client.src.util.game;
using Client.src.util;
using GameForms.Forms;
using System.Collections.Generic;
using Renderer.SceneTools;
using Renderer.SceneTools.Entities;
using Client.src.config;
using Client.src.callbacks;
using Client.src.service.services;
using Client.src.events;
using Network.Util;
using Client.src.util.gui;
using Client.src.util.game.data;


namespace Client.src.states.gamestate
{
    /// <summary>
    /// This state will allow the user to play the game
    /// </summary>
    public class GamePlayState : State
    {
        #region Game Constants
        private static readonly float DefaultCameraDistance = 1000.0f;
        private static readonly string OverheadCamera = "overhead";
        private static readonly string ChaseCamera = "chase";
        private static readonly double SYNC_TIMER_MAX_DEFAULT = 2.5;
        #endregion

        #region Members
        public delegate void GameFinishHandler(object sender, EventArgs e);
        public event GameFinishHandler OnGameFinished;

        public BuffBar buffbar;
        public MouseCursor mouseCursor;
        private HelpOverlay helpOverlay;
        private GameTips tips;
        private ChatInput input;
        private Countdown cd;
        private HUD hud;
        private Minimap miniMap;
        private EntityRenderer renderer;
        private FrameCounter fps;
        private Map map;
        //private InGameMenu form;
        private DrawableTile[] visibleTiles;
        private double timeLeft = 0;
        private bool cameraLocked = true;
        private GameCallback callback;
        private Rectangle viewportRect;
        private bool previousCollision = false; // Used for proper synchronization.
        private bool needsSync = false;
        private float prevFrameScrollWheelValue;
        private double syncTimer = 0;
        private double syncTimerMax = SYNC_TIMER_MAX_DEFAULT; // seconds
        private bool rotating = false;
        private VTankObject.GameMode currentGameMode;
        private EventBuffer buffer;
        private bool stuck = false;
        private PlayerTank localPlayer;
        private Texture2D sky;

        public ScoreBoard Scores;
        #endregion
        
        #region Properties

        public VTankObject.GameMode CurrentGameMode
        {
            get { return this.currentGameMode; }
        }


        public TexturedTileGroupManager Tiles
        {
            get;
            private set;
        }
        public LazerBeamManager Lazers
        {
            get;
            private set;
        }
        /// <summary>
        /// Gets the list of players in-game.
        /// </summary>
        public PlayerManager Players
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the local player.
        /// </summary>
        public PlayerTank LocalPlayer
        {
            get { return localPlayer; }
        }

        /// <summary>
        /// Gets the list of projectiles in-game.
        /// </summary>
        public ProjectileManager Projectiles
        {
            get;
            private set;
        }
        /// <summary>
        /// Gets the list of projectiles in-game.
        /// </summary>
        public UtilityManager Utilities
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the list of flags in-game.
        /// </summary>
        public FlagManager Flags
        {
            get;
            private set;
        }

        public BaseManager Bases
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the chat area.
        /// </summary>
        public ChatArea Chat
        {
            get;
            private set;
        }

        public bool Rotating
        {
            get { return rotating; }
        }

        /// <summary>
        /// Gets or sets whether or not the client should re-sync his position.
        /// </summary>
        public bool NeedsSync
        {
            get
            {
                return needsSync;
            }

            set
            {
                needsSync = value;
            }
        }

        /// <summary>
        /// Gets or sets the thread-safe wrapper for the active scene.
        /// </summary>
        public Renderer.SceneTools.Scene Scene
        {
            get
            {
                return ServiceManager.Scene;
            }
        }

        /// <summary>
        /// Gets the current map.
        /// </summary>
        public Map CurrentMap
        {
            get
            {
                return map;
            }
        }

        public EnvironmentEffectManager EnvironmentEffects
        {
            get;
            private set;
        }
        #endregion
        
        #region Constructors
        public GamePlayState(GameCallback _callback, Map _map, EventBuffer _buffer)
            : base(false) // False meaning, do not load/draw the background.
        {
            map = _map;
            //form = new InGameMenu(ServiceManager.Game.Manager);
            ServiceManager.Game.FormManager.RemoveWindow(
                ServiceManager.Game.FormManager.currentWindow);

            callback = _callback;
            buffer = _buffer;
            prevFrameScrollWheelValue = 0f;
        }
        #endregion
        
        #region Overridden Methods
        /// <summary>
        /// Initialize any components required by this state.
        /// </summary>
        public override void Initialize()
        {
            ServiceManager.Game.FormManager.ClearWindow();
            Options options = ServiceManager.Game.Options;
            RendererAssetPool.DrawShadows = GraphicOptions.ShadowMaps;
            mouseCursor = new MouseCursor(options.KeySettings.Pointer);
            mouseCursor.EnableCustomCursor();
            renderer = ServiceManager.Game.Renderer;
            fps = new FrameCounter();
            Chat = new ChatArea();
            Projectiles = new ProjectileManager();
            Tiles = new TexturedTileGroupManager();
            Utilities = new UtilityManager();            
            cd = new Countdown();
            Lazers = new LazerBeamManager(this);
            Scene.Add(Lazers, 0);
            Players = new PlayerManager();
            currentGameMode = ServiceManager.Theater.GetCurrentGameMode();
            Flags = new FlagManager();
            Bases = new BaseManager();
            EnvironmentEffects = new EnvironmentEffectManager();
            buffbar = new BuffBar();
            Scores = new ScoreBoard(currentGameMode, Players);
            input = new ChatInput(new Vector2(5, ServiceManager.Game.GraphicsDevice.Viewport.Height), 300);//300 is a magic number...Create a chat width variable.
            input.Visible = false;
            hud = HUD.GetHudForPlayer(Players.GetLocalPlayer());
            localPlayer = Players.GetLocalPlayer();
            Scene.MainEntity = localPlayer;
            sky = ServiceManager.Resources.GetTexture2D("textures\\misc\\background\\sky");
            tips = new GameTips(new GameContext(CurrentGameMode, localPlayer));
            helpOverlay = new HelpOverlay();
        }

        /// <summary>
        /// Load any content (textures, fonts, etc) required for this state.
        /// </summary>
        public override void LoadContent()
        {            
            visibleTiles = new DrawableTile[map.Width * map.Height];
            FillTiles();

            miniMap = new Minimap(Players, localPlayer);
            miniMap.SetMap(map, this);

            Scene.CreateCamera(
                new Vector3(localPlayer.Position.X, localPlayer.Position.Y, GamePlayState.DefaultCameraDistance),
                new Vector3(localPlayer.Position.X, localPlayer.Position.Y, 0),
                Matrix.CreatePerspectiveFieldOfView(
                    MathHelper.PiOver4,
                    (float)GraphicOptions.graphics.GraphicsDevice.Viewport.Width /
                        (float)GraphicOptions.graphics.GraphicsDevice.Viewport.Height,
                    10f, 
                    5000f), 
                OverheadCamera);
            Scene.SwitchCamera(OverheadCamera);
            Scene.AccessCamera(OverheadCamera).Follow(localPlayer);

            Scene.CreateCamera(
                localPlayer.Position - localPlayer.Front * 1000 + localPlayer.Up * 400,
                localPlayer.Position + new Vector3(0, 0, 100),
                renderer.ActiveScene.AccessCamera(OverheadCamera).Projection,
                ChaseCamera);
            Scene.AccessCamera(ChaseCamera).CameraUp = Vector3.UnitZ;

            timeLeft = ServiceManager.Theater.TimeLeftSeconds;
            ServiceManager.Game.Renderer.ActiveScene.SwitchCamera(ChaseCamera);
            ServiceManager.Game.Renderer.ActiveScene.CurrentCamera.LockTo(localPlayer, new Vector3(-500, 0, 200), new Vector3(0,0,75));
            ServiceManager.Game.Renderer.ActiveScene.LockCameras();
            callback.Ready = true;

            ServiceManager.Theater.Ready();
        }

        /// <summary>
        /// Unload any content (textures, fonts, etc) used by this state. Called when the state is removed.
        /// </summary>
        public override void UnloadContent()
        {
            /*try
            {
                ServiceManager.Game.GraphicsDevice.Reset();
                ServiceManager.Game.GraphicsDevice.VertexDeclaration = null;
                ServiceManager.Game.GraphicsDevice.Vertices[0].SetSource(null, 0, 0);
            }
            catch (Exception ex)
            {
                // If the graphics device was disposed already, it throws an exception.
                Console.Error.WriteLine(ex);
            }*/

            if (mouseCursor != null)
            {
                mouseCursor.DisableCustomCursor();
                mouseCursor = null;
            }
            EnvironmentEffects = null;
            Bases = null;
            buffbar = null;
            cd = null;
            hud = null;
            renderer = null;
            fps = null;
            map = null;
            visibleTiles = null;
            Scores = null;
            Players = null;
            Projectiles = null;
            Chat = null;
            buffer = null;            
            miniMap.Dispose();
            miniMap = null;

            if (OnGameFinished != null)
            {
                EventArgs args = new EventArgs();
                OnGameFinished.Invoke(this, args);
            }
        }

        /// <summary>
        /// Logical updates occur here.
        /// </summary>
        public override void Update()
        {
            fps.Update();

            // Chat input check:
            if (KeyPressHelper.IsPressed(Keys.Enter))
            {
                if (input.AcceptEnter() && !ServiceManager.Game.Console.Visible && ServiceManager.Game.IsActive)
                {
                    input.Visible = !input.Visible;
                    if (input.Visible)
                    {
                        input.Focused = true;
                        ChangeMovement(VTankObject.Direction.NONE);
                        ChangeRotation(VTankObject.Direction.NONE);
                    }
                    else
                    {
                        DoChatMessage(input.GetText());
                    }
                }
            }

            //Menu press response block (Esc keypress by default)
            if (KeyPressHelper.IsPressed(ServiceManager.Game.Options.KeySettings.Menu))
            {
                //rotating = true;
                // TODO: Show menu here.

                ServiceManager.DestroyTheaterCommunicator();
                ServiceManager.Game.Renderer.ActiveScene.DeleteCamera(OverheadCamera);
                ServiceManager.Game.Renderer.ActiveScene.DeleteCamera(ChaseCamera);
                ServiceManager.Game.Renderer.ActiveScene.ClearAll();
                ServiceManager.Game.SwitchToDefaultCamera();
                input.Hide();
                ServiceManager.StateManager.ChangeState(new TankListState());

                return;
            }

            input.Update();
            Chat.Update();

            if (rotating)
            {
                // Check for chat messages.
                IEvent[] eventBuf = buffer.PopAll();
                for (int i = 0; i < eventBuf.Length; ++i)
                {
                    IEvent evt = eventBuf[i];
                    if (evt is Client.src.events.types.ChatMessageEvent)
                        evt.DoAction();
                }

                Scores.Enabled = true;
                cd.Update();
                CheckForRotate();
                //if (input.Visible)
                //    input.Visible = false;
                return;
            }

            timeLeft -= ServiceManager.Game.DeltaTime;
            ServiceManager.Scene.PercentOfDayComplete = 0.6;//((4.5 * 60) - (timeLeft +2)) / (4.5 * 60);

            if (timeLeft <= 0)
            {
                RotateMap();
            }

            // Process game events.
            IEvent[] events = buffer.PopAll();
            for (int i = 0; i < events.Length; ++i)
            {
                events[i].DoAction();
            }

            if (cameraLocked)
            {
                Camera overheadCamera = renderer.ActiveScene.AccessCamera(OverheadCamera);
                Camera chaseCamera = renderer.ActiveScene.AccessCamera(ChaseCamera);

                if (renderer.ActiveScene.CurrentCamera == chaseCamera)
                {
                    ServiceManager.Game.Renderer.ActiveScene.CurrentCamera.LockTo(
                        localPlayer, new Vector3(-500, 0, 200), new Vector3(0, 0, 75));
                    ServiceManager.Game.Renderer.ActiveScene.TransparentWalls = true;
                }
                else
                {
                    ServiceManager.Game.Renderer.ActiveScene.CurrentCamera.Unlock();
                    ServiceManager.Game.Renderer.ActiveScene.TransparentWalls = false;
                }
            }

            Scene.Update();

            RendererAssetPool.UniversalEffect.LightParameters.FogSeedPosition = localPlayer.Position;
            if(RendererAssetPool.ParticleEffect != null)
                RendererAssetPool.ParticleEffect.Parameters["xFogSeedPosition"].SetValue(localPlayer.Position);
            GraphicOptions.BackgroundColor = new Color(new Vector4(0.15f, 0.15f, 0.15f, 0f) * Scene.AmbientColor.ToVector4());

            Projectiles.CheckCollisions(map, visibleTiles);

            if (!ServiceManager.Game.Console.Visible)
            {
                CheckInput();                

                if (rotating)
                    return;

                CheckMouseInput();                
            }
            else
            {
                ChangeMovement(VTankObject.Direction.NONE);
                ChangeRotation(VTankObject.Direction.NONE);
            }
            
            // Update other components.
            tips.Update();
            buffbar.Update();
            mouseCursor.Update();
            cd.Update();
            hud.Update();
            Bases.Update();
            EnvironmentEffects.Update();
            UpdateHUDValues(localPlayer);
            Projectiles.AddDelayedProjectiles();
            ServiceManager.AudioManager.Update(localPlayer.Position);
            syncTimer += ServiceManager.Game.DeltaTime;
            if (syncTimer >= syncTimerMax)
            {
                if (localPlayer.DirectionMovement != VTankObject.Direction.NONE)
                {
                    NeedsSync = true;
                }

                syncTimer = 0.0;
            }

            if (NeedsSync)
            {
                NeedsSync = false;

                Resync();
            }
        }

        /// <summary>
        /// Draw this state to the screen.
        /// </summary>
        public override void Draw()
        {
            fps.IncrementFrames();

            SpriteBatch batch = ServiceManager.Game.Batch;

            ServiceManager.Game.GraphicsDevice.RenderState.DepthBufferEnable = true;

            GraphicOptions.graphics.GraphicsDevice.Clear(GraphicOptions.BackgroundColor);//Must manually clear since we're not using the main Draw()

            batch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.FrontToBack, SaveStateMode.SaveState);
            batch.Draw(sky, ServiceManager.Game.GraphicsDevice.Viewport.TitleSafeArea, Color.White);
            batch.End();

            Scene.Draw(false);

            batch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.FrontToBack, SaveStateMode.SaveState);

            
            batch.DrawString(ServiceManager.Game.Font,
                fps.GetFormattedFPS(), Vector2.Zero, Color.White);
            batch.DrawString(ServiceManager.Game.Font,
                FormatTimeLeft(timeLeft), Vector2.UnitX * 300, Color.White);
            
            batch.End();

            tips.Draw();
            buffbar.Draw();
            if (mouseCursor != null)
                mouseCursor.Draw();
            hud.Draw();
            cd.Draw();
            Chat.Draw();
            miniMap.Draw(localPlayer.Position);

            Scores.Draw();
            helpOverlay.Draw();
        }
        #endregion

        #region Update Helpers
        /// <summary>
        /// Checks for key presses from the user
        /// </summary>
        private void CheckInput()
        {
            // Do not process input if not active.
            if (!ServiceManager.Game.IsActive)
            {
                // If the tank is moving/rotating, stop it.
                ChangeMovement(VTankObject.Direction.NONE);
                ChangeRotation(VTankObject.Direction.NONE);
                return;
            }

            Options.KeyBindings keys = ServiceManager.Game.Options.KeySettings;
            bool localClientCollision = PerformCollisionChecks();

            if (KeyPressHelper.IsPressed(keys.Camera))
            {
                if (input.Visible == false)
                {
                    if (renderer.ActiveScene.CurrentCamera == renderer.ActiveScene.AccessCamera(ChaseCamera))
                    {
                        renderer.ActiveScene.CurrentCamera = renderer.ActiveScene.AccessCamera(OverheadCamera);
                    }
                    else if (renderer.ActiveScene.CurrentCamera == renderer.ActiveScene.AccessCamera(OverheadCamera))
                    {
                        renderer.ActiveScene.CurrentCamera = renderer.ActiveScene.AccessCamera(ChaseCamera);
                    }
                }
            }

            if (!input.Visible)
            {
                if (localClientCollision && !stuck)
                {
                    ChangeMovement(VTankObject.Direction.NONE);

                    previousCollision = true;
                }
                else
                {

                    if (Keyboard.GetState().IsKeyDown(keys.Forward))
                    {
                        ChangeMovement(VTankObject.Direction.FORWARD);
                    }
                    else if (Keyboard.GetState().IsKeyDown(keys.Backward))
                    {
                        ChangeMovement(VTankObject.Direction.REVERSE);
                    }
                    else
                    {
                        ChangeMovement(VTankObject.Direction.NONE);
                    }

                    if (previousCollision)
                    {
                        previousCollision = false;

                        Resync();
                    }
                }

                if (Keyboard.GetState().IsKeyDown(keys.RotateRight))
                {
                    ChangeRotation(VTankObject.Direction.RIGHT);
                }
                else if (Keyboard.GetState().IsKeyDown(keys.RotateLeft))
                {
                    ChangeRotation(VTankObject.Direction.LEFT);
                }
                else
                {
                    ChangeRotation(VTankObject.Direction.NONE);
                }

                if (KeyPressHelper.IsPressed(keys.Minimap))
                {
                    miniMap.Enabled = !miniMap.Enabled;
                }

                if (Keyboard.GetState().IsKeyDown(keys.Score))
                {
                    Scores.Enabled = true;
                }
                else
                {
                    Scores.Enabled = false;
                }

                if (KeyPressHelper.IsPressed(Keys.F1))
                {
                    helpOverlay.Enabled = !helpOverlay.Enabled;
                }
            }
        }

        /// <summary>
        /// Send a chat message, if the message is not null or empty.
        /// </summary>
        /// <param name="message">Message to send.</param>
        private void DoChatMessage(string message)
        {
            if (message == null)
                return;

            message = message.Trim();
            if (!String.IsNullOrEmpty(message))
            {
                // Check for basic, local commands.
                if (message == "/unstuck")
                {
                    stuck = true;
                }
                else
                {
                    ServiceManager.Theater.SendChatMessage(message);
                }
            }
        }

        /// <summary>
        /// Helper method for checking mouse input (for things like firing).
        /// </summary>
        private void CheckMouseInput()
        {
            if (Mouse.GetState().ScrollWheelValue != prevFrameScrollWheelValue
                && renderer.ActiveScene.CurrentCamera == renderer.ActiveScene.AccessCamera(OverheadCamera))
            {
                renderer.ActiveScene.CurrentCamera.Zoom(prevFrameScrollWheelValue - Mouse.GetState().ScrollWheelValue);
                prevFrameScrollWheelValue = Mouse.GetState().ScrollWheelValue;
            }

            if (Mouse.GetState().LeftButton == ButtonState.Pressed &&
                ServiceManager.Game.IsActive)
            {
                if (localPlayer.Weapon.CanCharge && ! localPlayer.IsCharging && localPlayer.CanFire())
                {
                    localPlayer.IsCharging = true;
                    ServiceManager.Theater.StartCharging();
                }
                else if (localPlayer.IsCharging || localPlayer.Weapon.IsOverheating)
                {
                    hud.Bars[1].FadeIn();
                    //Do Still charging.
                }
                else if ( !localPlayer.Weapon.CanCharge)
                {
                    Fire();
                }
            }
            else if (localPlayer.IsCharging && Mouse.GetState().LeftButton == ButtonState.Released && localPlayer.CanFire())
            {
                //ServiceManager.Theater.Fire(localPlayer.Turret.TargetPosition.X, localPlayer.Turret.TargetPosition.Y);
                Fire();

                localPlayer.IsCharging = false;
                hud.Bars[1].FadeOut();
            }
        }

        /// <summary>
        /// Helper method for firing a projectile
        /// </summary>
        private void Fire()
        {
            if (localPlayer.CanFire())
            {
                if (localPlayer.Weapon.CanCharge == true && !localPlayer.IsCharging)
                {
                    ServiceManager.Theater.StartCharging();
                    localPlayer.IsCharging = true;
                }
                else if (localPlayer.Weapon.UsesOverHeat)
                {
                    ServiceManager.Theater.Fire(localPlayer.Turret.TargetPosition.X, localPlayer.Turret.TargetPosition.Y);
                    localPlayer.Weapon.OverheatAmount += localPlayer.Weapon.OverheatAmountPerShot;
                    localPlayer.TimeSinceFiring = 0;
                }
                else
                {
                    ServiceManager.Theater.Fire(localPlayer.Turret.TargetPosition.X, localPlayer.Turret.TargetPosition.Y);
                }

                foreach (HUD_Bar bar in hud.Bars)
                {
                    bar.FadeIn();
                }

                localPlayer.ResetWeaponCooldown();
            }            
        }

        /// <summary>
        /// Re-synchronize the client to the server.
        /// </summary>
        private void Resync()
        {            
            ServiceManager.Theater.Move(localPlayer.Position.X, localPlayer.Position.Y,
                localPlayer.DirectionMovement);

            ServiceManager.Theater.Rotate(localPlayer.Angle, localPlayer.DirectionRotation);
        }

        /// <summary>
        /// Helper method for changing the movement of the user. Does nothing if the given
        /// direction is not actually a change.
        /// </summary>
        /// <param name="direction">Direction that the tank is moving towards.</param>
        private void ChangeMovement(VTankObject.Direction direction)
        {
            ChangeMovement(direction, false);
        }

        /// <summary>
        /// Helper method for changing the movement of the user. Does nothing if the given
        /// direction is not actually a change.
        /// </summary>
        /// <param name="direction">Direction that the tank is moving towards.</param>
        /// <param name="force">If set to true, forces a re-sync packet.</param>
        private void ChangeMovement(VTankObject.Direction direction, bool force)
        {
            if (localPlayer.DirectionMovement != direction || force)
            {
                localPlayer.DirectionMovement = direction;
                ServiceManager.Theater.Move(localPlayer.Position.X, localPlayer.Position.Y,
                    direction);
            }
        }

        /// <summary>
        /// Helper method for changing the rotation of the user. Does nothing if the given
        /// direction is not actually a change.
        /// </summary>
        /// <param name="direction">Direction that the tank is rotating towards.</param>
        private void ChangeRotation(VTankObject.Direction direction)
        {
            ChangeRotation(direction, false);
        }

        /// <summary>
        /// Helper method for changing the rotation of the user. Does nothing if the given
        /// direction is not actually a change.
        /// </summary>
        /// <param name="direction">Direction that the tank is rotating towards.</param>
        /// <param name="force">If true, forces the movement packet.</param>
        private void ChangeRotation(VTankObject.Direction direction, bool force)
        {
            if (localPlayer.DirectionRotation != direction || force)
            {
                localPlayer.DirectionRotation = direction;
                ServiceManager.Theater.Rotate(localPlayer.Angle, direction);
            }
        }

        /// <summary>
        /// Updates the HUD based on the values of the player
        /// </summary>
        /// <param name="player"></param>
        private void UpdateHUDValues(PlayerTank player)
        {
            hud.UpdateHealth(player);
            hud.UpdateSecondaryBar(player);
        }

        /// <summary>
        /// Gives the final word to rotate map after countdown has finished
        /// </summary>
        private void CheckForRotate()
        {
            if (!cd.Enabled)
            {
                DoRotation();
            }
        }
        #endregion

        #region Collision Detection
        /// <summary>
        /// Helper method that check for local and non-local collisions. First the local 
        /// client's collision is checked. 
        /// </summary>
        /// <param name="tank"></param>
        /// <returns>True if local collision is detected; false otherwise.</returns>
        private bool PerformCollisionChecks()
        {
            bool collide = DetectCollision(localPlayer);
            if (!collide && stuck && localPlayer.DirectionMovement != VTankObject.Direction.NONE)
            {
                stuck = false;
            }

            if (Constants.DETECT_NONLOCAL_COLLISION)
            {
                // Perform a collision detection check on other players.
                ICollection<PlayerTank> playerList = Players.Values;
                    foreach (PlayerTank playerTank in playerList)
                    {
                        if (playerTank.Name == PlayerManager.LocalPlayerName)
                        {
                            continue;
                        }

                        if (DetectCollision(playerTank))
                        {
                            playerTank.DirectionMovement = VTankObject.Direction.NONE;
                            playerTank.PreviouslyCollidied = true;
                        }
                    }
            }

            return collide;
        }

        /// <summary>
        /// Detects if tank is intersecting a impassible tile
        /// </summary>
        /// <param name="tankObject">Clients Tank</param>
        /// <returns>Collision</returns>
        private bool DetectCollision(PlayerTank tankObject)
        {
            // The 3 below this comment is an offset to make sure we only check for collisions with tiles
            // adjacent to us.  Checks are done to a range of 3 tiles.
            int numTilesX = (viewportRect.X / Constants.TILE_SIZE) + 3;
            int numTilesY = (viewportRect.Y / Constants.TILE_SIZE) + 3;

            int minimumX = (int)(tankObject.Position.X / Constants.TILE_SIZE) - numTilesX;
            int minimumY = (int)(-tankObject.Position.Y / Constants.TILE_SIZE) - numTilesY;
            int maximumX = (int)(tankObject.Position.X / Constants.TILE_SIZE) + numTilesX;
            int maximumY = (int)(-tankObject.Position.Y / Constants.TILE_SIZE) + numTilesY;

            for (int y = minimumY; y < map.Height && y <= maximumY; y++)
            {
                if (y < 0)
                    continue;

                for (int x = minimumX; x < map.Width && x <= maximumX; x++)
                {
                    if (x < 0)
                        continue;

                    Options.KeyBindings keys = ServiceManager.Game.Options.KeySettings;

                    DrawableTile tile = visibleTiles[y * map.Width + x];
                    if (tankObject.Name == PlayerManager.LocalPlayerName)
                    {
                        if (!tile.Passable && (
                            (Keyboard.GetState().IsKeyDown(keys.Forward) && 
                                tankObject.FrontSphere.Intersects(tile.BoundingBox)) ||
                            (Keyboard.GetState().IsKeyDown(keys.Backward) && 
                                tankObject.BackSphere.Intersects(tile.BoundingBox))))
                        {
                            return true;
                        }
                    }
                    else
                    {
                        if (!tile.Passable && 
                            ((tankObject.FrontSphere.Intersects(tile.BoundingBox) && 
                            tankObject.DirectionMovement == VTankObject.Direction.FORWARD) ||
                             (tankObject.BackSphere.Intersects(tile.BoundingBox) &&
                             tankObject.DirectionMovement == VTankObject.Direction.REVERSE)))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }
        #endregion

        #region Utility
        /// <summary>
        /// Test Method for formating a string to draw with
        /// </summary>
        /// <param name="timeLeft"></param>
        /// <returns></returns>
        private String FormatTimeLeft(double timeLeft)
        {
            return Network.Util.Clock.FormatTime(timeLeft);
        }

        /// <summary>
        /// Calculates parameters for tiles with height and creates DrawableTiles from them
        /// </summary>
        private void FillTiles()
        {
            Random random = new Random();
            Dictionary<uint, int> borderTileCounter = new Dictionary<uint, int>();
            for (uint y = 0; y < map.Height; y++)
            {
                if (borderTileCounter.ContainsKey(map.GetTile(0, y).ID) && map.GetTile(0, y).Height ==0)
                    borderTileCounter[map.GetTile(0, y).ID]++;
                else if( map.GetTile(0, y).Height ==0)
                    borderTileCounter.Add(map.GetTile(0, y).ID, 1);

                if (borderTileCounter.ContainsKey(map.GetTile(map.Width - 1, y).ID) && map.GetTile(map.Width - 1, y).Height == 0)
                    borderTileCounter[map.GetTile(map.Width-1, y).ID]++;
                else if (map.GetTile(map.Width - 1, y).Height == 0)
                    borderTileCounter.Add(map.GetTile(map.Width-1, y).ID, 1);
            }
            for (uint x = 0; x < map.Width-1; x++)
            {
                if (borderTileCounter.ContainsKey(map.GetTile(x, 0).ID) && map.GetTile(x, 0).Height ==0)
                    borderTileCounter[map.GetTile(x, 0).ID]++;
                else if (map.GetTile(x, 0).Height == 0)
                    borderTileCounter.Add(map.GetTile(x, 0).ID, 1);

                if (borderTileCounter.ContainsKey(map.GetTile(x, map.Height - 1).ID) && map.GetTile(x, map.Height - 1).Height == 0)
                    borderTileCounter[map.GetTile(x, map.Height - 1).ID]++;
                else if (map.GetTile(x, map.Height - 1).Height == 0)
                    borderTileCounter.Add(map.GetTile(x, map.Height - 1).ID, 1);
            }
            int maxValue = 0; uint maxValueTile = 0;
            foreach (KeyValuePair<uint, int> element in borderTileCounter)
            {
                if (element.Value > maxValue)
                {
                    maxValue = element.Value;
                    maxValueTile = element.Key;
                }
            }
            //Load floor 
            float minX = -10000f; float maxX = 100000f;
            float minY = -100000f; float maxY = 10000f;
            float Xspread = (maxX - minX) / Constants.TILE_SIZE;
            float Yspread = (maxY - minY) / Constants.TILE_SIZE;
            List<VertexPositionNormalTexture> verts = new List<VertexPositionNormalTexture>();
            verts.Add(new VertexPositionNormalTexture(new Vector3(minX, minY, -0.75f), Vector3.UnitZ, Vector2.UnitY * Yspread));
            verts.Add(new VertexPositionNormalTexture(new Vector3(minX, maxY, -0.75f), Vector3.UnitZ, Vector2.Zero));
            verts.Add(new VertexPositionNormalTexture(new Vector3(maxX, minY, -0.75f), Vector3.UnitZ, new Vector2(Xspread, Yspread)));
            verts.Add(new VertexPositionNormalTexture(new Vector3(maxX, maxY, -0.75f), Vector3.UnitZ, Vector2.UnitX * Xspread));
            verts.Add(new VertexPositionNormalTexture(new Vector3(maxX, minY, -0.75f), Vector3.UnitZ, new Vector2(Xspread, Yspread)));
            verts.Add(new VertexPositionNormalTexture(new Vector3(minX, maxY, -0.75f), Vector3.UnitZ, Vector2.Zero));
            VertexGroup floor = new VertexGroup(TileList.GetTile((int)maxValueTile), verts);
            floor.Technique = RendererAssetPool.UniversalEffect.Techniques.TexturedWrap;
            floor.CastsShadow = false;
            floor.TransparencyEnabled = false;
            floor.Ready();
            renderer.ActiveScene.Add(floor, 0);
            //Loads the tiles into the background array
            for (uint y = 0; y < map.Height; y++)
            {
                for (uint x = 0; x < map.Width; x++)
                {
                    Tile tempTile = map.GetTile(x, y);
                    // Figure out the height of each tile in each direction relative to this one.

                    int positionNorth = (int)(y - 1);
                    int positionSouth = (int)(y + 1);
                    int positionWest = (int)(x - 1);
                    int positionEast = (int)(x + 1);
                    if (positionNorth >= 0)
                        tempTile.heightN = map.GetTile(x, y - 1).Height;
                    if (positionSouth < map.Height)
                        tempTile.heightS = map.GetTile(x, y + 1).Height;
                    if (positionWest >= 0)
                        tempTile.heightW = map.GetTile(x - 1, y).Height;
                    if (positionEast < map.Width)
                        tempTile.heightE = map.GetTile(x + 1, y).Height;
                    map.SetTile(x, y, tempTile);

                    Texture2D texture = TileList.GetTile((int)tempTile.ID);
                    DrawableTile tile = new DrawableTile(
                        texture, tempTile, (int)x, (int)y,
                        tempTile.heightN, tempTile.heightW, tempTile.heightE, tempTile.heightS);

                    Vector3 pos = new Vector3(
                        x * Constants.TILE_SIZE + (Constants.TILE_SIZE / 2),
                        -(y * Constants.TILE_SIZE + (Constants.TILE_SIZE / 2)),
                        0);
                    if (tempTile.ObjectID != 0)
                    {
                        
                        Model objectModel = TileList.GetObject(tempTile.ObjectID);
                        Object3 newTileObj = new Object3(objectModel, pos + (Vector3.UnitZ * tempTile.Height * Constants.TILE_SIZE));
                        newTileObj.TransparencyEnabled = true;
                        Scene.Add(newTileObj, 1);
                    }

                    switch (tempTile.EventID)
                    {
                        case 4:
                        case 5:
                            GameSession.Alliance team = tempTile.EventID == 4 ?
                                GameSession.Alliance.RED : GameSession.Alliance.BLUE;
                            if (currentGameMode == VTankObject.GameMode.CAPTURETHEFLAG)
                            {
                                Flags.AddFlag(team, pos);
                            }
                            break;
                        case 8: case 9: case 10:
                            if (currentGameMode == VTankObject.GameMode.CAPTURETHEBASE)
                            {
                                Bases.AddBase(GameSession.Alliance.BLUE, tempTile.EventID, pos);
                            }
                            break;
                        case 11: case 12: case 13:
                            if (currentGameMode == VTankObject.GameMode.CAPTURETHEBASE)
                            {
                                Bases.AddBase(GameSession.Alliance.RED, tempTile.EventID, pos);
                            }
                            break;
                        default:
                            break;
                    }
                    visibleTiles[y * map.Width + x] = tile;
                }
            }


            #region Make Flat Tiles
            for (uint y = 0; y < map.Height; y++)
            {
                for (uint x = 0; x < map.Width; x++)
                {
                    Tile tempTile = map.GetTile(x, y);
                    Vector3 pos = new Vector3(x * Constants.TILE_SIZE, (-(y + 1) * Constants.TILE_SIZE), 0);
                    Tiles.AddFloor(pos, tempTile.Height, (int)tempTile.ID);
                }
            }
            #endregion

            #region Make North Walls
            int height = 0;
            int tileID = 0;
            int width = 0; 
            int Hdir = 0;
            //Make north facing walls
            for (uint y = 0; y < map.Height; y++)
            {
                for (uint x = 0; x < map.Width; x++)
                {
                    Tile tempTile = map.GetTile(x, y);
                    if ((width == 0 || (tempTile.Height == height && tempTile.ID == tileID && Hdir == tempTile.heightN)) && (x + 1) < map.Width)
                    {
                        height = tempTile.Height;
                        tileID = (int)tempTile.ID;
                        Hdir = tempTile.heightN;
                        width++;
                    }
                    else 
                    {
                        if (height > Hdir)
                        {
                            Vector3 pos = new Vector3(x * Constants.TILE_SIZE, (-y * Constants.TILE_SIZE), 0);
                            Tiles.AddWall(pos, Vector3.UnitY, width, height, tileID);
                        }
                        width = 1; height = tempTile.Height; tileID = (int)tempTile.ID; Hdir = tempTile.heightN;
                    }
                }
                width = 0;
            }
            #endregion

            #region Make South Walls
            height = 0;
            tileID = 0;
            width = 0; 
            Hdir = 0;
            uint startX = 0;
            //Make south facing walls
            for (uint y = 0; y < map.Height; y++)
            {
                for (uint x = 0; x < map.Width; x++)
                {
                    Tile tempTile = map.GetTile(x, y);
                    if ((width == 0 || (tempTile.Height == height && tempTile.ID == tileID && Hdir == tempTile.heightS)) && (x + 1) < map.Width)
                    {
                        if (width == 0)
                        {
                            startX = x;
                        }
                        height = tempTile.Height;
                        tileID = (int)tempTile.ID;
                        Hdir = tempTile.heightS;
                        width++;
                    }
                    else
                    {
                        if (height > Hdir)
                        {
                            Vector3 pos = new Vector3(startX * Constants.TILE_SIZE, (-(y+1) * Constants.TILE_SIZE), 0);
                            Tiles.AddWall(pos, -Vector3.UnitY, width, height, tileID);
                        }
                        width = 1; height = tempTile.Height; tileID = (int)tempTile.ID; Hdir = tempTile.heightS;
                        startX = x;
                    }
                }
                width = 0;
            }

            #endregion

            #region Make East Walls
            height = 0;
            tileID = 0;
            width = 0;
            Hdir = 0;
            //Make east facing walls
            for (uint x = 0; x < map.Width; x++)
            {
                for (uint y = 0; y < map.Height; y++)
                {
                    Tile tempTile = map.GetTile(x, y);
                    if ((width == 0 || (tempTile.Height == height && tempTile.ID == tileID && Hdir == tempTile.heightE)) && (y + 1) < map.Height)
                    {
                        height = tempTile.Height;
                        tileID = (int)tempTile.ID;
                        Hdir = tempTile.heightE;
                        width++;
                    }
                    else
                    {
                        if (height > Hdir)
                        {
                            Vector3 pos = new Vector3((x+1) * Constants.TILE_SIZE, (-y * Constants.TILE_SIZE), 0);
                            Tiles.AddWall(pos, Vector3.UnitX, width, height, tileID);
                        }
                        width = 1; height = tempTile.Height; tileID = (int)tempTile.ID; Hdir = tempTile.heightE;
                    }
                }
                width = 0;
            }

            #endregion

            #region Make West Walls
            height = 0;
            tileID = 0;
            width = 0;
            Hdir = 0;
            uint startY = 0;
            //Make south facing walls
            for (uint x = 0; x < map.Width; x++)
            {
                for (uint y = 0; y < map.Height; y++)
                {
                    Tile tempTile = map.GetTile(x, y);
                    if ((width == 0 || (tempTile.Height == height && tempTile.ID == tileID && Hdir == tempTile.heightW)) && (y+1) < map.Height)
                    {
                        if (width == 0)
                        {
                            startY = y;
                        }
                        height = tempTile.Height;
                        tileID = (int)tempTile.ID;
                        Hdir = tempTile.heightW;
                        width++;
                    }
                    else
                    {
                        if (height > Hdir)
                        {
                            Vector3 pos = new Vector3(x * Constants.TILE_SIZE, (-startY * Constants.TILE_SIZE), 0);
                            Tiles.AddWall(pos, -Vector3.UnitX, width, height, tileID);
                        }
                        width = 1; height = tempTile.Height; tileID = (int)tempTile.ID; Hdir = tempTile.heightW;
                        startY = y;
                    }
                }
                width = 0;
            }

            #endregion







            Tiles.AllReady();
            ////////////////////////////////////////////
            ///TODO:::: FOR EACH TILE GOING LEFT TO RIGHT
            //
            //   IF height = height  and tileID = tileID 
            //       if Hnorth = Hnorth
            //          NorthWidth ++; 
            //       else 
            //          TexturedTileGroupManager.AddWall(lowerLeftNorth, Vector3.UnitY, NorthWidth, height, tileID)
            //          NorthWidth = 0;
            //          height = -1
            //          

            //       if Hsouth = Hsouth
            //          SouthWidth ++; 
            //       else 
            //          TexturedTileGroupManager.AddWall(lowerLeftSouth, -Vector3.UnitY, SouthhWidth, height, tileID)
            //etc
        }

        /// <summary>
        /// This method is called by the game callback when the game should rotate to the
        /// next map.
        /// </summary>
        public void RotateMap()
        {
            Utilities.Clear();

            cd.StartCD(Countdown.CountdownType.MapChange);

            rotating = true;
            mouseCursor.DisableCustomCursor();
            this.Scores.CheckForRankUps();
        }

        /// <summary>
        /// Perform the actual map rotation.
        /// </summary>
        private void DoRotation()
        {
            input.Visible = false;

            if (ServiceManager.CurrentServer != null)
            {
                ServiceManager.Game.Renderer.ActiveScene.DeleteCamera(OverheadCamera);
                ServiceManager.Game.Renderer.ActiveScene.DeleteCamera(ChaseCamera);
                ServiceManager.StateManager.ChangeState(
                    new LoadingScreenState(ServiceManager.CurrentServer));
            }
        }

        /// <summary>
        /// This method is called by the game callback when the player has been killed
        /// </summary>
        public void StartRespawnTimer()
        {
            cd.StartCD(Countdown.CountdownType.Respawn);
        }

        /// <summary>
        /// This method is called by the game callback when the player has been hit
        /// </summary>
        public void PlayerHit()
        {
            foreach (HUD_Bar bar in hud.Bars)
            {
                bar.FadeIn();
            }
        }
        #endregion
    }
}
