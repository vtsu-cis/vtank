using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;
using Renderer.SceneTools.Entities;
using SkinnedModel;

namespace DynamicModelViewer
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        Renderer.EntityRenderer renderer;
        ContentBuilder contentBuilder;
        int other, tank, turret, powerup, emitter;
        bool overhead, camChanged, emitterAdded, shadowChanged, lightChanged;
        float colorTimer;
        enum ColorStateEnum { RED, GREEN, BLUE };
        ColorStateEnum colorState;
        Color meshColor;
        float percentDay = 0.0f;

        AnimationPlayer animationPlayer;
        Dictionary<string, AnimationClip> animations;


        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            contentBuilder = new ContentBuilder();
            overhead = true;
            emitterAdded = false;
            colorTimer = 0;
            colorState = ColorStateEnum.RED;
            meshColor = Color.Blue;
            graphics.PreparingDeviceSettings += this.graphics_PreparingDeviceSettings;
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
        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            Content.RootDirectory = Content.RootDirectory + "/Content";
            renderer = new Renderer.EntityRenderer(graphics, Content);
            other = -1; tank = -1; turret = -1; powerup = -1; emitter = -1;
            this.IsMouseVisible = true;
            // TODO: Add your initialization logic here

            base.Initialize();
        }
        #region Load
        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            LoadCameras();
            LoadTextures();
            FillTiles();
        }

        void LoadSampleEmitter()
        {
            Renderer.SceneTools.Entities.Particles.ParticleSystemSettings newPSS = new Renderer.SceneTools.Entities.Particles.ParticleSystemSettings();
            StreamReader reads = new StreamReader(Path.Combine(Content.RootDirectory, "Sample.vtpss"));
            newPSS.Load(reads);
            reads.Close();
            Renderer.RendererAssetPool.ParticleSystemSettings.Add("Sample", newPSS);

            Renderer.SceneTools.Entities.Particles.ParticleEmitterSettings newPES = new Renderer.SceneTools.Entities.Particles.ParticleEmitterSettings();
            StreamReader reade = new StreamReader(Path.Combine(Content.RootDirectory, "Sample.vtpes"));
            newPES.Load(reade);
            reade.Close();
            Renderer.RendererAssetPool.ParticleEmitterSettings.Clear();
            Renderer.RendererAssetPool.ParticleEmitterSettings.Add("Sample", newPES);
        }
        void LoadCameras()
        {
            renderer.ActiveScene.CreateCamera(
                   new Vector3(0, 0, 800),
                       Vector3.Zero,
                   Matrix.CreatePerspectiveFieldOfView(
                       MathHelper.PiOver4,
                       (float)Renderer.GraphicOptions.graphics.GraphicsDevice.Viewport.Width /
                           (float)Renderer.GraphicOptions.graphics.GraphicsDevice.Viewport.Height,
                       10f,
                       5000f),
                   "Overhead");

            renderer.ActiveScene.CreateCamera(
                Vector3.Up * 300 + Vector3.Backward * 100,
                new Vector3(0, 0, 50),
                renderer.ActiveScene.AccessCamera("Overhead").Projection,
                "Chase");
            renderer.ActiveScene.AccessCamera("Chase").CameraUp = Vector3.UnitZ;

            renderer.ActiveScene.CurrentCamera = renderer.ActiveScene.AccessCamera("Overhead");
        }

        void LoadTextures()
        {
            string dir = Path.Combine(System.Reflection.Assembly.GetExecutingAssembly().Location, "../Content/particles");
            DirectoryInfo di = new DirectoryInfo(dir);
            FileInfo[] files = di.GetFiles("*.xnb");
            foreach (System.IO.FileInfo file in files)
            {
                string name = Path.GetFileNameWithoutExtension(file.Name);
                Renderer.RendererAssetPool.Particles.Add(name, Content.Load<Texture2D>("particles\\" + name));
            }
        }
        #endregion
        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            InputHandler();
            Renderer.GraphicOptions.FrameLength = (float)gameTime.ElapsedGameTime.TotalSeconds;
            renderer.Update();
            colorTimer += gameTime.ElapsedGameTime.Milliseconds;
            if (colorTimer > 10.0f)
            {
                colorTimer = 0.0f;
                switch (colorState)
                {
                    case ColorStateEnum.RED:
                        if (meshColor.R == 255) colorState = ColorStateEnum.GREEN;
                        else { meshColor.R++; meshColor.B--; }
                        break;
                    case ColorStateEnum.GREEN:
                        if (meshColor.G == 255) colorState = ColorStateEnum.BLUE;
                        else { meshColor.G++; meshColor.R--; }
                        break;
                    case ColorStateEnum.BLUE:
                        if (meshColor.B == 255) colorState = ColorStateEnum.RED;
                        else { meshColor.B++; meshColor.G--; }
                        break;
                }


                if (tank != -1)
                {
                    renderer.ActiveScene.Access3D(tank).MeshColor = meshColor;
                }

                if (turret != -1)
                {
                    renderer.ActiveScene.Access3D(turret).MeshColor = meshColor;
                }
            }
            //base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            renderer.Draw();
            base.Draw(gameTime);
        }

        void InputHandler()
        {
            // Allows the game to exit
            if (Keyboard.GetState().IsKeyDown(Keys.Q))
                this.Exit();
            //Load tank model

            if (Keyboard.GetState().IsKeyDown(Keys.C))
            {
                if (!camChanged && overhead)
                {
                    renderer.ActiveScene.CurrentCamera = renderer.ActiveScene.AccessCamera("Chase");
                    overhead = false;
                }
                else if (!camChanged)
                {
                    renderer.ActiveScene.CurrentCamera = renderer.ActiveScene.AccessCamera("Overhead");
                    overhead = true;
                }
                camChanged = true;
            }
            if (Keyboard.GetState().IsKeyUp(Keys.C))
            { camChanged = false; }

            if (Keyboard.GetState().IsKeyDown(Keys.S))
            {
                if (!shadowChanged)
                {
                    Renderer.RendererAssetPool.DrawShadows = !Renderer.RendererAssetPool.DrawShadows;
                    shadowChanged = true;
                }
            }
            else
            { shadowChanged = false; }

            if (Keyboard.GetState().IsKeyDown(Keys.L))
            {
                if (!lightChanged)
                {
                    Renderer.GraphicOptions.ShadingSupport = !Renderer.GraphicOptions.ShadingSupport;
                    lightChanged = true;
                }
            }
            else
            { lightChanged = false; }

            if (Keyboard.GetState().IsKeyDown(Keys.Add))
            {
                percentDay = (percentDay + 0.01f);
                if (percentDay > 1.0) percentDay = 0.0f;
                renderer.ActiveScene.PercentOfDayComplete = percentDay;
            }

            if (Keyboard.GetState().IsKeyDown(Keys.Right) && renderer.ActiveScene.Access3D(tank) != null)
                renderer.ActiveScene.Access3D(tank).Translate(Vector3.UnitX);
            if (Keyboard.GetState().IsKeyDown(Keys.Left) && renderer.ActiveScene.Access3D(tank) != null)
                renderer.ActiveScene.Access3D(tank).Translate(-Vector3.UnitX);

            if (Keyboard.GetState().IsKeyDown(Keys.RightControl) || Keyboard.GetState().IsKeyDown(Keys.LeftControl))
            {
                if (Keyboard.GetState().IsKeyDown(Keys.T))
                {
                    renderer.ActiveScene.Delete(other);
                    renderer.ActiveScene.Delete(tank);
                    renderer.ActiveScene.Delete(powerup);
                    renderer.ActiveScene.Delete(emitter);
                    other = -1; tank = -1; powerup = -1; emitter = -1;
                    LoadFile("Load Tank", "ModelProcessor", ref tank);
                    if (tank != -1 && turret != -1)
                    {
                        renderer.ActiveScene.Access3D(turret).Attach(renderer.ActiveScene.Access3D(tank), "Mount");
                    }
                        renderer.ActiveScene.MainEntity = renderer.ActiveScene.Access3D(tank);
                }
                else if (Keyboard.GetState().IsKeyDown(Keys.A))
                {
                    renderer.ActiveScene.Delete(other);
                    renderer.ActiveScene.Delete(tank);
                    renderer.ActiveScene.Delete(powerup);
                    renderer.ActiveScene.Delete(emitter);
                    other = -1; tank = -1; powerup = -1; emitter = -1;
                    LoadFile("Load Animation", "SkinnedModelProcessor", ref other);
                    if (tank != -1 && turret != -1)
                    {
                        renderer.ActiveScene.Access3D(turret).Attach(renderer.ActiveScene.Access3D(tank), "Mount");
                    }
                }
                else if (Keyboard.GetState().IsKeyDown(Keys.W))
                {
                    renderer.ActiveScene.Delete(other);
                    renderer.ActiveScene.Delete(turret);
                    renderer.ActiveScene.Delete(powerup);
                    renderer.ActiveScene.Delete(emitter);
                    other = -1; turret = -1; powerup = -1; emitter = -1;
                    LoadFile("Load Weapon", "ModelProcessor", ref turret);
                    if (tank != -1 && turret != -1)
                    {
                        renderer.ActiveScene.Access3D(turret).Attach(renderer.ActiveScene.Access3D(tank), "Mount");
                    }
                }
                else if (Keyboard.GetState().IsKeyDown(Keys.P))
                {
                    renderer.ActiveScene.Clear(1);
                    tank = -1; turret = -1; other = -1; powerup = -1; emitter = -1;
                    LoadFile("Load Powerup", "ModelProcessor", ref powerup);
                    if (powerup != -1)
                    { renderer.ActiveScene.Access3D(powerup).Position = new Vector3(0, 0, 30); }
                }
                else if (Keyboard.GetState().IsKeyDown(Keys.O))
                {
                    renderer.ActiveScene.Clear(1);
                    tank = -1; turret = -1; other = -1; powerup = -1; emitter = -1;
                    LoadFile("Load Any Model", "ModelProcessor", ref other);
                    renderer.ActiveScene.Access3D(other).Position = new Vector3(32, 32, 0);
                }
                else if (Keyboard.GetState().IsKeyDown(Keys.E))
                {
                    renderer.ActiveScene.Clear(1);
                    Renderer.RendererAssetPool.ParticleEmitterSettings.Remove("Emitter");
                    Renderer.RendererAssetPool.ParticleSystemSettings.Remove("Emitter");
                    tank = -1; turret = -1; other = -1; powerup = -1; emitter = -1;
                    LoadParticleSystem();
                    LoadParticleEmitter(ref emitter);
                }
            }

            else if (Keyboard.GetState().IsKeyDown(Keys.E))
            {
                if (!emitterAdded)
                {
                    renderer.ActiveScene.Clear(1);
                    emitter = -1;
                    if (Renderer.RendererAssetPool.ParticleEmitterSettings.ContainsKey("Emitter"))
                    {
                        Renderer.SceneTools.Entities.ParticleEmitter em = new Renderer.SceneTools.Entities.ParticleEmitter("Emitter");
                        em.Position = Vector3.Zero;
                        emitter = renderer.ActiveScene.Add(em, 1);
                        emitterAdded = true;
                    }
                }
            }
            else if (Keyboard.GetState().IsKeyUp(Keys.E))
            {
                emitterAdded = false;
            }

            else if (Keyboard.GetState().IsKeyDown(Keys.Right))
            {
                renderer.ActiveScene.Access3D(tank).RotateZ(0.1f);
            }
            else if (Keyboard.GetState().IsKeyDown(Keys.Left))
            {
                renderer.ActiveScene.Access3D(tank).RotateZ(-0.1f);
            }

        }

        void LoadFile(string dialogueTitle, string modelProcessor, ref int var)
        {
            System.Windows.Forms.OpenFileDialog fileDialog = new System.Windows.Forms.OpenFileDialog();

            // Default to the directory which contains our content files.
            string assemblyLocation = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string relativePath = System.IO.Path.Combine(assemblyLocation, "../../../../../../");
            string contentPath = System.IO.Path.GetFullPath(relativePath);

            fileDialog.InitialDirectory = contentPath;

            fileDialog.Title = dialogueTitle;

            fileDialog.Filter = "Model Files (*.fbx;*.x)|*.fbx;*.x|" +
                                "FBX Files (*.fbx)|*.fbx|" +
                                "X Files (*.x)|*.x|" +
                                "All Files (*.*)|*.*";

            if (fileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                LoadModel(fileDialog.FileName, modelProcessor, ref var);
            }
        }

        void LoadModel(string fileName, string modelProcessor, ref int variable)
        {
            System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.WaitCursor;
            // Unload any existing model.

            // Tell the ContentBuilder what to build.
            contentBuilder.Clear();
            string name = System.IO.Path.GetFileNameWithoutExtension(fileName);
            contentBuilder.Add(fileName, name, null, modelProcessor);

            // Build this new model data.
            string buildError = contentBuilder.Build();

            if (string.IsNullOrEmpty(buildError))
            {
                // If the build succeeded, use the ContentManager to
                // load the temporary .xnb file that we just created.
                string path = System.IO.Path.Combine(contentBuilder.OutputExtention, name);
                Model model = Content.Load<Model>(path);
                SkinningData data = model.Tag as SkinningData;
                if (data == null)
                {
                    animationPlayer = null;
                    animations = null;
                }
                else
                {
                    animationPlayer = new AnimationPlayer(data);
                    animations = data.AnimationClips;
                }
                variable = renderer.ActiveScene.Add(model, Vector3.Zero, 1);
                renderer.ActiveScene.Access3D(variable).MeshColor = Color.Red;
                System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.Arrow;
            }
            else
            {
                // If the build failed, display an error message.
                System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.Arrow;
                System.Windows.Forms.MessageBox.Show(buildError, "Error");
            }

        }
        void LoadParticleSystem()
        {
            System.Windows.Forms.OpenFileDialog fileDialog = new System.Windows.Forms.OpenFileDialog();

            // Default to the directory which contains our content files.
            string assemblyLocation = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string relativePath = System.IO.Path.Combine(assemblyLocation, "../../../../../../Client/Driver/Content");
            string contentPath = System.IO.Path.GetFullPath(relativePath);

            fileDialog.InitialDirectory = contentPath;

            fileDialog.Title = "Load Particle System";

            fileDialog.Filter = "VTank Particle System (*.vtpss)|*.vtpss";

            if (fileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Renderer.SceneTools.Entities.Particles.ParticleSystemSettings newPSS = new Renderer.SceneTools.Entities.Particles.ParticleSystemSettings();
                StreamReader read = new StreamReader(fileDialog.FileName);
                newPSS.Load(read);
                read.Close();
                Renderer.RendererAssetPool.ParticleSystemSettings.Remove(Path.GetFileNameWithoutExtension(fileDialog.FileName));
                Renderer.RendererAssetPool.ParticleSystemSettings.Add(Path.GetFileNameWithoutExtension(fileDialog.FileName), newPSS);
            }
        }
        void LoadParticleEmitter(ref int var)
        {
            System.Windows.Forms.OpenFileDialog fileDialog = new System.Windows.Forms.OpenFileDialog();

            // Default to the directory which contains our content files.
            string assemblyLocation = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string relativePath = System.IO.Path.Combine(assemblyLocation, "../../../../../../Client/Driver/Content");
            string contentPath = System.IO.Path.GetFullPath(relativePath);

            fileDialog.InitialDirectory = contentPath;

            fileDialog.Title = "Load Particle Emitter";

            fileDialog.Filter = "VTank Particle System (*.vtpes)|*.vtpes";

            if (fileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Renderer.SceneTools.Entities.Particles.ParticleEmitterSettings newPSS = new Renderer.SceneTools.Entities.Particles.ParticleEmitterSettings();
                StreamReader read = new StreamReader(fileDialog.FileName);
                newPSS.Load(read);
                read.Close();
                Renderer.RendererAssetPool.ParticleEmitterSettings.Clear();
                Renderer.RendererAssetPool.ParticleEmitterSettings.Add("Emitter", newPSS);
                Renderer.SceneTools.Entities.ParticleEmitter emitter = new Renderer.SceneTools.Entities.ParticleEmitter("Emitter");
                emitter.Position = Vector3.Zero;
                var = renderer.ActiveScene.Add(emitter, 1);
            }
        }



        private void FillTiles()
        {
            Random random = new Random();
            //Loads the tiles into the background array
            int height = 7, width = 7;
            int minHeight = -7, minWidth = -7;
            //string path = System.IO.Path.Combine(System.Reflection.Assembly.GetExecutingAssembly().Location, "../../../../Content/");
            Texture2D texture = Content.Load<Texture2D>("grid");

            int tileSize = 64;


            Vector3 posUL = new Vector3(minWidth * tileSize, height * tileSize, 0);
            Vector3 posLL = new Vector3(minWidth * tileSize, minHeight * tileSize, 0);
            Vector3 posLR = new Vector3(width * tileSize, minHeight * tileSize, 0);
            Vector3 posUR = new Vector3(width * tileSize, height * tileSize, 0);


            VertexPositionNormalTexture UL = new VertexPositionNormalTexture(posUL, Vector3.UnitZ, Vector2.Zero);
            VertexPositionNormalTexture LL = new VertexPositionNormalTexture(posLL, Vector3.UnitZ, Vector2.UnitY * (height-minHeight));
            VertexPositionNormalTexture UR = new VertexPositionNormalTexture(posUR, Vector3.UnitZ, Vector2.UnitX * (width-minWidth));
            VertexPositionNormalTexture LR = new VertexPositionNormalTexture(posLR, Vector3.UnitZ, new Vector2((height-minHeight), (width-minWidth)));

            List<VertexPositionNormalTexture> list = new List<VertexPositionNormalTexture>();
            list.Add(UL); list.Add(LR); list.Add(LL);
            list.Add(UR); list.Add(LR); list.Add(UL);
            VertexGroup group = new VertexGroup(texture, list);
            group.Technique = Renderer.RendererAssetPool.UniversalEffect.Techniques.TexturedWrap;
            group.Ready();
            renderer.ActiveScene.Add(group, 0);

        }
    }
}
