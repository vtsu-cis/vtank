using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Renderer;
using Renderer.Utils;
using Renderer.SceneTools;
using Renderer.SceneTools.Entities;
using Renderer.SceneTools.Effects;

namespace Renderer
{
    /// <summary>
    /// EntityRenderer is the class that tracks the current active scenes. 
    /// </summary>
    public class EntityRenderer
    {
        #region Members
        private RenderTarget2D image;
        private List<Scene> sceneList;
        private Scene activeScene;
        private LowestAvailableID sceneIdGenerator;
        private GraphicOptions options;
        #endregion

        /// <summary>
        /// Constructor. 
        /// </summary>
        /// <param name="graphics">The GraphicsDeviceManager for the EntityRenderer to use. </param>
        /// <param name="content">The ContentManager for the EntityRenderer to use.</param>
        public EntityRenderer(GraphicsDeviceManager graphics, ContentManager content)
        {
            options = new GraphicOptions(graphics, content);
            sceneList = new List<Scene>();
            sceneIdGenerator = new LowestAvailableID();
            RendererAssetPool.UniversalEffect = UniversalEffect.LoadEffect(content);
            activeScene = new Scene();
            GraphicOptions.Content = content;
            sceneList.Add(activeScene);
        }
        #region Properties
        /// <summary>
        /// Public get/set for the active scene.
        /// </summary>
        public Scene ActiveScene
        {
            get { return activeScene; }
            set { activeScene = value; }
        }

        #endregion

        #region Animation Functions
        #endregion

        #region Scene functions
        /// <summary>
        /// Creates a new Scene
        /// </summary>
        /// <returns>The new scenes Id</returns>
        public int NewScene()
        {
            Scene newScene = new Scene();
            int x = sceneIdGenerator.GetID();
            sceneList.Insert(x, newScene);
            return x;
        }

        /// <summary>
        /// Calls Update on the active scene
        /// </summary>
        public void Update()
        {   
            WorldMouse.Update();
            activeScene.Update();
        }

        /// <summary>
        /// Calls Draw on the Active Scene
        /// </summary>
        public void Draw()
        {
            activeScene.Draw();
        }

        /// <summary>
        /// Draws the current scene as a texture.
        /// </summary>
        /// <returns>The textured scene</returns>
        public Texture2D DrawAsImage()
        {
            if (image == null)
            {
                // Only needs to be created once.
                image = new RenderTarget2D(GraphicOptions.batch.GraphicsDevice,
                    GraphicOptions.graphics.PreferredBackBufferWidth,
                    GraphicOptions.graphics.PreferredBackBufferHeight,
                    1, SurfaceFormat.Color);
            }

            GraphicOptions.graphics.GraphicsDevice.SetRenderTarget(0, image);
            Draw();
            GraphicOptions.graphics.GraphicsDevice.SetRenderTarget(0, null);

            return image.GetTexture();
        }

        /// <summary>
        /// Deletes a scene using its scene id
        /// </summary>
        /// <param name="sceneId">The id of the scene to delete</param>
        public void DelScene(int sceneId)
        {
            sceneIdGenerator.ReleaseID(sceneId);
            sceneList.RemoveAt(sceneId);
        }

        /// <summary>
        /// Uses a sceneID to make an existing Scene the active scene
        /// </summary>
        /// <param name="sceneID">The ID of the Scene to make active</param>
        public void ChooseActiveScene(int sceneID)
        {
            try
            {
                activeScene = sceneList[sceneID];
            }
            finally { }
        }

        /// <summary>
        /// Chooses active scene from existing scenes
        /// </summary>
        /// <param name="scene"></param>
        public void ChooseActiveScene(Scene scene)
        {
            try
            {
                if (sceneList.Contains(scene))
                {
                    activeScene = sceneList[sceneList.IndexOf(scene)];
                }
            }
            finally { }
        }
        #endregion

    }
}
