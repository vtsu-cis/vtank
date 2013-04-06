using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Renderer;
using Renderer.SceneTools.Entities;
using Client.src.service;

namespace Client.src.util
{
    class ModelToTexture
    {
        EntityRenderer renderer;
        int tnk, tur;
        RenderTarget2D renderTarget;
        Model model1;
        Model model2;
        Color bak;

        public ModelToTexture(Model _tank, Model _turret)
        {
            model1 = _tank;
            model2 = _turret;
            renderer = new EntityRenderer(ServiceManager.Game.GraphicsDeviceManager, ServiceManager.Game.Content);
            bak = Renderer.GraphicOptions.BackgroundColor;

            //Set up the camera for the shot
            Camera cam = renderer.ActiveScene.CurrentCamera;
            renderer.ActiveScene.CreateCamera(new Vector3(100, 100, 20), Vector3.Zero, cam.Projection, "Side View");
            renderer.ActiveScene.AccessCamera("Side View").CameraUp = Vector3.UnitZ;
            renderer.ActiveScene.SwitchCamera("Side View");


            LoadContent();

            renderTarget = new RenderTarget2D(ServiceManager.Game.GraphicsDevice,
                ServiceManager.Game.GraphicsDeviceManager.PreferredBackBufferWidth,
                ServiceManager.Game.GraphicsDeviceManager.PreferredBackBufferHeight,
                1,
                SurfaceFormat.Color,
                ServiceManager.Game.GraphicsDevice.PresentationParameters.MultiSampleType,
                ServiceManager.Game.GraphicsDevice.PresentationParameters.MultiSampleQuality);

        }

        private void LoadContent()
        {
            tnk = renderer.ActiveScene.Add(model1, Vector3.Zero, 0);
            tur = renderer.ActiveScene.Add(model2, Vector3.Zero, 0);
            

            renderer.ActiveScene.Access3D(tur).Attach(renderer.ActiveScene.Access3D(tnk));

        }

        public void ChangeModels(Model _mod1, Model _mod2)
        {
            renderer.ActiveScene.Delete(tur);
            renderer.ActiveScene.Delete(tnk);


            model1 = _mod1;
            model2 = _mod2;

            LoadContent();

        }

        public void Update()
        {
            renderer.Update();
        }

        public void Draw()
        {
            renderer.Draw();
        }


        public Texture2D GetImage()
        {
            //Make a reference to the screen
            RenderTarget oldTarget = ServiceManager.Game.GraphicsDevice.GetRenderTarget(0);

            //Set the new render target to our new palette
            ServiceManager.Game.GraphicsDevice.SetRenderTarget(0, renderTarget);

            ServiceManager.Game.GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1.0f, 0);

            Renderer.GraphicOptions.BackgroundColor = Color.Black;
            Draw();
            Renderer.GraphicOptions.BackgroundColor = bak;

            //Set the render target back to the screen
            ServiceManager.Game.GraphicsDevice.SetRenderTarget(0, oldTarget as RenderTarget2D);

            return renderTarget.GetTexture();
        }
    }
}
