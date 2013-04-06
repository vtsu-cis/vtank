using System;
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
using Renderer;
using Renderer.SceneTools;
using Renderer.SceneTools.Entities;
using Renderer.SceneTools.Animation;
using Renderer.Utils;

namespace ModelViewer
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        ContentBuilder cBuilder;
        EntityRenderer renderer;
        AnimationManager animations;
        int mod, tur, mod1, mod3, tur1, tex, exp;
        float scrollWheelValue;
        Vector2 mouseReferencePosition;
        Texture2D texture;
        Boolean exploding;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            animations = new AnimationManager();
            scrollWheelValue = 0;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            cBuilder = new ContentBuilder();
            Content.RootDirectory = cBuilder.OutputDirectory;
            renderer = new EntityRenderer(graphics, Content);
            this.IsMouseVisible = true;
            base.Initialize();

        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {

           
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            InputHandler();
            GraphicOptions.FrameLength = (float)gameTime.ElapsedGameTime.TotalSeconds;
            renderer.Update();
            //base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            renderer.Draw();
            base.Draw(gameTime);
        }

        private void InputHandler()
        {
            renderer.ActiveScene.CurrentCamera.TranslateRelativly(Vector3.Forward * (Mouse.GetState().ScrollWheelValue - scrollWheelValue) * .1f);
            scrollWheelValue = Mouse.GetState().ScrollWheelValue;
            renderer.ActiveScene.CurrentCamera.LockRotation = true;

            if (Mouse.GetState().RightButton == Microsoft.Xna.Framework.Input.ButtonState.Released)
            {
                WorldMouse.Update();
                mouseReferencePosition.X = WorldMouse.Position.X;
                mouseReferencePosition.Y = WorldMouse.Position.Y;
            }

            if (Mouse.GetState().RightButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed)
            {
                Vector3 translation;
                WorldMouse.Update();
                translation.X = mouseReferencePosition.X - WorldMouse.Position.X;
                translation.Y = mouseReferencePosition.Y - WorldMouse.Position.Y;
                mouseReferencePosition.X = WorldMouse.Position.X;
                mouseReferencePosition.Y = WorldMouse.Position.Y;
                translation.Z = 0;
                renderer.ActiveScene.CurrentCamera.Translate(translation/100);
            }

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            if (Keyboard.GetState().IsKeyDown(Keys.E))
            {
                if (!exploding)
                {
                    exploding = true;
                }
            }
            if (Keyboard.GetState().IsKeyDown(Keys.F))
            {
                    exploding = false;

            }
            if (Keyboard.GetState().IsKeyDown(Keys.Right))
            {
                renderer.ActiveScene.Access(mod1).Translate(Vector3.Right);
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Left))
            {
                renderer.ActiveScene.Access(mod1).Translate(Vector3.Left);
            }

            if (Keyboard.GetState().IsKeyDown(Keys.Up))
            {
                renderer.ActiveScene.Access(mod1).Translate(Vector3.Up);
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Down))
            {
                renderer.ActiveScene.Access(mod1).Translate(Vector3.Down);
            }
            if (Keyboard.GetState().IsKeyDown(Keys.F))
            {
                renderer.ActiveScene.Access(mod).Translate(Vector3.Forward);
            }
            if (Keyboard.GetState().IsKeyDown(Keys.B))
            {
                renderer.ActiveScene.Access(mod).Translate(Vector3.Backward);
            }

            if (Keyboard.GetState().IsKeyDown(Keys.X))
            {
                renderer.ActiveScene.Access(mod).RotateZ(.1f); //No Longer used

            }
            if (Keyboard.GetState().IsKeyDown(Keys.Y))
            {
                renderer.ActiveScene.Access(mod).RotateY(.1f);// No Longer used
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Z))
            {
                renderer.ActiveScene.Access(mod).Aim(renderer.ActiveScene.Access(mod).Right, .1f);
            }
            if (Keyboard.GetState().IsKeyDown(Keys.D))
            {
                renderer.ActiveScene.Access(mod).TranslateRelativly(Vector3.Right);
            }
            if (Keyboard.GetState().IsKeyDown(Keys.S))
            {
                renderer.ActiveScene.Access(mod).TranslateRelativly(Vector3.Down);
            }
            if (Keyboard.GetState().IsKeyDown(Keys.W))
            {
                renderer.ActiveScene.Access(mod).TranslateRelativly(Vector3.Up);
            }
            if (Keyboard.GetState().IsKeyDown(Keys.A))
            {
                renderer.ActiveScene.Access(mod).TranslateRelativly(Vector3.Left);
            }

           //if (Keyboard.GetState().IsKeyDown(Keys.A))
           // {
           //    renderer.ActiveScene.Access3D(tur).Attach(renderer.ActiveScene.Access3D(mod), "Mount");
           //     renderer.ActiveScene.Access3D(tur1).Attach(renderer.ActiveScene.Access3D(mod1), "Mount");
           // }
            if (Keyboard.GetState().IsKeyDown(Keys.T))
            {
                renderer.ActiveScene.Access3D(tur).TrackToCursor();
                renderer.ActiveScene.Access3D(tur1).TrackToCursor(); 
            }
            if (Keyboard.GetState().IsKeyDown(Keys.P))
            {
                renderer.ActiveScene.Access3D(tur).Animate(animations.GetAnimation("Cannon0_Fire"));
            }
            if (Keyboard.GetState().IsKeyDown(Keys.O))
            {
                renderer.ActiveScene.Access3D(tur).Animate(animations.GetAnimation("Cannon0_Cooldown"));
            }


            if (Keyboard.GetState().IsKeyDown(Keys.L))
            {

                System.Windows.Forms.OpenFileDialog fileDialog = new System.Windows.Forms.OpenFileDialog();

                // Default to the directory which contains our content files.
                string assemblyLocation = System.Reflection.Assembly.GetExecutingAssembly().Location;
                string relativePath = System.IO.Path.Combine(assemblyLocation, "../../../../Content");
                string contentPath = System.IO.Path.GetFullPath(relativePath);

                fileDialog.InitialDirectory = contentPath;

                fileDialog.Title = "Load Model";

                fileDialog.Filter = "Model Files (*.fbx;*.x)|*.fbx;*.x|" +
                                    "FBX Files (*.fbx)|*.fbx|" +
                                    "X Files (*.x)|*.x|" +
                                    "All Files (*.*)|*.*";

                if (fileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    LoadModel(fileDialog.FileName);
                }
            }

        }
        /// <summary>
        /// Loads a new 3D model file into the ModelViewerControl.
        /// </summary>
        void LoadModel(string fileName)
        {
            Content.Unload();
            // Tell the ContentBuilder what to build.
            cBuilder.Clear();
            cBuilder.Add(fileName, "Model", null, "ModelProcessor");

            // Build this new model data.
            string buildError = cBuilder.Build();

            if (string.IsNullOrEmpty(buildError))
            {
                // If the build succeeded, use the ContentManager to
                // load the temporary .xnb file that we just created.
                Model model = Content.Load<Model>("Model");
            }
            else
            {
                // If the build failed, display an error message.
                System.Windows.Forms.MessageBox.Show(buildError, "Error");
            }
        }
    }
}
