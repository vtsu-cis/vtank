/*!
    \file   VTank.cs
    \brief  The "main" Game object for XNA.
    \author (C) Copyright 2009 by Vermont Technical College
*/

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Storage;
using Client.src.states;
using Client.src.config;
using Client.src.states.gamestate;
using Client.src.callbacks;
using GameForms.Forms;
using TomShane.Neoforce.Controls;
using Client.src.util;
using Renderer;
using Renderer.SceneTools.Entities;
using System.IO;
using Client.src.util.game;
using Client.src.events;

namespace Client.src.service.services
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class VTank : Microsoft.Xna.Framework.Game
    {
        #region Members
        public static string MENU_PATH;
        private const string DefaultCameraName = "Tank Display View";

        // TODO: This stuff should be in the services right?
        public Manager Manager;
        public FormManager FormManager;

        private long lastTimeStamp;
        private RenderTarget2D screenshotTarget;
        private InvocationBuffer invocationBuffer;
        #endregion

        #region Properties
        /// <summary>
        /// Allow public access to the sprite batch, which is needed for external drawing (by states).
        /// </summary>
        public SpriteBatch Batch
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the renderer which draws 2D and 3D objects.
        /// </summary>
        public EntityRenderer Renderer
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the graphics device manager for this program.
        /// </summary>
        public GraphicsDeviceManager GraphicsDeviceManager
        {
            get;
            private set;
        }

        /// <summary>
        /// Shortcut for getting the graphics device.
        /// </summary>
        public new GraphicsDevice GraphicsDevice
        {
            get
            {
                return GraphicsDeviceManager.GraphicsDevice;
            }
        }

        /// <summary>
        /// Gets or sets the font used for this program.
        /// </summary>
        public SpriteFont Font
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the options available for this program.
        /// </summary>
        public Options Options
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the console which logs debug information and allows the use rto input
        /// debugging commands.
        /// </summary>
        public GameConsole Console
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the delta time for this frame in seconds.
        /// </summary>
        public double DeltaTime
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the screen width.
        /// </summary>
        public int Width
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the screen height.
        /// </summary>
        public int Height
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets whether or not the game is running slowly. See the documentation at:
        /// GameTime#IsRunningSlowly.
        /// </summary>
        public bool IsRunningSlowly
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the background animation.
        /// </summary>
        public LoopingMovieRunner BackgroundMovie
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the invocation buffer, used to invoke event callbacks on the GUI thread.
        /// </summary>
        public InvocationBuffer InvocationBuffer
        {
            get;
            private set;
        }
        #endregion

        #region Constructors

        /// <summary>
        /// Game constructor.  Creates a new GraphicsDeviceManager and sets the root directory to "Content".
        /// </summary>
        public VTank()
        {
            GraphicsDeviceManager = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            Manager = new Manager(this, GraphicsDeviceManager);
            GraphicsDeviceManager.PreparingDeviceSettings += this.graphics_PreparingDeviceSettings;

            FormManager = new FormManager(GraphicsDeviceManager, this, Manager);
            Manager.SkinDirectory = @"Content\Skins\";

            // Set the resolution and full screen values in constructor so that it starts
            // with these values without needing graphics.ApplyChanges().
            Options = Options.ReadOptions();
            invocationBuffer = new InvocationBuffer();
            string resolution = Options.Video.Resolution;
            int width = int.Parse(resolution.Substring(0, resolution.IndexOf("x")));
            int height = int.Parse(resolution.Substring(resolution.IndexOf("x") + 1));

            GraphicsDeviceManager.PreferredBackBufferWidth = width;
            GraphicsDeviceManager.PreferredBackBufferHeight = height;
            GraphicsDeviceManager.IsFullScreen = !Options.Video.Windowed;

            Width = width;
            Height = height;
        }
        void graphics_PreparingDeviceSettings(object sender, PreparingDeviceSettingsEventArgs e)
        {
            foreach (GraphicsAdapter curAdapter in GraphicsAdapter.Adapters)
            {
                if (curAdapter.Description.Contains("PerfHUD"))
                {
                    e.GraphicsDeviceInformation.Adapter = curAdapter;
                    e.GraphicsDeviceInformation.DeviceType = DeviceType.Reference;
                    break;
                }
            }
            return;
        }

        #endregion

        #region Methods
        /// <summary>
        /// Create the background movie.
        /// </summary>
        public void CreateBackgroundMovie()
        {
            try
            {
                BackgroundMovie = new LoopingMovieRunner(GraphicsDevice);

                const float RATIO_4_3 = 1.33333f;
                float aspectRatio = GraphicsDevice.Viewport.AspectRatio;
                
                string filename;
                if (Math.Abs(aspectRatio - RATIO_4_3) < 0.001 || !GraphicsAdapter.DefaultAdapter.IsWideScreen)
                {
                    // Aspect ratio is 4:3
                    filename = Path.GetFullPath(@"Content\video\background4-3.wmv");
                }
                else
                {
                    // Load wide screen animation.
                    filename = Path.GetFullPath(@"Content\video\background16-9.wmv");
                }

                BackgroundMovie.Play(filename);
            }
            catch (Exception ex)
            {
                System.Console.Error.WriteLine(
                    "[ERROR] Cannot load background animation: {0}", ex.Message);
            }
        }

        /// <summary>
        /// Convenience method for switching the game back to it's default camera.
        /// Useful for when switching back to the tank list viewer state.
        /// </summary>
        public void SwitchToDefaultCamera()
        {
            ServiceManager.Game.Renderer.ActiveScene.SwitchCamera(DefaultCameraName);
        }

        /// <summary>
        /// Render the current scene as a texture and return it.
        /// </summary>
        /// <returns>Screenshot of the current screen.</returns>
        public Texture2D TakeScreenshot()
        {
            GraphicsDevice.SetRenderTarget(0, screenshotTarget);

            GameTime gameTime = new GameTime();
            Draw(gameTime);

            Texture2D result = screenshotTarget.GetTexture();

            GraphicsDevice.SetRenderTarget(0, null);

            return result;
        }

        /// <summary>
        /// Play the MP3 player's playlist.
        /// </summary>
        /// <returns>True if the playlist was found and is playing; false otherwise.</returns>
        public bool PlayPlaylist()
        {
            try
            {
                return ServiceManager.MP3Player.PlayPlaylist();
            }
            catch (Exception ex)
            {
                System.Console.Error.WriteLine(ex);
                return false;
            }
        }

        /// <summary>
        /// Invoke a method callback on the GUI thread. This behavior is thread-safe.
        /// </summary>
        public void Invoke(Client.src.events.Invocation.InvocationTarget target, object parameter)
        {
            invocationBuffer.Enqueue(target, parameter);
        }

        /// <summary>
        /// Accepts a string in the format of "2x" or "4x" (referring to the number of
        /// multisamples), and spits back an enumeration value. Only accepts 2x, 4x, 8x,
        /// 16x, and 0x.
        /// </summary>
        /// <param name="preferenceString">Anti-aliasing preference string in the form of
        /// "2x", "4x", "8x", "16x", or "off".</param>
        /// <returns>The gathered multisample type enumeration. Returns 2x by default.</returns>
        private MultiSampleType GetMultiSampleType(string preferenceString)
        {
            switch (preferenceString)
            {
                case "0":
                case "0x":
                case "off":
                    return MultiSampleType.None;
                case "2x":
                    return MultiSampleType.TwoSamples;
                case "3x":
                    return MultiSampleType.ThreeSamples;
                case "4x":
                    return MultiSampleType.FourSamples;
                case "5x":
                    return MultiSampleType.FiveSamples;
                case "6x":
                    return MultiSampleType.SixSamples;
                case "7x":
                    return MultiSampleType.SevenSamples;
                case "8x":
                    return MultiSampleType.EightSamples;
                case "16x":
                    return MultiSampleType.SixteenSamples;
                default:
                    return MultiSampleType.TwoSamples;
            }
        }
        #endregion

        #region Overridden Methods

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            IsMouseVisible = true;

            //Manager.Initialize();
            ServiceManager.CreateResourceManager();
            ServiceManager.CreateAudioManager();
            ServiceManager.CreateMP3Player();
            ServiceManager.CreateStateManager();
            ServiceManager.CreateLogger();
            base.Initialize();

            PresentationParameters pp = GraphicsDevice.PresentationParameters;

            const int borderPadding = 20;
            Console = new GameConsole(new Rectangle(
                borderPadding, borderPadding,
                GraphicsDeviceManager.PreferredBackBufferWidth - borderPadding, 
                GraphicsDeviceManager.PreferredBackBufferHeight - borderPadding));

            Options.VideoOptions video = Options.Video;
            // Set anti-aliasing options.
            string antialiasing = video.AntiAliasing.ToLower();
            if (antialiasing == "off")
            {
                pp.MultiSampleQuality = 0;
                pp.MultiSampleType = MultiSampleType.None;
                GraphicsDeviceManager.PreferMultiSampling = false;
            }
            else
            {
                GraphicsDeviceManager.PreparingDeviceSettings += new EventHandler<PreparingDeviceSettingsEventArgs>(
                    GraphicsDeviceManager_PreparingDeviceSettings);
            }

            GraphicsDeviceManager.DeviceReset += new System.EventHandler(GraphicsDeviceManager_DeviceReset);
            // TODO: Don't forget other options.

            GraphicsDeviceManager.ApplyChanges();
            //ServiceManager.Game.GraphicsDevice.GraphicsDeviceCapabilities.MaxPointSize;

            Renderer = new EntityRenderer(GraphicsDeviceManager, Content);
            Renderer.SceneTools.Entities.Camera cam = Renderer.ActiveScene.CurrentCamera;
            Camera camera = Renderer.ActiveScene.CreateCamera(
                new Vector3(100, 100, 60),  // Position
                Vector3.Backward * 20,      // Target
                cam.Projection,             // Projection
                DefaultCameraName           /* Name */);
            camera.CameraUp = Vector3.UnitZ;
            camera.Updatable = false;

            lastTimeStamp = Network.Util.Clock.GetTimeMilliseconds();
            
            screenshotTarget = new RenderTarget2D(GraphicsDevice,
                pp.BackBufferWidth, pp.BackBufferHeight, 1, GraphicsDevice.DisplayMode.Format);

            WeaponLoader.LoadFiles();

            ServiceManager.StateManager.ChangeState<LoginScreenState>();

            ServiceManager.Game.GraphicsDevice.RenderState.PointSizeMax =
                ServiceManager.Game.GraphicsDevice.GraphicsDeviceCapabilities.MaxPointSize;

            Console.DebugPrint("Max Point Size: " + ServiceManager.Game.GraphicsDevice.GraphicsDeviceCapabilities.MaxPointSize);

            if (ServiceManager.MP3Player != null)
            {
                MENU_PATH = Path.Combine(
                    ServiceManager.MP3Player.AudioDirectory, @"official\menu.wav");
                if (!ServiceManager.MP3Player.Play(MENU_PATH, true))
                {
                    Console.DebugPrint("[WARNING] Cannot play file official\\menu.wav (not found).");
                }
            }
        }

        /// <summary>
        /// Event handler called by XNA when the graphic device manager *has*
        /// reset itself.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void GraphicsDeviceManager_DeviceReset(object sender, System.EventArgs e)
        {
            Width = GraphicsDeviceManager.PreferredBackBufferWidth;
            Height = GraphicsDeviceManager.PreferredBackBufferHeight;
        }
        
        /// <summary>
        /// Event handler called by XNA when graphic settings are being prepared. This is where
        /// we manage what level of antialiasing is set.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void GraphicsDeviceManager_PreparingDeviceSettings(object sender, 
            PreparingDeviceSettingsEventArgs e)
        {
            try
            {
                PresentationParameters pp = GraphicsDevice.PresentationParameters;
                MultiSampleType preferredType = GetMultiSampleType(Options.Video.AntiAliasing.ToLower());
                GraphicsAdapter adapter = e.GraphicsDeviceInformation.Adapter;
                SurfaceFormat format = adapter.CurrentDisplayMode.Format;
                int quality = 0;

                if (adapter.CheckDeviceMultiSampleType(DeviceType.Hardware, format, !Options.Video.Windowed,
                    preferredType, out quality))
                {
                    // The MultiSampleQuality value is specific to graphic drivers and it's not clear what
                    // exactly it does. Setting it to zero is the best option.
                    pp.MultiSampleQuality = 0;
                    pp.MultiSampleType = preferredType;
                }
                else if (adapter.CheckDeviceMultiSampleType(DeviceType.Hardware, format, !Options.Video.Windowed,
                    MultiSampleType.TwoSamples, out quality))
                {
                    // The MultiSampleQuality value is specific to graphic drivers and it's not clear what
                    // exactly it does. Setting it to zero is the best option.
                    pp.MultiSampleQuality = 0;
                    pp.MultiSampleType = MultiSampleType.TwoSamples;
                    Console.DebugPrint("[WARNING] Preferred AA {0} is not supported by the current hardware, " +
                        "defaulting to 2x.",
                        Options.Video.AntiAliasing);
                }
                else
                {
                    pp.MultiSampleQuality = 0;
                    pp.MultiSampleType = MultiSampleType.None;
                    Console.DebugPrint("[WARNING] AA is not supported by this hardware; using 0 samples.");
                }
            }
            catch (Exception ex)
            {
                System.Console.Error.WriteLine(ex);
            }
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            Batch = new SpriteBatch(GraphicsDevice);
            Font = ServiceManager.Resources.GetFont("tahoma");

            ServiceManager.StateManager.CurrentState.LoadContent();

            base.LoadContent();
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            try
            {
                BackgroundMovie.UnloadContent();
            }
            catch (Exception) { }

            ServiceManager.StateManager.CurrentState.UnloadContent();
            base.UnloadContent();
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            IsRunningSlowly = gameTime.IsRunningSlowly;

            BackgroundMovie.Update();

            while (invocationBuffer.Count > 0)
                invocationBuffer.InvokeNext();

            long currentTimeStamp = Network.Util.Clock.GetTimeMilliseconds();
            long elapsed = currentTimeStamp - lastTimeStamp;
            DeltaTime = ((double)elapsed) / 1000.0;
            lastTimeStamp = currentTimeStamp;

            if (DeltaTime > 1.0)
                return;

            if (ServiceManager.MP3Player != null)
            {
                ServiceManager.MP3Player.Update();
            }
            
            GraphicOptions.FrameLength = (float)DeltaTime;

            ServiceManager.StateManager.CurrentState.Update();
            
            Console.Update();

            try
            {
                base.Update(gameTime);
            }
            catch (Exception e)
            {
                // Ignore messages relating to NeoForce.
                // TODO: Figure out why NeoForce throws exceptions.
                if (!e.StackTrace.ToString().ToLower().Contains("neoforce"))
                {
                    System.Console.Error.WriteLine(e);
                }
            }
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer,
                Color.Black, 1.0f, 0);

            ServiceManager.StateManager.CurrentState.Draw();
            
            Console.Draw();
            base.Draw(gameTime);
        }

        #endregion
    }
}
