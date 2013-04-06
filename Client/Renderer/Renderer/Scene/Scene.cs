using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Renderer.SceneTools.Effects;
using Renderer.SceneTools.Entities;
using Renderer.Utils;
using Renderer.SceneTools.ShadowMapping;

namespace Renderer.SceneTools
{
    public struct Light
    {
        public Vector3 Position;
        public Vector3 TargetPosition;
        public Vector3 LightDirection;
        public Matrix View;
        public Matrix Projection;
    }
    /// <summary>
    /// This class keeps track of a set of entities and layers specific to this scene.
    /// </summary>
    public class Scene
    {
        #region Members
        private Dictionary<int, Entity> entityList;
        private List<SceneLayer> layers;
        private List<int> deleteQueue;
        private Dictionary<string, Camera> cameraDictionary;
        private List<SceneLayer> drawableEntities;
        private List<SceneLayer> transparentEntities;
        private Vector3 defaultPosition;
        private LowestAvailableID entityID;
        private BoundingFrustum currentFrustum;
        private BoundingFrustum transparent1Frustum;
        private Boolean updateToggle;
        private Entity mainEntity;
        Texture2D currentShadowMap;

        //for shadows        // Rendertarget, it's buffer, and the texture
        // to hold the result
        RenderTarget2D shadowRenderTarget;
        RenderTarget2D oldRenderTarget;
        DepthStencilBuffer shadowDepthBuffer;
        DepthStencilBuffer oldDepthStencil;
        Texture2D shadowMap;

        Light worldLight;
        Color lightColor;


        //private List<Light> lightList;
            
        #endregion

        #region Constructor
        /// <summary>
        /// Creates and Initializes a new RendererScene with a camera named "Default" as the active camera.
        /// </summary>
        public Scene()
        {
            entityID = new LowestAvailableID();
            defaultPosition = Vector3.Zero;
            updateToggle = false;

            layers = new List<SceneLayer>();
            SceneLayer layer = new SceneLayer();
            layers.Add(layer);

            entityList = new Dictionary<int, Entity>();
            drawableEntities = new List<SceneLayer>();
            transparentEntities = new List<SceneLayer>();
            deleteQueue = new List<int>();
            TransparentWalls = true;
            
            cameraDictionary = new Dictionary<string, Camera>();

            CurrentCamera = new Camera();
 
            
            cameraDictionary.Add("Default", CurrentCamera);
            currentFrustum = new BoundingFrustum(CurrentCamera.Projection);

            shadowRenderTarget = GfxComponent.CreateRenderTarget(GraphicOptions.graphics.GraphicsDevice,
                1, SurfaceFormat.Single);
            shadowDepthBuffer =
                GfxComponent.CreateDepthStencil(shadowRenderTarget,
                DepthFormat.Depth24Stencil8Single);

            worldLight.Position = new Vector3(7000, 4000, 10000);
            worldLight.TargetPosition = Vector3.Zero;
            worldLight.View = Matrix.Identity;
            worldLight.Projection = Matrix.Identity;

            PercentOfDayComplete = 0.5f;   
        }
        #endregion

        #region Properties

        /// <summary>
        /// Certain graphics features such as shadows will have highest definition around this entity.
        /// </summary>
        public Entity MainEntity
        {
            get { return mainEntity; }
            set { mainEntity = value; }
        }

        public Color AmbientColor
        {
            get;
            private set;
        }

        /// <summary>
        /// If true, walls between the camera and player will render transparently. 
        /// </summary>
        public  bool TransparentWalls
        {
            get {return GraphicOptions.TransparentWalls;}
            set { GraphicOptions.TransparentWalls = value; }
        }
        /// <summary>
        /// Gets and sets the Renderer global Current Camera
        /// </summary>
        public Camera CurrentCamera
        {
            get { return GraphicOptions.CurrentCamera; }
            set { GraphicOptions.CurrentCamera = value; }
        }

        /// <summary>
        /// Sets the light values based on the percentage of day complete. 0 is sunrise 1 is sunset
        /// </summary>
        public double PercentOfDayComplete
        {
            set;
            get;
        }

        #endregion

        #region Update and Draw

        /// <summary>
        /// Updates the position, scale, rotation, animation and effects of each entity
        /// </summary>
        public void Update()
        {
            updateToggle = !updateToggle;

            foreach (Camera cam in cameraDictionary.Values)
            {
                cam.Update(updateToggle);
            }

            foreach (SceneLayer layer in layers)
            {
                foreach (KeyValuePair<int, Object3> ent in layer.entityDictionary3D)
                {
                    ent.Value.Update(updateToggle);
                    if (ent.Value.AutoDelete)
                    {
                        deleteQueue.Add(ent.Key);
                    }
                }
                foreach (KeyValuePair<int, VertexGroup> ent in layer.entityDictionaryVertex)
                {
                    ent.Value.Update(updateToggle); 
                    if (ent.Value.AutoDelete)
                    {
                        deleteQueue.Add(ent.Key);
                    }
                } 
                foreach (KeyValuePair<int, ParticleEmitter> ent in layer.entityDictionaryEmitter)
                {
                    ent.Value.Update(updateToggle);

                    if (ent.Value.Exhausted)
                    {
                        deleteQueue.Add(ent.Key);
                    }
                }

                foreach (KeyValuePair<int, VertexColorGroup> ent in layer.entityDictionaryVertexColor)
                {
                    ent.Value.Update(updateToggle);
                    if (ent.Value.AutoDelete)
                    {
                        deleteQueue.Add(ent.Key);
                    }
                }

                foreach (KeyValuePair<int, VertexPositionColorTextureGroup> ent in layer.entityDictionaryVPCT)
                {
                    ent.Value.Update(updateToggle);
                    if (ent.Value.AutoDelete)
                    {
                        deleteQueue.Add(ent.Key);
                    }
                }
            }

            //Delete all queued things to delete
            foreach (int id in deleteQueue)
            {
                Delete(id);
            }
            deleteQueue.Clear();

            UpdateWorldLight();
     
        }

        private void UpdateWorldLight()
        {
            //Set new Target Position
            if (mainEntity != null)
            {
                worldLight.TargetPosition = mainEntity.Position;
            }
            else
            {
                worldLight.TargetPosition = Vector3.Zero;
            }

            //Set light view matrix
            if (GraphicOptions.ShadingSupport)
            {
                double dayPercent = PercentOfDayComplete;
                if (dayPercent < 0.0) dayPercent = 0.0;
                if (dayPercent > 1.0) dayPercent = 1.0;

                //Function that makes the intensisy 1 at .5 and 0 at 0 and 1
                double lightIntensity = -4 * Math.Pow(dayPercent - 0.5, 2.0) + 1.0;
                Vector4 minColor = (new Color(200, 100, 0)).ToVector4(); ;
                Vector4 maxColor = (new Color(230, 230, 200)).ToVector4();
                lightColor = new Color(Vector4.Lerp(minColor, maxColor, (float)lightIntensity));
                Vector4 minAmbient = new Vector4(0.1f, 0.1f, 0.1f, 1f);
                Vector4 maxAmbient = new Vector4(.6f, .6f, .6f, 1f);
                Color ambient = new Color(Vector4.Lerp(minAmbient, maxAmbient, (float)lightIntensity));
                AmbientColor = ambient;

                Vector3 morningLightDirection = new Vector3(1, 1, 0);
                Vector3 noonLightDirection = new Vector3(0, 0, -1);
                Vector3 lightDirectionNew;
                lightDirectionNew = Vector3.Lerp(Math.Sign(dayPercent - 0.5f) * morningLightDirection, noonLightDirection, (float)lightIntensity);
                lightDirectionNew.Normalize();
                lightDirectionNew *= 5000;

                worldLight.Position = worldLight.TargetPosition - lightDirectionNew;
                worldLight.View = Matrix.CreateLookAt(worldLight.Position,
                                                      worldLight.TargetPosition,
                                                      Vector3.Cross(lightDirectionNew, new Vector3(-1f, 1f, 0)));


                //get light projection
                Vector3 Center = worldLight.TargetPosition;
                float Radius = 250;
                Vector3 temp = worldLight.Position - worldLight.TargetPosition;
                float near = temp.Length() - Radius;
                float far = temp.Length() + Radius;

                worldLight.Projection = Matrix.CreateOrthographic(Radius, Radius, near, far);
            }
        }

        /// <summary>
        /// Duplicated draw function to allow its calling without clearing the graphics device automatically.
        /// (You should still be clearing between frames manually if using this)
        /// </summary>
        /// <param name="clearScreen"></param>
        public void Draw(bool clearScreen)
        {
            if ( clearScreen == true)
                GraphicOptions.graphics.GraphicsDevice.Clear(GraphicOptions.BackgroundColor);

            if (RendererAssetPool.DrawShadows)
            {
                ConfigureShadowEffect();
                currentShadowMap = DrawShadowMap();
                RendererAssetPool.UniversalEffect.ShadowParameters.ShadowMapTexture = currentShadowMap;
            }
            ConfigureEffect();
            FindVisible();

            DrawScene();
            DrawTransparent();

            if (RendererAssetPool.DrawShadows)
            {
                Rectangle rect = new Rectangle(0, 300, 300, 300);
                GraphicOptions.batch.Begin();
                GraphicOptions.batch.Draw(currentShadowMap, rect, Color.White);
                GraphicOptions.batch.End();
               // currentShadowMap.Save("shadowmap.png", ImageFileFormat.Png);
            }

        }

        /// <summary>
        /// Calls the draw function in every entity that is in the bounding frustrum
        /// </summary>
        public void Draw()
        {
            //GraphicOptions.graphics.GraphicsDevice.RenderState.FillMode = FillMode.Solid;
            GraphicOptions.graphics.GraphicsDevice.Clear(GraphicOptions.BackgroundColor);

            if (RendererAssetPool.DrawShadows)
            {
                ConfigureShadowEffect();
                currentShadowMap = DrawShadowMap();
                RendererAssetPool.UniversalEffect.ShadowParameters.ShadowMapTexture = currentShadowMap;
            }
            ConfigureEffect();
            FindVisible();

            DrawScene();
            DrawTransparent();

            if (RendererAssetPool.DrawShadows)
            {
                Rectangle rect = new Rectangle(0, 300, 300, 300);
                GraphicOptions.batch.Begin();
                GraphicOptions.batch.Draw(currentShadowMap, rect, Color.White);
                GraphicOptions.batch.End();
               // currentShadowMap.Save("shadowmap.png", ImageFileFormat.Png);
            }
        }

        #region Shadow Helpers
        private Texture2D DrawShadowMap()
        {
            GraphicsDevice GraphicsDevice = GraphicOptions.graphics.GraphicsDevice;
            // Set the depth buffer function that best fits our stencil type
            // and projection (a reverse projection would use GreaterEqual)
            GraphicsDevice.RenderState.DepthBufferFunction =
                CompareFunction.LessEqual;
            oldRenderTarget = GraphicsDevice.GetRenderTarget(0) as RenderTarget2D;
            // Set Render Target for shadow map
            GraphicsDevice.SetRenderTarget(0, shadowRenderTarget);
            // Cache the current depth buffer
            oldDepthStencil = GraphicsDevice.DepthStencilBuffer;
            // Set our custom depth buffer
            GraphicsDevice.DepthStencilBuffer = shadowDepthBuffer;

            // Render the shadow map
            GraphicsDevice.Clear(ClearOptions.Target, Color.Black, 1.0f, 0);
            GraphicsDevice.Clear(ClearOptions.DepthBuffer, Color.Black, 1.0f, 0);

            //Set renderstates
            GraphicsDevice.RenderState.CullMode = CullMode.None;
            GraphicsDevice.RenderState.DepthBufferEnable = true;
            GraphicsDevice.RenderState.DepthBufferWriteEnable = true;
            GraphicsDevice.RenderState.AlphaBlendEnable = false;

            EffectTechnique technique = RendererAssetPool.UniversalEffect.Techniques.ShadowMap;
            foreach (SceneLayer layer in layers)
            {
                foreach (KeyValuePair<int, Object3> ent in layer.entityDictionary3D)
                {
                    if (ent.Value.CastsShadow)
                         ent.Value.Draw(technique);
                }

                foreach (KeyValuePair<int, VertexGroup> ent in layer.entityDictionaryVertex)
                {
                    if (ent.Value.CastsShadow)
                         ent.Value.Draw(technique);
                }
            }

            // Set render target back to the back buffer
            GraphicsDevice.SetRenderTarget(0, oldRenderTarget);
            // Reset the depth buffer
            GraphicsDevice.DepthStencilBuffer = oldDepthStencil;

            // Return the shadow map as a texture
            return shadowRenderTarget.GetTexture();
        }

        /// <summary>
        /// Draws the specified entities with the specified technique
        /// </summary>
        /// <param name="entitiesToDraw">The list of entities to draw</param>
        /// <param name="technique">The technique to draw them with, if null they will draw with their individual defualt settings.</param>
        private void DrawScene()
        {
            RenderState renderState = GraphicOptions.graphics.GraphicsDevice.RenderState;
            EffectTechnique technique = RendererAssetPool.UniversalEffect.Techniques.UseDefault;
            foreach (SceneLayer layer in drawableEntities)
            {
                if (layer.DrawLayer)
                {
                    renderState.CullMode = CullMode.CullCounterClockwiseFace;
                    renderState.AlphaBlendEnable = true;
                    renderState.AlphaTestEnable = true;
                    renderState.AlphaFunction = CompareFunction.Greater;
                    renderState.ReferenceAlpha = 235;
                    renderState.DepthBufferEnable = true;
                    renderState.DepthBufferWriteEnable = true;
                    foreach (KeyValuePair<int, Object3> ent in layer.entityDictionary3D)
                    {
                        ent.Value.Draw(technique);
                    }

                    foreach (KeyValuePair<int, VertexGroup> ent in layer.entityDictionaryVertex)
                    {
                        ent.Value.Draw(technique);
                    }

                    foreach (KeyValuePair<int, ParticleEmitter> ent in layer.entityDictionaryEmitter)
                    {
                        ent.Value.Draw(technique);
                    }

                    renderState.AlphaBlendEnable = false;
                    renderState.AlphaTestEnable = false;
                    renderState.DepthBufferEnable = true;
                    renderState.AlphaTestEnable = false;
                    foreach (KeyValuePair<int, VertexColorGroup> ent in layer.entityDictionaryVertexColor)
                    {
                        ent.Value.Draw(technique);
                    }

                    renderState.AlphaBlendEnable = true;
                    renderState.AlphaTestEnable = false;
                    renderState.SourceBlend = Blend.SourceAlpha;
                    renderState.DestinationBlend = Blend.InverseSourceAlpha;
                    renderState.DepthBufferEnable = true;
                    renderState.DepthBufferWriteEnable = true;
                    foreach (KeyValuePair<int, VertexPositionColorTextureGroup> ent in layer.entityDictionaryVPCT)
                    {
                        ent.Value.Draw(technique);
                    }
                }
            }
        }

        #endregion

        /// <summary>
        /// Helper Draw function. Determines what objects are fully visible and what objects are transparent.
        /// </summary>
        private void FindVisible()
        {
            currentFrustum = new BoundingFrustum(CurrentCamera.View * CurrentCamera.Projection);
            Matrix transView = Matrix.CreateLookAt(CurrentCamera.Position, CurrentCamera.TargetPosition - Vector3.UnitZ * 30, Vector3.UnitZ);
            Matrix trans1Proj = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4  / 15,
                (float)GraphicOptions.graphics.GraphicsDevice.Viewport.Width /
                        (float)GraphicOptions.graphics.GraphicsDevice.Viewport.Height,
                        10f,
                        475f);
            transparent1Frustum = new BoundingFrustum(transView * trans1Proj);

            //this section updates each entity and then determines if they are within the bounding frustum
            drawableEntities.Clear();
            transparentEntities.Clear();
            int i = 0;
            foreach (SceneLayer layer in layers)
            {
                drawableEntities.Add(new SceneLayer());
                transparentEntities.Add(new SceneLayer());
                drawableEntities[i].DrawLayer = layer.DrawLayer;
                transparentEntities[i].DrawLayer = layer.DrawLayer;

                foreach (KeyValuePair<int, Object3> ent in layer.entityDictionary3D)
                {
                    if (currentFrustum.Contains(ent.Value.BoundingSphere) != ContainmentType.Disjoint)
                    {
                        if (transparent1Frustum.Contains(ent.Value.BoundingSphere) != ContainmentType.Disjoint 
                            && ent.Value.TransparencyEnabled && GraphicOptions.TransparentWalls)
                        {
                            transparentEntities[i].Add(ent);
                        }
                        if (transparent1Frustum.Contains(ent.Value.BoundingSphere) != ContainmentType.Contains
                            || !ent.Value.TransparencyEnabled || !GraphicOptions.TransparentWalls)
                        {
                            drawableEntities[i].Add(ent);
                        }
                        
                    }
                }
                foreach (KeyValuePair<int, VertexGroup> ent in layer.entityDictionaryVertex)
                {
                  drawableEntities[i].Add(ent);
                }
                foreach (KeyValuePair<int, ParticleEmitter> ent in layer.entityDictionaryEmitter)
                {
                    if (currentFrustum.Contains(ent.Value.BoundingSphere) != ContainmentType.Disjoint)
                    {
                        drawableEntities[i].Add(ent);
                    }
                    else
                    {
                        ent.Value.OutOfFrustumDraw();
                    }
                }

                foreach (KeyValuePair<int, VertexColorGroup> ent in layer.entityDictionaryVertexColor)
                {
                    // if (currentFrustum.Contains(ent.Value.BoundingSphere) != ContainmentType.Disjoint)
                    // {
                    drawableEntities[i].Add(ent);
                    // }
                }
                foreach (KeyValuePair<int, VertexPositionColorTextureGroup> ent in layer.entityDictionaryVPCT)
                {
                    // if (currentFrustum.Contains(ent.Value.BoundingSphere) != ContainmentType.Disjoint)
                    // {
                    drawableEntities[i].Add(ent);
                    // }
                }
                i++;
            }

        }
        public void ConfigureShadowEffect()
        {
            UniversalEffect effect = RendererAssetPool.UniversalEffect;

            effect.CurrentTechnique = effect.Techniques.ShadowMap;
            effect.WorldParameters.WorldMatrix = Matrix.Identity;
            effect.LightParameters.Position = worldLight.Position;
            effect.LightParameters.View = worldLight.View;
            effect.LightParameters.Projection = worldLight.Projection;
        }
        public void ConfigureEffect()
        {
            UniversalEffect effect = RendererAssetPool.UniversalEffect;


            effect.LightParameters.EnableLighting = GraphicOptions.ShadingSupport;
            effect.ShadowParameters.EnableShadows = RendererAssetPool.DrawShadows;

            //Light Parameters
            effect.LightParameters.Position = worldLight.Position;
            effect.LightParameters.View = worldLight.View;
            effect.LightParameters.Projection = worldLight.Projection;

            effect.LightParameters.Color = lightColor;

            //Camera Parameters
            effect.CameraParameters.Position = CurrentCamera.Position;
            effect.CameraParameters.Projection = CurrentCamera.Projection;
            effect.CameraParameters.View = CurrentCamera.View;

            //World Parameters
            effect.WorldParameters.WorldMatrix = Matrix.Identity;
            if (!GraphicOptions.ShadingSupport)
            {
                AmbientColor = Color.White;
            }
            effect.LightParameters.AmbientColor = AmbientColor;
            if(RendererAssetPool.ParticleEffect != null)
                RendererAssetPool.ParticleEffect.Parameters["xAmbient"].SetValue(AmbientColor.ToVector4());
        }

        /// <summary>
        /// Draws transparent objects. 
        /// </summary>
        private void DrawTransparent()
        {
            RenderState renderState = GraphicOptions.graphics.GraphicsDevice.RenderState;

            renderState.AlphaBlendEnable = true;
            renderState.SourceBlend = Blend.SourceAlpha;
            renderState.DestinationBlend = Blend.InverseSourceAlpha;
            renderState.AlphaTestEnable = true;
            renderState.AlphaFunction = CompareFunction.LessEqual;
            renderState.ReferenceAlpha = 235;
            renderState.DepthBufferEnable = true;
            renderState.DepthBufferWriteEnable = false;
          
            foreach (SceneLayer layer in transparentEntities)
            {
                foreach (KeyValuePair<int, Object3> ent in layer.entityDictionary3D)
                {
                    ent.Value.Draw(RendererAssetPool.UniversalEffect.Techniques.UseDefault);
                }
            }
            foreach (SceneLayer layer in drawableEntities)
            {
                foreach (KeyValuePair<int, VertexGroup> ent in layer.entityDictionaryVertex)
                {
                    ent.Value.Draw(RendererAssetPool.UniversalEffect.Techniques.UseDefault);
                }
            }
        }

        #endregion

        #region Camera

        /// <summary>
        /// Makes a new Camera object and adds it to the scene.
        /// </summary>
        /// <param name="position">The position that the camera is inserted at. </param>
        /// <param name="target">The position the camera is pointing at</param>
        /// <param name="projection">The projection matrix for the camera to use.</param>
        /// <param name="name">The identifying name of the camera.</param>
        /// <returns>The newly created Camera object.</returns>
        public Camera CreateCamera(Vector3 position, Vector3 target, 
            Matrix projection, string name)
        {
            if (!cameraDictionary.ContainsKey(name))
            {
                Camera newCamera = new Camera(position, target, projection);
                cameraDictionary.Add(name, newCamera);

                return newCamera;
            }
            else
            {
                return cameraDictionary[name];
            }
        }

        /// <summary>
        /// Sets a new camera to currentView
        /// </summary>
        /// <param name="_cameraName">The id of the camera</param>
        public void SwitchCamera(string cameraName)
        {
            try
            {
                CurrentCamera = cameraDictionary[cameraName];
            }
            finally { }

        }

        /// <summary>
        /// Deletes a camera
        /// </summary>
        /// <param name="_cameraName">The id of the camera</param>
        public void DeleteCamera(string cameraName)
        {
            if (cameraDictionary.ContainsKey(cameraName))
            {
                cameraDictionary.Remove(cameraName);
            }
        }

        /// <summary>
        /// Forces all locked cameras to their locked position
        /// </summary>
        public void LockCameras()
        {
            foreach (Camera cam in cameraDictionary.Values)
            {
                cam.ForceToLockedPosition();
            }
        }

        /// <summary>
        /// Public get for the projection matrix that the current Frustum should use.
        /// </summary>
        /// <returns>Projection Matrix for BoundingFrustrum initializaion</returns>
        public Matrix FrustumProjection()
        {
            return CurrentCamera.Projection;
        }

        #endregion

        #region Layers

        /// <summary>
        /// Sets a layer to not draw.
        /// </summary>
        /// <param name="layer">The layer to hide.</param>
        public void HideLayer(int layer)
        {
            layers[layer].DrawLayer = false;
        }

        /// <summary>
        /// Sets a layer to draw.
        /// </summary>
        /// <param name="layer">The layer to show.</param>
        public void ShowLayer(int layer)
        {
            layers[layer].DrawLayer = true;
        }

        #endregion

        #region Add/Delete

        /// <summary>
        /// Starts tracking a new Renderer3DObject
        /// </summary>
        /// <param name="_entity">The Renderer3DObject to add</param>
        /// <param name="_position">The position to assign the object</param>
        /// <param name="_layer">The layer to associate the entity with. Lower layer numbers are drawn first.</param>
        /// <returns>The unique entity id used to keep track of each entity</returns>
        public int Add(Object3 _entity, int _layer)
        {
            RendererAssetPool.UniversalEffect.RemapModel(_entity.Model);
            int x = entityID.GetID();
            entityList.Add(x, _entity);
            AddLayers(_layer);
            layers[_layer].Add(x, _entity);

            return x;
        }

        /// <summary>
        /// Adds a new RendererVertexGroup entity to the scene
        /// </summary>
        /// <param name="_entity">The entity to add.</param>
        /// <param name="_layer">The layer to add it to, lower layers are drawn first.</param>
        /// <returns>The entity's unique ID.</returns>
        public int Add(VertexGroup _entity, int _layer)
        {
            int x = entityID.GetID();
            entityList.Add(x, _entity);
            AddLayers(_layer);
            layers[_layer].Add(x, _entity);

            return x;
        }

        /// <summary>
        /// Adds a new ParticleEmitter to the scene.
        /// </summary>
        /// <param name="_entity">The ParticleEmitter object to add.</param>
        /// <param name="_layer">The layer to add the emitter.</param>
        /// <returns>The unique Renderer ID of the added emitter. </returns>
        public int Add(ParticleEmitter _entity, int _layer)
        {
            int x = entityID.GetID();
            entityList.Add(x, _entity);
            AddLayers(_layer);
            layers[_layer].Add(x, _entity);

            return x;
        }

        /// <summary>
        /// Add a particle emitter at a specified position, on the default layer (3).
        /// </summary>
        /// <param name="particleSettingsName">The name of the particle emitter settings file.</param>
        /// <param name="position">The X, Y, Z position of the emitter.</param>
        /// <returns>The emitter's render ID.</returns>
        public int AddParticleEmitterAtPosition(string particleSettingsName, Vector3 position)
        {
            ParticleEmitter emitter = new ParticleEmitter(particleSettingsName, position);

            int renderID = entityID.GetID();
            entityList.Add(renderID, emitter);
            AddLayers(3);
            layers[3].Add(renderID, emitter);

            return renderID;
        }

        /// <summary>
        /// Add a particle emitter at a specified position, on the default layer (3).
        /// </summary>
        /// <param name="emitter">The ParticleEmitter object to add.</param>
        /// <param name="position">The X, Y, Z position of the emitter.</param>
        /// <returns>The emitter's render ID.</returns>
        public int AddParticleEmitterAtPosition(ParticleEmitter emitter, Vector3 position)
        {
            emitter.Position = position;
            int renderID = entityID.GetID();
            entityList.Add(renderID, emitter);
            AddLayers(3);
            layers[3].Add(renderID, emitter);

            return renderID;
        }

        /// <summary>
        /// Adds a new VertexColorGroup object to the scene.
        /// </summary>
        /// <param name="_entity">The VertexColorGroup object to add.</param>
        /// <param name="_layer">The layer to add the VertexColorGroup object.</param>
        /// <returns>A unique ID for this entity.</returns>
        public int Add(VertexColorGroup _entity, int _layer)
        {
            int x = entityID.GetID();
            entityList.Add(x, _entity);
            AddLayers(_layer);
            layers[_layer].Add(x, _entity);

            return x;
        }

        /// <summary>
        /// Adds a new VertexPositionColorTextureGroup object to the scene.
        /// </summary>
        /// <param name="_entity">The VertexPositionColorTextureGroup object to add.</param>
        /// <param name="_layer">The layer to add the VertexPositionColorTextureGroup object.</param>
        /// <returns>A unique ID for this entity.</returns>
        public int Add(VertexPositionColorTextureGroup _entity, int _layer)
        {
            int x = entityID.GetID();
            entityList.Add(x, _entity);
            AddLayers(_layer);
            layers[_layer].Add(x, _entity);

            return x;
        }

        /// <summary>
        /// Converts a model to a RendererEntity and starts tracking it
        /// </summary>
        /// <param name="_entity">The Model to insert</param>
        /// <param name="_position">The position to insert the Model</param>
        /// <param name="_layer">The layer to associate the entity with. Lower layer numbers are drawn first.</param>
        /// <returns>The unique entity id used to keep track of each entity</returns>
        public int Add(Model _entity, Vector3 _position, int _layer)
        {
            RendererAssetPool.UniversalEffect.RemapModel(_entity);
            int x = entityID.GetID();
            Object3 obj = new Object3(_entity, _position);
            entityList.Add(x, obj);
            AddLayers(_layer);
            layers[_layer].Add(x, obj);
            return x;
        }

        /// <summary>
        /// Makes sure that the layer the user is trying to add an entity to exists
        /// </summary>
        /// <param name="maxLayer">The layer number.</param>
        private void AddLayers(int _layer)
        {
            while (layers.Count <= _layer)
            {
                SceneLayer layer = new SceneLayer();
                layers.Add(layer);
            }
        }
        /// <summary>
        /// Removes a specific entity from Renderer
        /// </summary>
        /// <param name="_entityID">The id of the entity to remove</param>
        public bool Delete(int _entityID)
        {
            try
            {
                int i = 0;
                while (!layers[i].Delete(_entityID))
                {
                    i++;
                }
                entityList.Remove(_entityID);
                entityID.ReleaseID(_entityID);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false; 
            }

            return true;
        }

        /// <summary>
        /// Removes all entities from all layers
        /// </summary>
        public void ClearAll()
        {
            foreach(SceneLayer layer in layers)
            {
                layer.Clear();
            }
            entityList.Clear();
            drawableEntities.Clear();
            transparentEntities.Clear();

            entityID.ReleaseAll();
        }

        /// <summary>
        /// Clears all entities from a specific layer.
        /// </summary>
        /// <param name="layer">The layer to clear.</param>
        public void Clear(int layer)
        {
            if (layer < layers.Count)
            {
                List<int> list = layers[layer].Clear();

                foreach (int key in list)
                {
                    entityList.Remove(key);
                    entityID.ReleaseID(key);
                }
            }
        }

                
        #endregion // TODO: Add ClearAll()

        #region Entity Access

        /// <summary>
        /// Gets an entity via its unique ID.
        /// </summary>
        /// <param name="entityID">The ID of the entity you want to access.</param>
        /// <returns>The entity.</returns>
        public Entity Access(int entityID)
        {
            return entityList[entityID];
        }

        /// <summary>
        /// Gets an Object3 based on it's unique renderer ID. Note: the ID must correspond to an Object3.
        /// </summary>
        /// <param name="entityID">The ID of the Object3.</param>
        /// <returns>The Object3 object.</returns>
        public Object3 Access3D(int entityID)
        {
           return (Object3)entityList[entityID];
        }

        /// <summary>
        /// Gets a Camera object from this scene based on its unique name.
        /// </summary>
        /// <param name="cameraName">The name of the camera.</param>
        /// <returns>The Camera object.</returns>
        public Camera AccessCamera(string cameraName)
        {
            if(cameraDictionary.ContainsKey(cameraName))
                return cameraDictionary[cameraName];

            return null;
        }
        #endregion

        #region Movement

        #endregion


        
    }
}
